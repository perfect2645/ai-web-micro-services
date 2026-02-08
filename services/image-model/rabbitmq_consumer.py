import pika
import json
import os
import uuid
from datetime import datetime
import requests  # 保留，用于后续可能的回调
from io import BytesIO

# ====================== 新增：导入脑卒中图像处理函数 ======================
# 确保 stroke_segmentation_U_net_load_model_Post.py 和本文件在同一目录
from stroke_segmentation_U_net_load_model_Post import process_stroke_image, post_outputs

# ====================== 全局禁用SSL警告 ======================
from urllib3.exceptions import InsecureRequestWarning
# 全局禁用：所有requests请求的InsecureRequestWarning都不会显示
requests.packages.urllib3.disable_warnings(InsecureRequestWarning)

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

# ====================== 图片处理相关配置（适配脑卒中处理） ======================
# 本地临时存储路径（下载图片/处理后图片）
TEMP_IMAGE_DIR = "./temp_images"
# 脑卒中处理结果保存目录（和分割脚本中的results_dir保持一致）
STROKE_RESULT_DIR = "./results"
# 处理后图片的基础URL（根据你的实际存储服务调整，如OSS/MinIO地址）
OUTPUT_IMAGE_BASE_URL = "http://192.168.60.128:8080/images/"

# 初始化目录
os.makedirs(TEMP_IMAGE_DIR, exist_ok=True)
os.makedirs(STROKE_RESULT_DIR, exist_ok=True)

def process_image(task_id, input_image_url, prompt_text):
    """
    核心图片处理函数（替换为脑卒中图像分割逻辑）
    :param task_id: 任务唯一ID（doraemonItem.id）
    :param input_image_url: 消息中的图片URL
    :param prompt_text: 处理提示文本（保留，兼容原有参数）
    :return: 处理结果（success, output_image_id, output_image_url, error_msg）
    """
    try:
        print(f"📌 开始处理脑卒中分割任务 {task_id}, 图片URL: {input_image_url}")
        
        # 调用脑卒中分割核心函数，传入图片URL（函数内部自动下载+推理）
        success, overlay_path, prob_path = process_stroke_image(input_image_url)
        
        if not success:
            return False, "", "", "脑卒中图像分割模型执行失败"
        
        # 1. 生成输出图片ID（保持原有UUID逻辑）
        output_image_id = str(uuid.uuid4())
        
        # 2. 生成输出图片URL（适配你的存储服务）
        # 示例1：本地文件映射URL（需配置web服务器指向STROKE_RESULT_DIR）
        overlay_filename = os.path.basename(overlay_path)
        output_image_url = f"{OUTPUT_IMAGE_BASE_URL}{overlay_filename}"
        
        # （可选）示例2：如果需要上传到OSS/MinIO，取消下面注释并实现upload_to_storage函数
        # output_image_url = upload_to_storage(overlay_path, output_image_id)
        
        # （可选）如果需要POST结果到指定URL，取消下面注释
        # post_url = os.getenv('POST_URL', 'http://192.168.60.128:8080/api/result')
        # post_outputs(post_url, overlay_path, prob_path, {"task_id": task_id, "user_id": user_id})
        
        print(f"✅ 脑卒中分割完成：任务ID={task_id}，叠加图路径={overlay_path}，输出URL={output_image_url}")
        return True, output_image_id, output_image_url, ""
    
    except Exception as e:
        error_msg = f"脑卒中分割处理异常: {str(e)}"
        print(f"❌ 任务{task_id}处理失败：{error_msg}")
        return False, "", "", error_msg

def callback(ch, method, properties, body):
    """Topic消息回调函数：保持原有逻辑，仅适配新的process_image返回值"""
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
        prompt_text = doraemon_item.get("promptText", "")  # 保留，兼容原有参数
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
        
        # 4. 执行脑卒中图像分割处理（替换原有缩放逻辑）
        success, output_image_id, output_image_url, error_msg = process_image(
            task_id=task_id,
            input_image_url=input_image_url,
            prompt_text=prompt_text
        )
        
        # 5. 更新doraemonItem的状态和结果（完全保留原有逻辑）
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
            retry_count = int(properties.headers.get('x-retry-count', 0))
            max_retry = 3  # 最多重试3次
            
            if retry_count < max_retry:
                # 重试次数+1，重新入队
                new_headers = properties.headers or {}
                new_headers['x-retry-count'] = retry_count + 1
                # 重新发布消息到队列（带更新的headers）
                ch.basic_publish(
                    exchange=RABBITMQ_SETTINGS["ExchangeName"],
                    routing_key=method.routing_key,
                    body=body,
                    properties=pika.BasicProperties(headers=new_headers)
                )
                print(f"❌ 任务{task_id}处理失败，重试次数{retry_count+1}/{max_retry}，消息重新入队")
            else:
                # 超过重试次数，丢弃消息
                print(f"❌ 任务{task_id}处理失败，已超过最大重试次数{max_retry}，消息丢弃")
            
            # 无论是否重试，都要nack原消息（requeue=False，避免重复）
            ch.basic_nack(delivery_tag=method.delivery_tag, requeue=False)
        
        # （可选）将更新后的doraemonItem发送到回调队列，通知WebAPI处理结果
        # send_callback_to_webapi(doraemon_item)

    except Exception as e:
        error_msg = f"解析/处理消息失败：{str(e)}"
        print(f"❌ {error_msg}")
        # 避免死循环：失败后不再重新入队
        ch.basic_nack(delivery_tag=method.delivery_tag, requeue=False)

# ====================== 以下代码完全保留原有逻辑 ======================
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

        print(f"🚀 Python Topic消费者已启动（脑卒中分割版）：")
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

# （可选）回调函数：将处理结果通知WebAPI（保留原有注释）
# def send_callback_to_webapi(updated_doraemon_item):
#     callback_url = "http://192.168.60.128:5000/api/image/callback"
#     try:
#         requests.post(callback_url, json=updated_doraemon_item, timeout=10)
#     except Exception as e:
#         print(f"❌ 回调WebAPI失败：{str(e)}")

# （可选）文件上传函数（如需上传到OSS/MinIO，实现此函数）
# def upload_to_storage(local_file_path, file_id):
#     """将本地处理后的图片上传到存储服务，返回访问URL"""
#     # 示例：上传到MinIO/OSS
#     # client = Minio(...)
#     # client.fput_object("bucket-name", f"{file_id}.png", local_file_path)
#     # return f"{OUTPUT_IMAGE_BASE_URL}{file_id}.png"
#     pass

if __name__ == "__main__":
    # 安装依赖：pip install pika pillow requests opencv-python<4.10 segmentation-models-pytorch torch albumentations
    start_topic_consumer()