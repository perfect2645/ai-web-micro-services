# post_model_process.py
import cv2
import numpy as np
import requests
import json
from io import BytesIO
from datetime import datetime

# ====================== 新增：服务接口配置 ======================
# DomainService更新doraemon对象的API
DOMAIN_SERVICE_UPDATE_URL = "https://localhost:7093/api/doraemon"
# MessagingService通知前端的SignalR API
MESSAGING_SERVICE_NOTIFY_URL = "https://localhost:7094/api/signalrmessaging/send"

# FileService配置
FILE_SERVICE_API = "https://localhost:7092/api/file"

def post_process(overlay_rgb, prob_array, file_stem):
    """
    调用FileService保存图片（替代本地保存）
    :param overlay_rgb: 叠加图的RGB数组（模型处理后的结果）
    :param prob_array: 概率图的numpy数组（0-1范围）
    :param file_stem: 文件名前缀（用于生成唯一文件名）
    :return: success(bool), overlay_file_obj(dict), prob_file_obj(dict)
    """
    try:
        # ========== 处理叠加图：编码为字节流并上传 ==========
        # 1. RGB转BGR（CV2默认格式）
        overlay_bgr = cv2.cvtColor(overlay_rgb, cv2.COLOR_RGB2BGR)
        # 2. 编码为PNG字节流（不落地保存）
        _, overlay_png = cv2.imencode(".png", overlay_bgr)
        overlay_stream = BytesIO(overlay_png.tobytes())
        overlay_filename = f"{file_stem}_overlay.png"

        # 3. 调用FileService上传叠加图
        overlay_resp = requests.post(
            FILE_SERVICE_API,
            files={"file": (overlay_filename, overlay_stream, "image/png")},  # 标准multipart/form-data格式
            verify=False,  # 适配本地自签名证书（生产环境替换为证书路径）
            timeout=30
        )
        overlay_resp.raise_for_status()  # 非200响应抛出异常
        # 解析完整的文件对象
        overlay_file_obj = overlay_resp.json()
        # 校验核心字段（确保返回对象有效）
        if not overlay_file_obj.get("remoteUrl") or not overlay_file_obj.get("id"):
            raise Exception("FileService返回的叠加图对象缺少核心字段（remoteUrl/id）")

        # ========== 处理概率图：编码为字节流并上传 ==========
        # 1. 概率数组转8位灰度图
        prob_u8 = (np.clip(prob_array, 0, 1) * 255).astype(np.uint8)
        # 2. 编码为PNG字节流
        _, prob_png = cv2.imencode(".png", prob_u8)
        prob_stream = BytesIO(prob_png.tobytes())
        prob_filename = f"{file_stem}_prob.png"

        # 3. 调用FileService上传概率图
        prob_resp = requests.post(
            FILE_SERVICE_API,
            files={"file": (prob_filename, prob_stream, "image/png")},
            verify=False,
            timeout=30
        )
        prob_resp.raise_for_status()
        # 解析完整的文件对象
        prob_file_obj = prob_resp.json()
        # 校验核心字段
        if not prob_file_obj.get("remoteUrl") or not prob_file_obj.get("id"):
            raise Exception("FileService返回的概率图对象缺少核心字段（remoteUrl/id）")

        # 返回完整文件对象
        return True, overlay_file_obj, prob_file_obj

    except Exception as e:
        print(f"调用FileService上传图片失败: {str(e)}")
        return False, {}, {}  # 失败时返回空字典，避免后续取值报错

