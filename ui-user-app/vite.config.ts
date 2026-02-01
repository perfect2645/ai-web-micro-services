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
  },
});
