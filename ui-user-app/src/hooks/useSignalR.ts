import { useState, useEffect, useRef, useCallback } from "react";
import * as signalR from "@microsoft/signalr";
import { SignalRConnectionState } from "../types/signalR";
import { DoraemonMessage } from "@/types/doraemonMessage";

// 配置Hub地址（Vite环境变量，生产/开发自动切换）
const SIGNALR_HUB_URL =
  import.meta.env.VITE_SIGNALR_HUB_URL || "https://localhost:7094/signalRHub";

export function useSignalR(
  onMessageReceived: (message: DoraemonMessage) => void,
) {
  // 连接实例Ref，解决函数自引用、提前访问问题
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const [connectionState, setConnectionState] =
    useState<SignalRConnectionState>("disconnected");

  // 核心：用useRef缓存初始化函数，彻底解决【声明前访问】报错
  const initConnectionRef = useRef(async () => {
    // 已连接则直接退出
    if (connectionRef.current?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    // 创建连接实例
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(SIGNALR_HUB_URL, {
        transport: signalR.HttpTransportType.WebSockets,
        withCredentials: true,
      })
      .withAutomaticReconnect([0, 5000, 20000, 30000])
      .build();

    // 连接状态监听
    newConnection.onreconnecting(() => setConnectionState("reconnecting"));
    newConnection.onreconnected(() => setConnectionState("connected"));
    newConnection.onclose((error) => {
      setConnectionState("disconnected");
      if (error) {
        console.error("连接异常关闭", error);
      } else {
        console.log("连接正常关闭");
      }
      // 断开后自动重试，无提前访问问题
      setTimeout(initConnectionRef.current, 5000);
    });

    // 修复：移除泛型，类型断言，解决上一个TS报错
    newConnection.on("ReceiveRealTimeMessage", (message: unknown) => {
      const msg = message as DoraemonMessage;
      onMessageReceived(msg);
      console.log("✅ 收到服务端消息:", msg);
    });

    // 启动连接
    try {
      setConnectionState("connecting");
      await newConnection.start();
      setConnectionState("connected");
      connectionRef.current = newConnection;
      console.log("✅ SignalR 连接成功");
    } catch (err) {
      setConnectionState("disconnected");
      console.error("❌ 连接失败:", err);
      // 连接失败重试，使用ref调用，无声明前访问
      setTimeout(initConnectionRef.current, 5000);
    }
  });

  // 主动发送消息方法
  const sendMessage = useCallback(
    async (methodName: string, ...args: unknown[]) => {
      if (
        connectionRef.current?.state !== signalR.HubConnectionState.Connected
      ) {
        console.error("❌ 未连接，无法发送消息");
        return;
      }
      try {
        await connectionRef.current.invoke(methodName, ...args);
      } catch (err) {
        console.error("❌ 发送消息失败:", err);
      }
    },
    [],
  );

  // 组件挂载初始化，卸载销毁
  useEffect(() => {
    // 执行连接
    initConnectionRef.current();

    // 清理函数：组件卸载，关闭连接，防止内存泄漏
    return () => {
      if (connectionRef.current) {
        connectionRef.current.off("ReceiveRealTimeMessage");
        connectionRef.current.stop();
        connectionRef.current = null;
        setConnectionState("disconnected");
        console.log("🔌 连接已销毁");
      }
    };
  }, [onMessageReceived]);

  return {
    connectionState,
    sendMessage,
  };
}
