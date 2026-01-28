# Self-signed-certificate

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
