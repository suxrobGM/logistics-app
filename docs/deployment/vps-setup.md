# VPS Initial Setup

Prepare your Ubuntu/Debian VPS for Logistics TMS deployment.

## Prerequisites

- Ubuntu 22.04+ LTS or Debian 12+
- Root or sudo access
- SSH access configured

## Step 1: System Update

```bash
sudo apt update && sudo apt upgrade -y
```

## Step 2: Create Deploy User

```bash
sudo adduser logistics
sudo usermod -aG sudo logistics
su - logistics
```

## Step 3: Install Docker

```bash
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker $USER
newgrp docker

# Verify
docker --version
docker compose version
```

## Step 4: Install Nginx

```bash
sudo apt install nginx -y
sudo systemctl enable nginx
```

## Step 5: Install Certbot (SSL)

```bash
sudo apt install certbot python3-certbot-nginx -y
```

## Step 6: Configure Firewall

```bash
sudo ufw allow OpenSSH
sudo ufw allow 'Nginx Full'
sudo ufw enable
```

## Step 7: Clone and Deploy

```bash
git clone https://github.com/suxrobgm/logistics-app.git
cd logistics-app/src/Aspire/Logistics.Aspire.AppHost/aspire-output

# Configure environment
cp .env.example .env
nano .env  # Edit with your production values

# Deploy
docker compose up -d
```

## Step 8: Configure Nginx

Copy the nginx configuration:

```bash
sudo cp logistics.conf /etc/nginx/sites-available/logistics
sudo ln -s /etc/nginx/sites-available/logistics /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

## Step 9: Obtain SSL Certificates

```bash
sudo certbot --nginx -d api.yourdomain.com
sudo certbot --nginx -d id.yourdomain.com
sudo certbot --nginx -d admin.yourdomain.com
sudo certbot --nginx -d office.yourdomain.com
```

## Security Recommendations

1. **Disable password SSH**:

   ```bash
   sudo sed -i 's/#PasswordAuthentication yes/PasswordAuthentication no/' /etc/ssh/sshd_config
   sudo systemctl restart sshd
   ```

2. **Install Fail2ban**:

   ```bash
   sudo apt install fail2ban -y
   sudo systemctl enable fail2ban
   ```

3. **Enable automatic updates**:

   ```bash
   sudo apt install unattended-upgrades -y
   sudo dpkg-reconfigure -plow unattended-upgrades
   ```

## Next Steps

- [Docker Compose Deployment](aspire-deployment.md) - Service configuration details
- [Environment Variables](../configuration/environment-variables.md) - Configuration reference
