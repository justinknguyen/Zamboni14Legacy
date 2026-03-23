#!/bin/sh
set -e

: "${QOS_IP:?QOS_IP environment variable is required}"

# Generate appsettings.json at container start
cat > /app/appsettings.json <<EOF
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "Qos": {
    "QosIp": "${QOS_IP}",
    "FirewallPrimaryIp": "${QOS_IP}",
    "FirewallSecondaryIp": "${QOS_IP}"
  }
}
EOF

exec ./Skateboard3Server.Qos
