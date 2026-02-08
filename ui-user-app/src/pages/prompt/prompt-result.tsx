import { useState } from "react";
import { useSignalR } from "@/hooks/useSignalR";
import {
  DoraemonMessage,
  ImageRecognitionStatus,
} from "@/types/doraemonMessage";
import { useCallback } from "react";
import classes from "./prompt-result.module.scss";
const PromptResult = () => {
  const [messages, setMessages] = useState<DoraemonMessage[]>([]);

  const handleMessageReceived = useCallback((msg: DoraemonMessage) => {
    console.log("Received message:", msg);

    setMessages((prev) => [msg, ...prev]);

    if (msg.doraemonItem.status === ImageRecognitionStatus.Failed) {
      alert(
        `用户${msg.doraemonItem.userId}识别失败：${msg.doraemonItem.errorMessage}`,
      );
    }
  }, []);

  const { connectionState } = useSignalR(handleMessageReceived);

  return (
    <div className={classes.container}>
      <h3 className={classes.signalRState}>SignalR state: {connectionState}</h3>
      <div>
        {messages.map((msg) => (
          <div key={msg.doraemonItem.id} style={{ margin: "10px 0" }}>
            <p>主题：{msg.topic}</p>
            <p>用户ID：{msg.doraemonItem.userId}</p>
            <article className={classes.imageContainer}>
              <img src={msg.doraemonItem.outputImageUrl} alt="Output Image" />
            </article>
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
