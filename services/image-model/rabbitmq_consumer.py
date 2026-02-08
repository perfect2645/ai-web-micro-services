import pika
import json
import os
import uuid
from datetime import datetime
from PIL import Image
import requests
from io import BytesIO

# ====================== RabbitMQ配置（保持原有） ======================
RABBITMQ_SETTINGS = {
    "HostName": "192.168.60.128",
    "Port": 5672,
    "UserName": "admin",
    "Password": "Asdf@1234",
    "VirtualHost": "/dev",
    "ExchangeName": "doraemon.exchange",
    "RoutingKey": "doraemon.topic",  # Topic路由键
    "QueueName": "image_process_topic_queue"
}

# ====================== 图片处理相关配置 ======================
# 本地临时存储路径（下载图片/处理后图片）
TEMP_IMAGE_DIR = "./temp_images"
# 处理后图片的基础URL（根据你的实际存储服务调整，如OSS/MinIO地址）
OUTPUT_IMAGE_BASE_URL = "http://192.168.60.128:8080/images/"

# 初始化临时目录
os.makedirs(TEMP_IMAGE_DIR, exist_ok=True)

def download_image_from_url(image_url, save_path):
    """从URL下载图片到本地临时路径"""
    try:
        # 发送GET请求获取图片（添加超时和重试）
        response = requests.get(
            image_url,
            timeout=30,
            headers={"User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64)"}
        )
        response.raise_for_status()  # 抛出HTTP错误
        
        # 保存图片到本地
        with open(save_path, 'wb') as f:
            f.write(response.content)
        
        print(f"✅ 图片下载完成：{image_url} -> {save_path}")
        return True
    except Exception as e:
        print(f"❌ 图片下载失败：{str(e)}")
        return False

def process_image(task_id, input_image_url, prompt_text):
    """
    核心图片处理函数（适配新payload）
    :param task_id: 任务唯一ID（doraemonItem.id）
    :param input_image_url: 待处理图片URL
    :param prompt_text: 处理提示文本（如尺寸、风格等）
    :return: 处理结果（success, output_image_id, output_image_url, error_msg）
    """
    # 1. 生成临时文件路径
    input_image_path = os.path.join(TEMP_IMAGE_DIR, f"{task_id}_input.jpg")
    output_image_path = os.path.join(TEMP_IMAGE_DIR, f"{task_id}_output.jpg")
    
    try:
        # 2. 下载图片
        if not download_image_from_url(input_image_url, input_image_path):
            raise Exception(f"图片下载失败：{input_image_url}")
        
        # 3. 解析promptText获取处理指令（示例：从prompt中提取目标尺寸，格式如"width=800,height=600"）
        target_width, target_height = 800, 600  # 默认尺寸
        if prompt_text and "," in prompt_text:
            for item in prompt_text.split(","):
                if "width=" in item:
                    target_width = int(item.split("=")[1].strip())
                if "height=" in item:
                    target_height = int(item.split("=")[1].strip())
        
        # 4. 执行图片处理（缩放为例，可扩展为AI风格转换等）
        with Image.open(input_image_path) as img:
            # 等比例缩放
            img.thumbnail((target_width, target_height))
            # 保存处理后图片
            img.save(output_image_path, quality=90)
        
        # 5. 生成输出图片的ID和URL（模拟上传到文件服务，替换为你的实际存储逻辑）
        output_image_id = str(uuid.uuid4())  # 生成新的UUID
        output_image_url = f"{OUTPUT_IMAGE_BASE_URL}{output_image_id}.jpg"
        
        # （可选）实际项目中需将output_image_path上传到存储服务，然后删除本地临时文件
        # upload_to_oss(output_image_path, output_image_id)
        # os.remove(input_image_path)
        # os.remove(output_image_path)
        
        print(f"✅ 图片处理完成：任务ID={task_id}，输出URL={output_image_url}")
        return True, output_image_id, output_image_url, ""
    
    except Exception as e:
        error_msg = str(e)
        print(f"❌ 图片处理失败：任务ID={task_id}，错误={error_msg}")
        return False, "", "", error_msg

