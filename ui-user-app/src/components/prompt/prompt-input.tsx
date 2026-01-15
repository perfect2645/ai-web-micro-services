import React, { useState, useCallback, useRef } from "react";
import UploadButton from "@/components/ui/prompt/upload-button";
import classes from "./prompt-input.module.scss";

// 图片信息类型定义
export interface PromptImage {
  id: string; // 图片唯一标识
  file: File; // 图片文件对象
  url: string; // 图片预览地址（base64/blob）
  name: string; // 图片名称
  size: number; // 图片大小（字节）
}

// 组件属性类型定义
export interface PromptInputProps {
  // 文字输入内容（受控属性）
  textValue: string;
  // 图片列表（受控属性）
  imageList: PromptImage[];
  // 内容变更回调（返回文字和图片列表）
  onChange: (text: string, images: PromptImage[]) => void;
  // 提交回调（点击提交/回车提交时触发）
  onSubmit: (text: string, images: PromptImage[]) => void;
  // 占位提示文字
  placeholder?: string;
  // 自定义类名
  className?: string;
  // 是否禁用
  disabled?: boolean;
  // 允许上传的图片格式（默认常见图片格式）
  acceptImageTypes?: string[];
  // 单张图片最大大小（默认20MB）
  maxImageSize?: number;
}

/**
 * 仿豆包风格 Prompt 输入框
 * 支持：文字输入、图片选择、图片粘贴、图片拖拽上传
 */
