#！pip install "opencv-python<4.10"
# Python 3.10.10
import os
import glob
import cv2
import numpy as np
import torch
import requests
import hashlib  # 移到顶部，避免函数内局部导入（可选，更规范）
from albumentations.pytorch import ToTensorV2
import albumentations as A
from io import BytesIO
from PIL import Image

# ===================== 全局配置（只加载1次模型）=====================
print("Setting up inference environment...")
DEVICE = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
IMG_H, IMG_W = 256, 256

# 结果保存目录
base_dir = os.getcwd()
results_dir = os.path.join(base_dir, 'results')
os.makedirs(results_dir, exist_ok=True)
print(f"Results will be saved to: {results_dir}")

# 模型加载（全局只执行一次，避免重复加载）
def find_model_weights():
    candidates = [
        'best_stroke_model_with_normals.pth',
        os.path.join(base_dir, 'best_stroke_model_with_normals.pth'),
        os.path.join(base_dir, 'working', 'best_stroke_model_with_normals.pth'),
    ]
    for c in candidates:
        if os.path.isfile(c):
            return c
    search_roots = [
        base_dir,
        os.path.dirname(base_dir),
        os.path.join(base_dir, 'Stroke_segmentation_UNet'),
    ]
    for root in search_roots:
        for p in glob.glob(os.path.join(root, '**', 'best_stroke_model_with_normals.pth'), recursive=True):
            return p
    return None

print("Locating model weights...")
MODEL_WEIGHTS = find_model_weights()
if MODEL_WEIGHTS is None:
    raise FileNotFoundError("Could not locate 'best_stroke_model_with_normals.pth'. Please set MODEL_WEIGHTS manually.")
print(f"Using weights: {MODEL_WEIGHTS}")

# 构建模型
import segmentation_models_pytorch as smp
def build_model_offline():
    return smp.Unet(
        encoder_name='efficientnet-b4',
        encoder_weights=None,
        in_channels=3,
        classes=1,
        activation=None,
    )

# 全局模型，只加载一次
model = build_model_offline().to(DEVICE)
state = torch.load(MODEL_WEIGHTS, map_location=DEVICE)
model.load_state_dict(state)
model.eval()
print("Model loaded successfully.")

# 预处理transform
def get_transforms(is_training=False):
    return A.Compose([
        A.Normalize(mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225]),
        ToTensorV2(),
    ])
tform = get_transforms(False)

# ===================== 核心处理函数：传入图片URL，返回处理结果 =====================
def process_stroke_image(image_url):
    """
    脑卒中图像分割处理函数
    :param image_url: 网络图片URL / 本地图片路径
    :return: success(bool), overlay_path(str), prob_path(str)
    """
    try:
        # 1. 下载网络图片 / 读取本地图片
        if image_url.startswith(('http://', 'https://')):
            print(f"Downloading image from URL: {image_url}")
            resp = requests.get(image_url, timeout=20, verify=False)
            resp.raise_for_status()
            # 转OpenCV格式
            img_arr = np.asarray(Image.open(BytesIO(resp.content)).convert('RGB'))
            img_bgr = cv2.cvtColor(img_arr, cv2.COLOR_RGB2BGR)
            # 临时文件名（用时间戳/哈希避免重名）
            file_hash = hashlib.md5(image_url.encode()).hexdigest()
            stem = file_hash
        else:
            # 本地路径
            img_bgr = cv2.imread(image_url)
            if img_bgr is None:
                print(f"ERROR: 无法读取本地图片 {image_url}")
                return False, "", ""
            stem = os.path.splitext(os.path.basename(image_url))[0]

        # 2. 图像预处理
        rgb = cv2.cvtColor(img_bgr, cv2.COLOR_BGR2RGB)
        rgb_r = cv2.resize(rgb, (IMG_W, IMG_H), interpolation=cv2.INTER_LINEAR)

        # 3. 模型推理
        with torch.no_grad():
            x = tform(image=rgb_r, mask=np.zeros((IMG_H, IMG_W), np.float32))['image'].unsqueeze(0).to(DEVICE)
            logits = model(x)[0, 0].cpu().numpy()
            prob = 1 / (1 + np.exp(-logits))  # sigmoid
            pred_bin = (prob > 0.5).astype(np.uint8) * 255

        # 4. 生成叠加图
        overlay = rgb_r.copy()
        overlay[pred_bin > 0] = (255, 64, 64)

        # 5. 保存输出图片
        out_overlay = os.path.join(results_dir, f"{stem}_overlay.png")
        out_prob = os.path.join(results_dir, f"{stem}_prob.png")

        cv2.imwrite(out_overlay, cv2.cvtColor(overlay, cv2.COLOR_RGB2BGR))
        prob_u8 = (np.clip(prob, 0, 1) * 255).astype(np.uint8)
        cv2.imwrite(out_prob, prob_u8)

        print(f"处理完成！保存叠加图: {out_overlay}\n保存概率图: {out_prob}")
        return True, out_overlay, out_prob

    except Exception as e:
        print(f"图像处理失败: {str(e)}")
        return False, "", ""

# ===================== 原上传函数（保留，可调用）=====================
def post_outputs(url, overlay_path, prob_path, payload=None, timeout=15):
    if not url:
        return False
    try:
        with open(overlay_path, 'rb') as f1, open(prob_path, 'rb') as f2:
            files = {
                'overlay': (os.path.basename(overlay_path), f1, 'image/png'),
                'prob': (os.path.basename(prob_path), f2, 'image/png'),
            }
            resp = requests.post(url, data=payload or {}, files=files, timeout=timeout, verify=False)  # 加verify=False适配自签名证书
        ok = 200 <= resp.status_code < 300
        print(f"POST {url} -> {resp.status_code}")
        return ok
    except Exception as e:
        print(f"POST请求失败: {e}")
        return False
    
""" 
    # 测试入口（单独运行此文件时测试）
if __name__ == '__main__':
    # 测试用URL/本地路径
    test_img = "https://xxx.png"  # 替换为你的测试图
    success, overlay, prob = process_stroke_image(test_img)
    print(success, overlay, prob) """