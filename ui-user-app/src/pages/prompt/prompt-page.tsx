import React, { useState } from "react";
import PromptInput, { PromptImage } from "@/components/prompt/prompt-input";

const PromptPage: React.FC = () => {
  // 管理文字内容和图片列表
  const [textValue, setTextValue] = useState("");
  const [imageList, setImageList] = useState<PromptImage[]>([]);

  // 处理内容变更
  const handlePromptChange = (text: string, images: PromptImage[]) => {
    setTextValue(text);
    setImageList(images);
    console.log("当前输入：", text);
    console.log("当前图片：", images);
  };

  // 处理提交
  const handlePromptSubmit = (text: string, images: PromptImage[]) => {
    console.log("提交内容：", text);
    console.log("提交图片：", images);

    // 这里可添加接口请求逻辑（如上传图片、发送Prompt）
    // 示例：获取图片文件对象用于上传
    // const imageFiles = images.map(img => img.file);
    // await uploadImages(imageFiles);
    // await sendPrompt(text);

    // 提交后清空内容（可选）
    setTextValue("");
    setImageList([]);
    alert("提交成功！");
  };

  return (
    <div style={{ padding: 20 }}>
      <h2 style={{ color: "#1f2937", marginBottom: 20, textAlign: "center" }}>
        仿豆包 Prompt 输入框
      </h2>
      <PromptInput
        textValue={textValue}
        imageList={imageList}
        onChange={handlePromptChange}
        onSubmit={handlePromptSubmit}
        placeholder="请输入你的问题，或上传图片获取帮助..."
      />
    </div>
  );
};

export default PromptPage;