# ====================== 新增：DomainService更新接口 ======================
def call_domain_service_update(updated_doraemon_item):
    """
    调用DomainService的Update API更新doraemon对象（修复400排查+调试器异常）
    :param updated_doraemon_item: 已更新的doraemonItem字典
    :return: success(bool), response_data(dict)
    """
    response_data = {}
    try:
        print(f"📤 调用DomainService更新doraemon对象：ID={updated_doraemon_item.get('id')}")
        # 打印最终发送的请求体（关键！对比Swagger）
        print(f"请求体：\n{json.dumps(updated_doraemon_item, ensure_ascii=False, indent=2)}")
        
        resp = requests.put(
            url=DOMAIN_SERVICE_UPDATE_URL,
            json=updated_doraemon_item,
            verify=False,  # 适配自签名证书
            timeout=30
        )
        
        # 先记录响应状态和内容，再判断是否抛异常（避免调试器拦截）
        status_code = resp.status_code
        response_text = resp.text.strip() if resp.text else "无响应内容"
        
        if status_code >= 400:
            raise Exception(f"HTTP {status_code}: {response_text}")
        
        # 兼容204无响应体
        if status_code == 204:
            response_data = {"status": "success", "message": "更新成功（无响应体）"}
        else:
            response_data = resp.json() if response_text else {}
        
        print(f"✅ DomainService更新成功：{response_data}")
        return True, response_data
    
    except Exception as e:
        # 修复：直接捕获通用异常，避免调试器解析HTTPError的特殊属性
        error_msg = f"DomainService更新失败：{str(e)}"
        print(f"❌ {error_msg}")
        # 打印完整的请求体和响应，定位400原因
        print(f"❌ 触发400的请求体：\n{json.dumps(updated_doraemon_item, ensure_ascii=False, indent=2)}")
        return False, {"error": error_msg}
# ====================== 新增：MessagingService通知接口 ======================
def call_messaging_service_notify(doraemon_message):
    try:
        print(f"📤 调用MessagingService的请求体：\n{json.dumps(doraemon_message, ensure_ascii=False, indent=2)}")
        resp = requests.post(
            url=MESSAGING_SERVICE_NOTIFY_URL,
            json=doraemon_message,
            verify=False,
            timeout=30
        )
        resp.raise_for_status()
        
        # 兼容空响应体
        if resp.status_code == 204:
            response_data = {"status": "success", "message": "通知成功（无响应体）"}
        else:
            response_data = resp.json() if resp.content.strip() else {}
        
        print(f"✅ MessagingService通知成功：{response_data}")
        return True, response_data
    
    except requests.exceptions.HTTPError as e:
        error_content = resp.text if resp.content else "无返回内容"
        error_msg = f"MessagingService通知失败（状态码{resp.status_code}）：{str(e)}，返回内容：{error_content}"
        print(f"❌ {error_msg}")
        return False, {"error": error_msg}
    
    except Exception as e:
        error_msg = f"MessagingService通知异常：{str(e)}"
        print(f"❌ {error_msg}")
        return False, {"error": error_msg}

# ====================== 新增：统一回调函数（封装两个接口调用） ======================
def send_doraemon_callback(original_payload, updated_doraemon_item):
    """
    统一回调入口：先更新DomainService，再通知前端
    :param original_payload: 原始消息payload（用于提取topic/source）
    :param updated_doraemon_item: 更新后的doraemonItem
    :return: 无（仅打印日志，不阻断主流程）
    """
    try:
        # 1. 调用DomainService更新doraemon对象
        domain_success, _ = call_domain_service_update(updated_doraemon_item)
        
        # 2. 构造doraemonMessage（匹配前端通知格式）
        doraemon_message = {
            "topic": original_payload.get("topic", "doraemon.topic"),  # 沿用原消息topic或默认
            "doraemonItem": updated_doraemon_item,
            "source": "python.image.process" # 沿用原消息source或默认
        }
        
        # 3. 调用MessagingService通知前端
        messaging_success, _ = call_messaging_service_notify(doraemon_message)
        
        # 4. 打印整体回调结果
        if domain_success and messaging_success:
            print(f"✅ 回调完成：DomainService更新成功 + MessagingService通知成功")
        else:
            print(f"⚠️  回调部分失败：DomainService={domain_success}, MessagingService={messaging_success}")
    except Exception as e:
        print(f"❌ 统一回调执行异常：{str(e)}")