const PromptInput: React.FC<PromptInputProps> = ({
  textValue,
  imageList,
  onChange,
  onSubmit,
  placeholder = "prompt text or image ...",
  className,
  disabled = false,
  acceptImageTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"],
  maxImageSize = 20 * 1024 * 1024, // 20MB
}) => {
  // 拖拽悬浮状态
  const [isDragOver, setIsDragOver] = useState(false);
  // 文件选择器Ref
  const fileInputRef = useRef<HTMLInputElement>(null);
  // 生成唯一ID
  const generateImageId = useCallback(() => {
    return `img_${Date.now()}_${Math.floor(Math.random() * 1000)}`;
  }, []);

  // 处理文件转Base64（用于预览）
  const fileToBase64 = useCallback((file: File): Promise<string> => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = (err) => reject(err);
      reader.readAsDataURL(file);
    });
  }, []);

  // 校验图片文件合法性
  const validateImageFile = useCallback(
    (file: File): { valid: boolean; message?: string } => {
      // 校验格式
      if (!acceptImageTypes.includes(file.type)) {
        return {
          valid: false,
          message: `不支持该图片格式，仅支持${acceptImageTypes
            .map((type) => type.split("/")[1])
            .join("、")}格式`,
        };
      }
      // 校验大小
      if (file.size > maxImageSize) {
        return {
          valid: false,
          message: `单张图片大小不能超过${(maxImageSize / 1024 / 1024).toFixed(
            1
          )}MB`,
        };
      }
      return { valid: true };
    },
    [acceptImageTypes, maxImageSize]
  );

  // 处理图片添加（统一处理选择/粘贴/拖拽的图片）
  const handleAddImages = useCallback(
    async (files: FileList | null) => {
      if (!files || files.length === 0 || disabled) return;

      const newImages: PromptImage[] = [];
      for (let i = 0; i < files.length; i++) {
        const file = files[i];
        // 校验文件
        const validateResult = validateImageFile(file);
        if (!validateResult.valid) {
          alert(validateResult.message);
          continue;
        }
        // 转Base64获取预览地址
        try {
          const url = await fileToBase64(file);
          newImages.push({
            id: generateImageId(),
            file,
            url,
            name: file.name,
            size: file.size,
          });
        } catch (err) {
          console.error("图片转Base64失败：", err);
          alert("图片预览失败，请重新上传");
        }
      }

      // 触发变更回调
      if (newImages.length > 0) {
        onChange(textValue, [...imageList, ...newImages]);
      }
    },
    [
      disabled,
      validateImageFile,
      fileToBase64,
      generateImageId,
      textValue,
      imageList,
      onChange,
    ]
  );

  // 1. 选择图片：触发文件选择器
  const handleSelectImageClick = useCallback(() => {
    if (disabled || !fileInputRef.current) return;
    fileInputRef.current.click();
  }, [disabled]);

  // 2. 选择图片：文件变更处理
  const handleFileInputChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      handleAddImages(e.target.files);
      // 重置文件选择器（允许重复选择同一张图片）
      if (e.target) e.target.value = "";
    },
    [handleAddImages]
  );

  // 3. 粘贴图片：处理剪贴板粘贴
  const handlePaste = useCallback(
    (e: React.ClipboardEvent<HTMLTextAreaElement>) => {
      if (disabled) return;
      const clipboardData = e.clipboardData;
      // 优先处理剪贴板中的图片文件
      const files = clipboardData.files;
      if (files && files.length > 0) {
        e.preventDefault(); // 阻止默认粘贴行为（避免粘贴文件路径）
        handleAddImages(files);
        return;
      }
      // 无图片时，允许正常文字粘贴（无需阻止默认行为）
    },
    [disabled, handleAddImages]
  );

  // 4. 拖拽图片：拖拽进入
  const handleDragEnter = useCallback(
    (e: React.DragEvent<HTMLDivElement>) => {
      e.preventDefault();
      e.stopPropagation();
      if (!disabled) setIsDragOver(true);
    },
    [disabled]
  );

  // 4. 拖拽图片：拖拽悬浮
  const handleDragOver = useCallback(
    (e: React.DragEvent<HTMLDivElement>) => {
      e.preventDefault();
      e.stopPropagation();
      if (!disabled) setIsDragOver(true);
    },
    [disabled]
  );

  // 4. 拖拽图片：拖拽离开
  const handleDragLeave = useCallback(
    (e: React.DragEvent<HTMLDivElement>) => {
      e.preventDefault();
      e.stopPropagation();
      if (!disabled) setIsDragOver(false);
    },
    [disabled]
  );

  // 4. 拖拽图片：拖拽放下
  const handleDrop = useCallback(
    (e: React.DragEvent<HTMLDivElement>) => {
      e.preventDefault();
      e.stopPropagation();
      if (disabled) return;
      setIsDragOver(false);
      const files = e.dataTransfer.files;
      handleAddImages(files);
    },
    [disabled, handleAddImages]
  );

  // 处理图片删除
  const handleRemoveImage = useCallback(
    (imageId: string) => {
      if (disabled) return;
      const newImageList = imageList.filter((img) => img.id !== imageId);
      onChange(textValue, newImageList);
    },
    [disabled, imageList, textValue, onChange]
  );

  // 处理文字输入变更
  const handleTextChange = useCallback(
    (e: React.ChangeEvent<HTMLTextAreaElement>) => {
      if (disabled) return;
      onChange(e.target.value, imageList);
    },
    [disabled, imageList, onChange]
  );

  // 处理键盘事件（回车提交，Ctrl/Command+回车换行）
  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
      if (disabled) return;
      // 纯回车提交（非Ctrl/Command+回车）
      if (e.key === "Enter" && !e.ctrlKey && !e.metaKey) {
        e.preventDefault();
        if (textValue.trim() || imageList.length > 0) {
          onSubmit(textValue.trim(), imageList);
        }
      }
    },
    [disabled, textValue, imageList, onSubmit]
  );

  // 处理手动提交
  const handleSubmitClick = useCallback(() => {
    if (disabled) return;
    if (textValue.trim() || imageList.length > 0) {
      onSubmit(textValue.trim(), imageList);
    } else {
      alert("请输入文字或上传图片后提交");
    }
  }, [disabled, textValue, imageList, onSubmit]);

  return (
    <div
      className={`${classes.promptContainer} ${className || ""} ${
        isDragOver ? classes.promptContainerDragOver : ""
      } ${disabled ? classes.promptContainerDisabled : ""}`}
      onDragEnter={handleDragEnter}
      onDragOver={handleDragOver}
      onDragLeave={handleDragLeave}
      onDrop={handleDrop}
    >
      {/* 图片预览列表 */}
      {imageList.length > 0 && (
        <div className={classes.promptImageList}>
          {imageList.map((image) => (
            <div key={image.id} className={classes.promptImageCard}>
              <img
                src={image.url}
                alt={image.name}
                className={classes.promptImagePreview}
              />
              <div
                className={classes.promptImageRemove}
                onClick={() => handleRemoveImage(image.id)}
              >
                ✕
              </div>
              <div className={classes.promptImageInfo}>
                <span className={classes.promptImageName}>{image.name}</span>
                <span className={classes.promptImageSize}>
                  {(image.size / 1024 / 1024).toFixed(2)}MB
                </span>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* 文字输入框 */}
      <textarea
        className={classes.promptTextarea}
        value={textValue}
        onChange={handleTextChange}
        onPaste={handlePaste}
        onKeyDown={handleKeyDown}
        placeholder={placeholder}
        disabled={disabled}
        rows={4}
        spellCheck={false}
      />

      {/* 操作栏 */}
      <div className={classes.promptActionBar}>
        {/* 使用新创建的 UploadButton 组件 */}
        <UploadButton onClick={handleSelectImageClick} disabled={disabled} />

        {/* 提交按钮 */}
        <button
          type="button"
          className={`${classes.promptBtn} ${classes.promptBtnSubmit}`}
          onClick={handleSubmitClick}
          disabled={disabled || (!textValue.trim() && imageList.length === 0)}
        >
          Send
        </button>

        {/* 隐藏的文件选择器 */}
        <input
          ref={fileInputRef}
          type="file"
          multiple
          accept={acceptImageTypes.join(",")}
          onChange={handleFileInputChange}
          style={{ display: "none" }}
        />
      </div>
    </div>
  );
};

export default PromptInput;
