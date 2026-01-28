# 生成自签名证书（替换为你的域名/IP，如 localhost、127.0.0.1）
$cert = New-SelfSignedCertificate -DnsName "localhost" -CertStoreLocation "Cert:\LocalMachine\My" -KeyUsage DigitalSignature, KeyEncipherment -Type SSLServerAuthentication

# 导出证书（.pfx 格式，含私钥，用于 ASP.NET Core）
$pwd = ConvertTo-SecureString -String "asdf1234" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath "D:\secrets\local-certs\localhost-doraemon.pfx" -Password $pwd

# 导出公钥（.cer 格式，用于浏览器信任）
Export-Certificate -Cert $cert -FilePath "D:\secrets\local-certs\localhost-doraemon.cer" -Type CERT