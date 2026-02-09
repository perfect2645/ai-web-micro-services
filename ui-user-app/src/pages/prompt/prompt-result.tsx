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
      <h3 className={classes.signalRState}>
        <a href="https://home.fawei.dpdns.org/rabbitmq">RabbitMQ Status</a>
      </h3>
      <div>
        {messages.map((msg) => (
          <div key={msg.doraemonItem.id} className={classes.messageItem}>
            <article className={classes.imgContainer}>
              <img src={msg.doraemonItem.outputImageUrl} alt="Output Image" />
            </article>
            <p>topic：{msg.topic}</p>
            <p>userId：{msg.doraemonItem.userId}</p>
            <p>process status：{msg.doraemonItem.status}</p>
            <p>
              createTime：
              {new Date(msg.doraemonItem.createTime).toLocaleString()}
            </p>
            <p>
              outputImageUrl ：
              <a href={msg.doraemonItem.outputImageUrl}>Click to download</a>
            </p>
          </div>
        ))}
      </div>
    </div>
  );
};

export default PromptResult;