def callback(ch, method, properties, body):
    """Topic消息回调函数：解析新payload并处理"""
    try:
        # 1. 解析完整的JSON消息payload
        payload = json.loads(body.decode('utf-8'))
        print(f"\n📥 收到Topic消息（路由键：{method.routing_key}）：")
        print(f"   消息主题：{payload.get('topic')}")
        print(f"   消息来源：{payload.get('source')}")
        
        # 2. 提取核心的doraemonItem（校验是否存在）
        doraemon_item = payload.get("doraemonItem")
        if not doraemon_item:
            raise Exception("消息体中缺少doraemonItem字段")
        
        # 3. 提取doraemonItem中的关键字段（添加空值校验）
        task_id = doraemon_item.get("id")
        input_image_url = doraemon_item.get("inputImageUrl")
        prompt_text = doraemon_item.get("promptText", "")
        user_id = doraemon_item.get("userId")
        
        # 基础字段校验
        if not task_id:
            raise Exception("doraemonItem.id 不能为空")
        if not input_image_url:
            raise Exception("doraemonItem.inputImageUrl 不能为空")
        if not user_id:
            raise Exception("doraemonItem.userId 不能为空")
        
        print(f"   任务ID：{task_id}")
        print(f"   用户ID：{user_id}")
        print(f"   输入图片URL：{input_image_url}")
        print(f"   处理提示：{prompt_text}")
        
        # 4. 执行图片处理
        success, output_image_id, output_image_url, error_msg = process_image(
            task_id=task_id,
            input_image_url=input_image_url,
            prompt_text=prompt_text
        )
        
        # 5. 更新doraemonItem的状态和结果（可发送回调通知WebAPI）
        doraemon_item["updateTime"] = datetime.utcnow().isoformat() + "Z"  # 符合ISO格式
        if success:
            doraemon_item["status"] = "Success"
            doraemon_item["outputImageId"] = output_image_id
            doraemon_item["outputImageUrl"] = output_image_url
            doraemon_item["errorMessage"] = ""
            # 手动确认消息（处理成功）
            ch.basic_ack(delivery_tag=method.delivery_tag)
            print(f"✅ 任务{task_id}处理完成，已确认消息")
        else:
            doraemon_item["status"] = "Failed"
            doraemon_item["errorMessage"] = error_msg
            # 处理失败：拒绝消息并重新入队（可根据需求调整重试次数）
            ch.basic_nack(delivery_tag=method.delivery_tag, requeue=True)
            print(f"❌ 任务{task_id}处理失败，消息重新入队")
        
        # （可选）将更新后的doraemonItem发送到回调队列，通知WebAPI处理结果
        # send_callback_to_webapi(doraemon_item)

    except Exception as e:
        error_msg = f"解析/处理消息失败：{str(e)}"
        print(f"❌ {error_msg}")
        # 避免死循环：失败后不再重新入队
        ch.basic_nack(delivery_tag=method.delivery_tag, requeue=False)

def start_topic_consumer():
    """启动RabbitMQ Topic模式消费者（保持原有逻辑）"""
    try:
        # 1. 创建连接参数（包含认证信息）
        credentials = pika.PlainCredentials(
            username=RABBITMQ_SETTINGS["UserName"],
            password=RABBITMQ_SETTINGS["Password"]
        )
        parameters = pika.ConnectionParameters(
            host=RABBITMQ_SETTINGS["HostName"],
            port=RABBITMQ_SETTINGS["Port"],
            virtual_host=RABBITMQ_SETTINGS["VirtualHost"],
            credentials=credentials,
            connection_attempts=3,
            retry_delay=5
        )

        # 2. 建立连接和通道
        connection = pika.BlockingConnection(parameters)
        channel = connection.channel()

        # 3. 声明Topic类型的交换机
        channel.exchange_declare(
            exchange=RABBITMQ_SETTINGS["ExchangeName"],
            exchange_type='topic',
            durable=True
        )

        # 4. 声明队列并绑定到交换机
        channel.queue_declare(
            queue=RABBITMQ_SETTINGS["QueueName"],
            durable=True
        )
        channel.queue_bind(
            queue=RABBITMQ_SETTINGS["QueueName"],
            exchange=RABBITMQ_SETTINGS["ExchangeName"],
            routing_key=RABBITMQ_SETTINGS["RoutingKey"]
        )

        # 5. 设置QoS并启动消费
        channel.basic_qos(prefetch_count=1)
        channel.basic_consume(
            queue=RABBITMQ_SETTINGS["QueueName"],
            on_message_callback=callback,
            auto_ack=False
        )

        print(f"🚀 Python Topic消费者已启动：")
        print(f"   交换机：{RABBITMQ_SETTINGS['ExchangeName']}")
        print(f"   路由键：{RABBITMQ_SETTINGS['RoutingKey']}")
        print(f"   监听队列：{RABBITMQ_SETTINGS['QueueName']}")
        print("   等待消息...（按Ctrl+C停止）")

        channel.start_consuming()

    except KeyboardInterrupt:
        print("\n🛑 消费者被手动停止")
    except Exception as e:
        print(f"❌ 消费者启动失败：{str(e)}")
        # 重连逻辑
        start_topic_consumer()

# （可选）回调函数：将处理结果通知WebAPI
# def send_callback_to_webapi(updated_doraemon_item):
#     callback_url = "http://192.168.60.128:5000/api/image/callback"
#     try:
#         requests.post(callback_url, json=updated_doraemon_item, timeout=10)
#     except Exception as e:
#         print(f"❌ 回调WebAPI失败：{str(e)}")

if __name__ == "__main__":
    # 安装依赖：pip install pika pillow requests
    start_topic_consumer()