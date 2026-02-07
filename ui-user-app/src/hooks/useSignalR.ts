// useSignalR.ts
import { useState, useEffect, useRef, useCallback } from "react";
import * as signalR from "@microsoft/signalr";
import { SignalRConnectionState } from "../types/signalR";
import { DoraemonMessage } from "@/types/doraemonMessage";

const SIGNALR_HUB_URL =
  import.meta.env.VITE_SIGNALR_HUB_URL || "https://localhost:7094/signalRHub";

/**
 * Custom hook for managing SignalR connections.
 *
 * @param onMessageReceived - Callback function to handle received messages.
 * @returns An object containing the current connection state.
 */
export function useSignalR(
  onMessageReceived: (message: DoraemonMessage) => void,
) {
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const [connectionState, setConnectionState] =
    useState<SignalRConnectionState>("disconnected");

  // Use useRef to store the latest message callback to avoid dependency changes
  const messageCallbackRef = useRef(onMessageReceived);

  // Sync the latest callback on every render
  useEffect(() => {
    messageCallbackRef.current = onMessageReceived;
  }, [onMessageReceived]);

  // Stable message handler that never changes
  const handleMessage = useCallback((message: unknown) => {
    const msg = message as DoraemonMessage;
    messageCallbackRef.current(msg);
  }, []);

  // Initialize connection (runs only once)
  useEffect(() => {
    // Create connection
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(SIGNALR_HUB_URL, {
        transport: signalR.HttpTransportType.WebSockets,
        withCredentials: true,
      })
      .withAutomaticReconnect()
      .build();

    // Bind listeners (only once)
    connection.on("ReceiveRealTimeMessage", handleMessage);

    // State listeners
    connection.onreconnecting(() => setConnectionState("reconnecting"));
    connection.onreconnected(() => setConnectionState("connected"));
    connection.onclose(() => setConnectionState("disconnected"));

    // Start connection
    connection
      .start()
      .then(() => {
        connectionRef.current = connection;
        setConnectionState("connected");
        console.log("✅ SignalR 连接成功");
      })
      .catch((err) => {
        setConnectionState("disconnected");
        console.error("❌ 连接失败", err);
      });

    // Cleanup function: runs only once when the component unmounts
    return () => {
      connection.off("ReceiveRealTimeMessage", handleMessage);
      connection.stop().catch(() => {});
      setConnectionState("disconnected");
      console.log("🔌 连接已销毁");
    };

    // 🔥 Key: Dependency is only [handleMessage](file://d:\Web\image-ai\ai-web-micro-services\ui-user-app\src\hooks\useSignalR.ts#L23-L26) (which never changes)
    // The effect runs exactly once and does not re-trigger
  }, [handleMessage]);

  return { connectionState };
}
