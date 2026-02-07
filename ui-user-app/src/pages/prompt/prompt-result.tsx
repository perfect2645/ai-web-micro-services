import { useState } from "react";
import { useSignalR } from "@/hooks/useSignalR";
import {
  DoraemonMessage,
  ImageRecognitionStatus,
} from "@/types/doraemonMessage";

import classes from "./prompt-result.module.scss";
const PromptResult = () => {
  const [messages, setMessages] = useState<DoraemonMessage[]>([]);

  // 强类型回调，TS会自动校验msg的结构
  const handleMessageReceived = (msg: DoraemonMessage) => {
    setMessages((prev) => [msg, ...prev]);

    // 示例：根据状态做业务逻辑
    if (msg.doraemonItem.status === ImageRecognitionStatus.Failed) {
      alert(
        `用户${msg.doraemonItem.userId}的图片识别失败：${msg.doraemonItem.errorMessage}`,
      );
    }
  };

  const { connectionState } = useSignalR(handleMessageReceived);

  return (
    <div className={classes.container}>
      <h3 className={classes.signalRState}>SignalR state: {connectionState}</h3>
      <div>
        {messages.map((msg) => (
          <div key={msg.doraemonItem.id} style={{ margin: "10px 0" }}>
            <p>主题：{msg.topic}</p>
            <p>用户ID：{msg.doraemonItem.userId}</p>
            <p>图片URL：{msg.doraemonItem.inputImageUrl}</p>
            <p>状态：{msg.doraemonItem.status}</p>
            <p>
              创建时间：{new Date(msg.doraemonItem.createTime).toLocaleString()}
            </p>
          </div>
        ))}
      </div>
    </div>
  );
};

export default PromptResult;
