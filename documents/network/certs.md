# Self-signed-certificate

## security

- don't publish your private key to git (add gitignore for private key)

## webapi config

- create a self-signed-certificate for micro-services

1. Open powershell with admin user
2. run create-local-cert.ps1
3. install public cert (.ce)
4. add pfx cert to webapi prject, set copy to output folder when build
5. update webapi appsettings.json

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
│   ├── localhost-doraemon-cert.pem  # 证书公钥
│   └── localhost-doraemon-key.pem   # 证书私钥
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
