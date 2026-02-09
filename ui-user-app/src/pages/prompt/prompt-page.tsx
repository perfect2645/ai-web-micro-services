import React, { useState } from "react";
import PromptInput, { PromptImage } from "@/components/prompt/prompt-input";
import { FileResponse } from "@/types/FileResponse";
import { v4 as uuidv4 } from "uuid";
import PromptResult from "@/pages/prompt/prompt-result";

const VITE_FILE_UPLOAD_URL =
  import.meta.env.VITE_FILE_UPLOAD_URL || "https://localhost:7092/api/file";
const VITE_DORAEMON_API_URL =
  import.meta.env.VITE_DORAEMON_API_URL ||
  "https://localhost:7093/api/doraemon";

const blessingList = [
  "新年快乐！",
  "马年吉祥！",
  "暴富暴富！",
  "好运连连！",
  "万事顺意！",
  "平安喜乐！",
  "心想事成！",
  "财运亨通！",
  "福气满满！",
  "喜事临门！",
  "前程似锦！",
  "笑口常开！",
  "健康无忧！",
  "吉祥如意！",
  "好运爆棚！",
  "升职加薪！",
  "幸福美满！",
  "诸事皆顺！",
  "好运常在！",
  "日日欢喜！",
];

const PromptPage: React.FC = () => {
  const [imgData, setImgData] = useState<FileResponse | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false); // 添加提交状态

  // 上传图片到服务器
  const uploadImages = async (
    image: PromptImage,
    description: string,
  ): Promise<FileResponse> => {
    if (!image) return null as unknown as FileResponse;

    const formData = new FormData();
    formData.append("file", image.file, image.name);
    formData.append("description", description);

    try {
      const response = await fetch(VITE_FILE_UPLOAD_URL, {
        method: "POST",
        body: formData,
      });

      console.log("上传结果:", response);
      if (!response.ok) {
        throw new Error(`上传失败: ${response.status} ${response.statusText}`);
      }

      const result: FileResponse = await response.json();
      return result;
    } catch (error) {
      console.error(`上传图片 ${image.name} 失败:`, error);
      throw new Error(
        `图片 "${image.name}" 上传失败: ${(error as Error).message}`,
      );
    }
  };

  // 处理内容变更
  const handlePromptChange = async (text: string, image: PromptImage) => {
    const imageData = await uploadImages(image, text);
    setImgData(imageData);
    console.log("当前输入：", text);
    console.log("当前图片：", imageData.remoteUrl);
  };

  const getOrCreateUserId = (): string => {
    let userId = localStorage.getItem("doraemonUserId");
    if (!userId) {
      userId = uuidv4();
      localStorage.setItem("doraemonUserId", userId);
    }
    return userId;
  };
  // 处理提交
  const handlePromptSubmit = async (text: string) => {
    if (isSubmitting) return; // 防止重复提交

    setIsSubmitting(true);

    const formData = new FormData();
    const tempUserId = getOrCreateUserId();
    formData.append("userId", tempUserId);
    formData.append("inputImageId", imgData ? imgData.id : "");
    formData.append("inputImageUrl", imgData ? imgData.remoteUrl : "");
    formData.append("propmtText", text);

    try {
      await fetch(VITE_DORAEMON_API_URL, {
        method: "POST",
        body: formData,
      });
      setImgData(null);

      const randomBlessing =
        blessingList[Math.floor(Math.random() * blessingList.length)];
      alert(`提交成功！${randomBlessing}`);
    } catch (error) {
      console.error("提交失败:", error);
      alert(`提交失败: ${(error as Error).message}`);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <>
      <div>
        <PromptResult></PromptResult>
      </div>
      <div style={{ padding: 20 }}>
        <h2 style={{ color: "#1f2937", marginBottom: 20, textAlign: "center" }}>
          Prompt with text and image
        </h2>
        <PromptInput
          onChange={handlePromptChange}
          onSubmit={handlePromptSubmit}
          placeholder="Please enter your question, or upload an image for assistance..."
        />
        {isSubmitting && (
          <div style={{ textAlign: "center", marginTop: 10, color: "#6b7280" }}>
            Uploading image...
          </div>
        )}
      </div>
    </>
  );
};

export default PromptPage;
