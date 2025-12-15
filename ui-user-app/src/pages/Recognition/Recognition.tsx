import { useState, useRef } from 'react';
import type { RecognitionResult } from '../../types/types';
import ProgressBar from '../../components/ui/ProgressBar/ProgressBar';

interface RecognitionProps {
  settings: {
    maxUploadSize: number;
    allowedFormats: string[];
    recognitionTimeout: number;
  };
  onRecognitionComplete: (result: RecognitionResult) => void;
}

const Recognition = ({ settings, onRecognitionComplete }: RecognitionProps) => {
  const [selectedImage, setSelectedImage] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  const [progress, setProgress] = useState(0);
  const [isRecognizing, setIsRecognizing] = useState(false);
  const [recognitionResult, setRecognitionResult] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  // 处理图片选择
  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // 验证文件大小
    if (file.size > settings.maxUploadSize * 1024 * 1024) {
      alert(`文件大小不能超过${settings.maxUploadSize}MB`);
      return;
    }

    // 验证文件格式
    const fileType = file.type.split('/')[1];
    if (!settings.allowedFormats.includes(fileType)) {
      alert(`仅支持${settings.allowedFormats.join(', ')}格式`);
      return;
    }

    setSelectedImage(file);
    setImagePreview(URL.createObjectURL(file));
    setRecognitionResult(null);
  };

  // 触发文件选择
  const handleUploadClick = () => {
    fileInputRef.current?.click();
  };

  // 模拟图片识别
  const handleRecognize = () => {
    if (!selectedImage) return;

    setIsRecognizing(true);
    setProgress(0);

    // 模拟进度更新
    const progressInterval = setInterval(() => {
      setProgress((prev) => {
        if (prev >= 100) {
          clearInterval(progressInterval);
          return 100;
        }
        return prev + 10;
      });
    }, 300);

    // 模拟识别完成
    setTimeout(() => {
      clearInterval(progressInterval);
      setProgress(100);
      setIsRecognizing(false);
      
      // 模拟识别结果
      const result = `识别结果：这是一张${selectedImage.type.split('/')[1]}格式图片，内容为${Math.random() > 0.5 ? '风景' : '人物'}`;
      setRecognitionResult(result);

      // 保存到历史记录
      const recognitionRecord: RecognitionResult = {
        id: Date.now().toString(),
        imageUrl: imagePreview!,
        result,
        timestamp: Date.now(),
        status: 'success',
      };
      onRecognitionComplete(recognitionRecord);
    }, settings.recognitionTimeout * 1000);
  };

  return (
    <div className="card">
      <h2 style={{ marginBottom: '20px' }}>图片识别</h2>
      
      {/* 上传区域 */}
      <div className="upload-area" onClick={handleUploadClick}>
        <input
          type="file"
          ref={fileInputRef}
          onChange={handleFileChange}
          accept={settings.allowedFormats.map(format => `.${format}`).join(',')}
          style={{ display: 'none' }}
        />
        {!imagePreview ? (
          <div>
            <h3>点击上传图片</h3>
            <p>支持格式：{settings.allowedFormats.join(', ')} | 最大大小：{settings.maxUploadSize}MB</p>
          </div>
        ) : (
          <img
            src={imagePreview}
            alt="预览"
            style={{ maxWidth: '100%', maxHeight: '400px', borderRadius: '8px' }}
          />
        )}
      </div>

      {/* 识别按钮 */}
      {imagePreview && (
        <button
          className="btn"
          onClick={handleRecognize}
          disabled={isRecognizing}
          style={{ marginBottom: '20px' }}
        >
          {isRecognizing ? '识别中...' : '开始识别'}
        </button>
      )}

      {/* 进度条 */}
      {isRecognizing && <ProgressBar progress={progress} />}

      {/* 识别结果 */}
      {recognitionResult && (
        <div style={{ marginTop: '20px', padding: '16px', backgroundColor: '#f0f8ff', borderRadius: '4px' }}>
          <h3>识别结果</h3>
          <p style={{ marginTop: '8px' }}>{recognitionResult}</p>
        </div>
      )}
    </div>
  );
};

export default Recognition;