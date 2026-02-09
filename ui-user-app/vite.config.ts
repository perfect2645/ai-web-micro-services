import path from "path";
import tailwindcss from "@tailwindcss/vite";
import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    allowedHosts: ["home.fawei.dpdns.org"], // 加入被拦截的主机
    https: {
      cert: path.resolve(__dirname, "./ssl/localhost-doraemon-cert.pem"),
      key: path.resolve(__dirname, "./ssl/localhost-doraemon-key.pem"),
    },
    port: 3000,
    open: true,
    proxy: {
      // 关键：将 /api 开头的请求转发到后端
      "/api/file": {
        target: "https://127.0.0.1:7092",
        changeOrigin: true, // 必须加，解决跨域
        secure: false, // 忽略自签名证书错误（关键！解决HTTPS证书问题）
        ws: true, // 如果有WebSocket可保留
        rewrite: (path) => path, // 无需重写路径，直接转发
      },
      "/api/doraemon": {
        target: "https://127.0.0.1:7093",
        changeOrigin: true, // 必须加，解决跨域
        secure: false, // 忽略自签名证书错误（关键！解决HTTPS证书问题）
        ws: true, // 如果有WebSocket可保留
        rewrite: (path) => path, // 无需重写路径，直接转发
      },
    },
  },
});
