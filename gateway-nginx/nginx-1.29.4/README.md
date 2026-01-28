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

# 使用localhost自签名证书启用SSl

1. Copy generated localhost self-signed cert (pfx file) to ssl folder
2. open git-bash and navigate to the nginx/ssl folder
3. run below command to generate pem cert file and private key pem file

   ```
    # 1. export cert（private key excluded）→ cert file
    openssl pkcs12 -in localhost-doraemon.pfx -clcerts -nokeys -out localhost-doraemon-cert.pem -passin pass:asdf1234

    # 2. export private key（cert content excluded）→ private key file
    openssl pkcs12 -in localhost-doraemon.pfx -nocerts -nodes -out localhost-doraemon-key.pem -passin pass:asdf1234
   ```

4. update nginx.conf

```
# HTTPS server
server {
    listen       443 ssl;
    server_name  localhost;

    # 替换为你转换后的 PEM 路径（绝对路径更稳妥）
    ssl_certificate      C:/nginx/ssl/localhost-doraemon-cert.pem;
    ssl_certificate_key  C:/nginx/ssl/localhost-doraemon-key.pem;

    # 其他 SSL 配置保持不变
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384:ECDHE-ECDSA-CHACHA20-POLY1305:ECDHE-RSA-CHACHA20-POLY1305:DHE-RSA-AES128-GCM-SHA256:DHE-RSA-AES256-GCM-SHA384;
    ssl_prefer_server_ciphers on;
    ssl_session_cache shared:SSL:1m;
    ssl_session_timeout 10m;

    location /files/ {
        proxy_pass https://127.0.0.1:7092/;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_http_version 1.1;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_ssl_verify off;
        proxy_ssl_verify_depth 0;
    }
}
```
