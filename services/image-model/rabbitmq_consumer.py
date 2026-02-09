import pika
import json
import os
import uuid  # 保留但不再使用（无需手动生成图片ID）
from datetime import datetime
import requests
from io import BytesIO

# ====================== 新增：导入回调函数 ======================
from stroke_segmentation_U_net_load_model_Post import process_stroke_image, post_outputs
from post_model_process import send_doraemon_callback  # 导入统一回调函数

# ====================== 全局禁用SSL警告 ======================
from urllib3.exceptions import InsecureRequestWarning
requests.packages.urllib3.disable_warnings(InsecureRequestWarning)

# ====================== RabbitMQ配置（保持原有） ======================
RABBITMQ_SETTINGS = {
    "HostName": "192.168.60.128",
    "Port": 5672,
    "UserName": "admin",
    "Password": "Asdf@1234",
    "VirtualHost": "/dev",
    "ExchangeName": "doraemon.exchange",
    "RoutingKey": "doraemon.topic",
    "QueueName": "image_process_topic_queue"
}

def process_image(task_id, input_image_url, prompt_text):
    """
    核心图片处理函数（适配接收图片实体对象，不再手动生成图片ID）
    :param task_id: 任务唯一ID
    :param input_image_url: 消息中的图片URL
    :param prompt_text: 兼容原有参数
    :return: success, output_image_id, output_image_url, error_msg（从实体对象提取）
    """
    try:
        print(f"📌 开始处理脑卒中分割任务 {task_id}, 图片URL: {input_image_url}")
        
        # 1. 调用分割函数：接收返回的【图片实体对象】（而非路径/URL）
        success, overlay_file_obj, prob_file_obj = process_stroke_image(input_image_url)
        
        if not success:
            return False, "", "", "脑卒中图像分割模型执行失败"
        
        # 2. 从返回的图片实体对象中提取ID和URL（不再手动生成UUID）
        output_image_id = overlay_file_obj.get("id", "")  # 使用FileService返回的图片ID
        output_image_url = overlay_file_obj.get("remoteUrl", "")  # 使用FileService返回的访问URL
        
        # 校验核心字段（确保实体对象有效）
        if not output_image_id:
            raise Exception("图片实体对象中缺少id字段")
        if not output_image_url:
            raise Exception("图片实体对象中缺少remoteUrl字段")
        
        # 打印实体对象关键信息（便于调试）
        print(f"✅ 脑卒中分割完成：")
        print(f"   任务ID={task_id}")
        print(f"   图片ID={output_image_id}")
        print(f"   图片访问URL={output_image_url}")
        print(f"   完整叠加图实体对象：{json.dumps(overlay_file_obj, ensure_ascii=False, indent=2)}")
        
        return True, output_image_id, output_image_url, ""
    
    except Exception as e:
        error_msg = f"脑卒中分割处理异常: {str(e)}"
        print(f"❌ 任务{task_id}处理失败：{error_msg}")
        return False, "", "", error_msg

def callback(ch, method, properties, body):
    """Topic消息回调函数：新增调用统一回调函数"""
    try:
        # 1. 解析消息payload（保留原始payload用于回调）
        payload = json.loads(body.decode('utf-8'))
        original_payload = payload  # 保存原始payload，用于构造回调消息
        print(f"\n📥 收到Topic消息（路由键：{method.routing_key}）：")
        print(f"   消息主题：{payload.get('topic')}")
        print(f"   消息来源：{payload.get('source')}")
        
        # 2. 提取doraemonItem并校验
        doraemon_item = payload.get("doraemonItem")
        if not doraemon_item:
            raise Exception("消息体中缺少doraemonItem字段")
        
        # 3. 提取关键字段并校验
        task_id = doraemon_item.get("id")
        input_image_url = doraemon_item.get("inputImageUrl")
        prompt_text = doraemon_item.get("promptText", "")
        user_id = doraemon_item.get("userId")
        
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
        
        # 4. 执行分割处理（接收从实体对象提取的ID/URL）
        success, output_image_id, output_image_url, error_msg = process_image(
            task_id=task_id,
            input_image_url=input_image_url,
            prompt_text=prompt_text
        )
        
        # 5. 更新doraemonItem（使用返回的图片ID，无需手动生成）
        updated_doraemon_item = doraemon_item.copy()  # 复制原对象，避免修改原始数据
        updated_doraemon_item["updateTime"] = datetime.utcnow().isoformat() + "Z"
        if success:
            updated_doraemon_item["status"] = 3 # Succeeded
            updated_doraemon_item["outputImageId"] = output_image_id  # 用FileService返回的ID
            updated_doraemon_item["outputImageUrl"] = output_image_url  # 用FileService返回的URL
            updated_doraemon_item["errorMessage"] = ""
            ch.basic_ack(delivery_tag=method.delivery_tag)
            print(f"✅ 任务{task_id}处理完成，已确认消息")
            
            # ========== 新增：调用统一回调函数 ==========
            send_doraemon_callback(original_payload, updated_doraemon_item)
        else:
            # 重试逻辑（保持原有）
            retry_count = int(properties.headers.get('x-retry-count', 0))
            max_retry = 3
            
            if retry_count < max_retry:
                new_headers = properties.headers or {}
                new_headers['x-retry-count'] = retry_count + 1
                ch.basic_publish(
                    exchange=RABBITMQ_SETTINGS["ExchangeName"],
                    routing_key=method.routing_key,
                    body=body,
                    properties=pika.BasicProperties(headers=new_headers)
                )
                print(f"❌ 任务{task_id}处理失败，重试次数{retry_count+1}/{max_retry}，消息重新入队")
            else:
                print(f"❌ 任务{task_id}处理失败，已超过最大重试次数{max_retry}，消息丢弃")
                # 失败时也更新状态并回调（可选）
                updated_doraemon_item["status"] = "Failed"
                updated_doraemon_item["errorMessage"] = error_msg
                send_doraemon_callback(original_payload, updated_doraemon_item)
            
            ch.basic_nack(delivery_tag=method.delivery_tag, requeue=False)

    except Exception as e:
        error_msg = f"解析/处理消息失败：{str(e)}"
        print(f"❌ {error_msg}")
        ch.basic_nack(delivery_tag=method.delivery_tag, requeue=False)

def start_topic_consumer():
    """启动RabbitMQ消费者（保持原有逻辑不变）"""
    try:
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

        connection = pika.BlockingConnection(parameters)
        channel = connection.channel()

        channel.exchange_declare(
            exchange=RABBITMQ_SETTINGS["ExchangeName"],
            exchange_type='topic',
            durable=True
        )

        channel.queue_declare(
            queue=RABBITMQ_SETTINGS["QueueName"],
            durable=True
        )
        channel.queue_bind(
            queue=RABBITMQ_SETTINGS["QueueName"],
            exchange=RABBITMQ_SETTINGS["ExchangeName"],
            routing_key=RABBITMQ_SETTINGS["RoutingKey"]
        )

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
        start_topic_consumer()

if __name__ == "__main__":
    # 安装依赖：pip install pika pillow requests opencv-python<4.10 segmentation-models-pytorch torch albumentations
    start_topic_consumer()