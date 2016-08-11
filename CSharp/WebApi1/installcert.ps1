$pwd=ConvertTo-SecureString -String "123456Abc" -Force -AsPlainText
Import-PfxCertificate -FilePath vantest.pfx cert:\LocalMachine\my -Password $pwd