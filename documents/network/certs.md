# Self-signed-certificate

## security

- don't publish your private key to git (add gitignore for private key)

## webapi config

- create a self-signed-certificate for micro-services

1. Open git bash with admin user
2. cd cert target path (D:\secrets\local-certs)
3. create openssl config

```
cat > openssl.cnf << EOF
[req]
default_bits = 2048
prompt = no
default_md = sha256
distinguished_name = dn
x509_extensions = v3_ext

[dn]
CN = localhost-doraemon

[v3_ext]
subjectAltName = @alt_names
keyUsage = digitalSignature, keyEncipherment
extendedKeyUsage = serverAuth

[alt_names]
DNS.1 = localhost
DNS.2 = localhost-doraemon
IP.1 = 127.0.0.1
IP.2 = 192.168.31.122
EOF
```

3. create cert by running below command

```
# 1. generate private key
openssl genrsa -out localhost-doraemon.key 2048

# 2. generate cert request
openssl req -new -key localhost-doraemon.key -out localhost-doraemon.csr -config openssl.cnf

# 3. generate self-signed cert
openssl x509 -req -days 3650 -in localhost-doraemon.csr -signkey localhost-doraemon.key -out localhost-doraemon.crt -extensions v3_ext -extfile openssl.cnf

# 4. generate .pfx cert
openssl pkcs12 -export -out localhost-doraemon.pfx -inkey localhost-doraemon.key -in localhost-doraemon.crt -password pass:asdf1234

# 5. generate windows type cert (.cer)
cp localhost-doraemon.crt localhost-doraemon.cer
```

4. install public cert (.cer)
5. run below command to generate pem cert file and private key pem file

   ```
    # 1. export cert（private key excluded）→ cert file
    openssl pkcs12 -in localhost-doraemon.pfx -clcerts -nokeys -out localhost-doraemon-cert.pem -passin pass:asdf1234

    # 2. export private key（cert content excluded）→ private key file
    openssl pkcs12 -in localhost-doraemon.pfx -nocerts -nodes -out localhost-doraemon-key.pem -passin pass:asdf1234
   ```

6. add pfx cert to webapi prject, set copy to output folder when build
7. update webapi appsettings.json

```
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:7092",
        "Certificate": {
          "Path": "Configurations/Certs/localhost-doraemon.pfx",
          "Password": "asdf1234",
          "AllowInvalid": false
        }
      }
    }
  }
```

## react config

1. place cert to react project

```
your-react-project/
├── ssl/
�?  ├── localhost-doraemon-cert.pem  # 证书公钥
�?  └── localhost-doraemon-key.pem   # 证书私钥
├── src/
├── package.json
└── vite.config.js/ts
```

2. update vite.conifg.js

```
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  server: {
    https: {
      cert: path.resolve(__dirname, './ssl/localhost-doraemon-cert.pem'),
      key: path.resolve(__dirname, './ssl/localhost-doraemon-key.pem'),
    },
    port: 3000,
    open: true,
  },
});
```
