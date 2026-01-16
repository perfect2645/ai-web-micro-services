import React, { useState } from "react";
import PromptInput, { PromptImage } from "@/components/prompt/prompt-input";

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
    const randomBlessing =
      blessingList[Math.floor(Math.random() * blessingList.length)];
    alert(randomBlessing);
  };

  return (
    <div style={{ padding: 20 }}>
      <h2 style={{ color: "#1f2937", marginBottom: 20, textAlign: "center" }}>
        Prompt with text and image
      </h2>
      <PromptInput
        textValue={textValue}
        imageList={imageList}
        onChange={handlePromptChange}
        onSubmit={handlePromptSubmit}
        placeholder="Please enter your question, or upload an image for assistance..."
      />
    </div>
  );
};

export default PromptPage;
