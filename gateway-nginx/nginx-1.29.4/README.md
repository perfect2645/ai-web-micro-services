# nginx-win

# 查看所有 TCP 监听端口（包含进程 PID）

netstat -ano | findstr "LISTENING"

# 先找 Nginx 的 PID，再查该 PID 监听的端口

```cmd
tasklist | findstr "nginx.exe"

# 把1234换成上面查到的Nginx PID
netstat -ano | findstr "1234"

```

# 启动、停止、重启 Nginx

start nginx
nginx.exe -s quit
nginx.exe -s reload
