# post_model_process.py
import cv2
import numpy as np
import requests
from io import BytesIO

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

        # 关键修改：返回完整文件对象
        return True, overlay_file_obj, prob_file_obj

    except Exception as e:
        print(f"调用FileService上传图片失败: {str(e)}")
        return False, {}, {}  # 失败时返回空字典，避免后续取值报错