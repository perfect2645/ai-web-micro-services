#Python 3.10.10


#！pip install "opencv-python<4.10"
# Override test image paths to use a specific image
TEST_IMAGE_PATHS = [r"D:\Web\image-ai\temp_images\10007.png"]
print("Using TEST_IMAGE_PATHS override:", TEST_IMAGE_PATHS)

# ==== Load trained model and predict brain images (save overlays) ====
import os, glob, cv2, numpy as np, torch
import matplotlib.pyplot as plt
try:
    import requests
except ImportError:
    requests = None
from albumentations.pytorch import ToTensorV2

print("Setting up inference environment...")
# Use existing helpers from the notebook: build_model(), get_transforms(False)
DEVICE = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
IMG_H, IMG_W = 256, 256

# Where to save visual outputs
base_dir = os.getcwd()
results_dir = os.path.join(base_dir, 'results')
os.makedirs(results_dir, exist_ok=True)
print(f"Results will be saved to: {results_dir}")

# --------- Find weights robustly ----------
def find_model_weights():
    candidates = [
        'best_stroke_model_with_normals.pth',
        os.path.join(base_dir, 'best_stroke_model_with_normals.pth'),
        os.path.join(base_dir, 'working', 'best_stroke_model_with_normals.pth'),
    ]
    for c in candidates:
        if os.path.isfile(c):
            return c
    # search upward and common folders
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

# --------- Build and load model (offline) ----------
import segmentation_models_pytorch as smp
def build_model_offline():
    return smp.Unet(
        encoder_name='efficientnet-b4',
        encoder_weights=None,  # avoid network download
        in_channels=3,
        classes=1,
        activation=None,
    )
model = build_model_offline().to(DEVICE)

state = torch.load(MODEL_WEIGHTS, map_location=DEVICE)
model.load_state_dict(state)
model.eval()
print("Model loaded.")
#################


import pika
import json

def post_outputs(url, overlay_path, prob_path, payload=None, timeout=15):
    if not url:
        return False
    if requests is None:
        print("HTTP post skipped: 'requests' not installed. Install with: pip install requests")
        return False
    data = payload or {}
    try:
        with open(overlay_path, 'rb') as f1, open(prob_path, 'rb') as f2:
            files = {
                'overlay': (os.path.basename(overlay_path), f1, 'image/png'),
                'prob': (os.path.basename(prob_path), f2, 'image/png'),
            }
            resp = requests.post(url, data=data, files=files, timeout=timeout)
        ok = 200 <= resp.status_code < 300
        print(f"POST {url} -> {resp.status_code}")
        if not ok:
            print(str(resp.text)[:500])
        return ok
    except Exception as e:
        print(f"POST request failed: {e}")
        return False
# --------- Collect test images ----------
def collect_test_images(max_count=6):
    
    paths = []
    # 1) Allow user-provided list via TEST_IMAGE_PATHS
    user_list = globals().get('TEST_IMAGE_PATHS', None)
    if isinstance(user_list, list) and len(user_list) > 0:
        for p in user_list:
            if isinstance(p, str) and os.path.isfile(p):
                paths.append(p)
        if len(paths) > 0:
            return paths[:max_count]
    # 2) Try External_Test PNG
    dataset_root = os.path.join(globals().get('config', None).DATASET_PATH if 'config' in globals() else base_dir, 'External_Test')
    png_dir = os.path.join(dataset_root, 'PNG')
    if os.path.isdir(png_dir):
        found = sorted(glob.glob(os.path.join(png_dir, '*.png')))
        if found:
            return found[:max_count]
    # 3) Try Bleeding/Ischemia/Normal PNGs under DATASET_PATH
    if 'config' in globals():
        for cls in ['Bleeding', 'Ischemia', 'Normal']:
            png_dir = os.path.join(config.DATASET_PATH, cls, 'PNG')
            if os.path.isdir(png_dir):
                found = sorted(glob.glob(os.path.join(png_dir, '*.png')))
                if found:
                    paths.extend(found[:max_count - len(paths)])
                    if len(paths) >= max_count:
                        break
    return paths

image_paths = collect_test_images(max_count=6)
if len(image_paths) == 0:
    raise FileNotFoundError("No test images found. Set TEST_IMAGE_PATHS = [<your_image.png>] before running.")
print(f"Found {len(image_paths)} test images.")

# --------- Transforms ----------
try:
    tform = get_transforms(False)
except NameError:
    import albumentations as A
    def get_transforms(is_training=True):
        return A.Compose([
            A.Normalize(mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225]),
            ToTensorV2(),
        ])
    tform = get_transforms(False)

# --------- Inference + overlay + save ----------
rows = len(image_paths)
fig, axs = plt.subplots(rows, 3, figsize=(12, 4*rows))
if rows == 1:
    axs = np.array([axs])

with torch.no_grad():
    for i, img_path in enumerate(image_paths):
        bgr = cv2.imread(img_path)
        if bgr is None:
            print(f"Could not read: {img_path}")
            continue
        rgb = cv2.cvtColor(bgr, cv2.COLOR_BGR2RGB)
        rgb_r = cv2.resize(rgb, (IMG_W, IMG_H), interpolation=cv2.INTER_LINEAR)

        x = tform(image=rgb_r, mask=np.zeros((IMG_H, IMG_W), np.float32))['image'].unsqueeze(0).to(DEVICE)
        logits = model(x)[0,0].cpu().numpy()
        prob = 1 / (1 + np.exp(-logits))  # sigmoid
        pred_bin = (prob > 0.5).astype(np.uint8) * 255

        # Red overlay on positives
        overlay = rgb_r.copy()
        overlay[pred_bin > 0] = (255, 64, 64)

        # Show
        axs[i,0].imshow(rgb_r); axs[i,0].set_title(os.path.basename(img_path)); axs[i,0].axis('off')
        axs[i,1].imshow(prob, cmap='gray'); axs[i,1].set_title('Prediction (prob)'); axs[i,1].axis('off')
        axs[i,2].imshow(overlay); axs[i,2].set_title('Overlay'); axs[i,2].axis('off')

        # Save outputs
        stem = os.path.splitext(os.path.basename(img_path))[0]
        out_overlay = os.path.join(results_dir, f"{stem}_overlay.png")
        out_prob = os.path.join(results_dir, f"{stem}_prob.png")
        cv2.imwrite(out_overlay, cv2.cvtColor(overlay, cv2.COLOR_RGB2BGR))
        # save prob as grayscale 0-255
        prob_u8 = (np.clip(prob, 0, 1)*255).astype(np.uint8)
        cv2.imwrite(out_prob, prob_u8)
        print(f"Saved: {out_overlay}\nSaved: {out_prob}")
        # optionally POST the results to a server if POST_URL is set
        post_url = os.getenv('POST_URL')
        if post_url:
            payload = {'image': stem, 'status': 'done'}
            post_outputs(post_url, out_overlay, out_prob, payload)

plt.tight_layout(); plt.show()
