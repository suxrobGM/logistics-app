# VPS Initial Setup (Ubuntu/Debian)

This guide prepares your Ubuntu/Debian VPS for Logistics TMS deployment.

## Prerequisites

- Ubuntu 22.04 LTS or Debian 12 (fresh install recommended)
- Root or sudo access
- SSH access configured

## Step 1: System Update

```bash
sudo apt update && sudo apt upgrade -y
```

## Step 2: Create Deploy User

```bash
# Create a dedicated user for the application
sudo adduser logistics
sudo usermod -aG sudo logistics

# Switch to the new user
su - logistics
```

## Step 3: Install Docker

```bash
# Install Docker using the convenience script
curl -fsSL https://get.docker.com | sudo sh

# Add your user to the docker group
sudo usermod -aG docker $USER

# Apply group changes (or logout/login)
newgrp docker

# Verify installation
docker --version
docker compose version
```

## Step 4: Install Nginx

```bash
sudo apt install nginx -y

# Start and enable Nginx
sudo systemctl start nginx
sudo systemctl enable nginx

# Verify Nginx is running
sudo systemctl status nginx
```

## Step 5: Install Certbot (SSL)

```bash
sudo apt install certbot python3-certbot-nginx -y
```

## Step 6: Configure Firewall

```bash
# Enable UFW firewall
sudo ufw allow OpenSSH
sudo ufw allow 'Nginx Full'
sudo ufw enable

# Verify firewall status
sudo ufw status
```

Expected output:

```
Status: active

To                         Action      From
--                         ------      ----
OpenSSH                    ALLOW       Anywhere
Nginx Full                 ALLOW       Anywhere
OpenSSH (v6)               ALLOW       Anywhere (v6)
Nginx Full (v6)            ALLOW       Anywhere (v6)
```

## Step 7: Create Application Directory

```bash
sudo mkdir -p /opt/logistics
sudo chown logistics:logistics /opt/logistics
```

## Step 8: Install .NET SDK (Optional)

Only needed if you want to run `aspirate` on the server:

```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET SDK
sudo apt update
sudo apt install -y dotnet-sdk-10.0

# Verify installation
dotnet --version
```

## Step 9: Configure Swap (Recommended for low-memory VPS)

```bash
# Create 4GB swap file
sudo fallocate -l 4G /swapfile
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile

# Make permanent
echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab

# Verify swap
free -h
```

## Directory Structure

After setup, your server should have:

```
/opt/logistics/           # Application root
├── docker-compose.yml    # Docker Compose configuration
├── .env                  # Environment variables
└── data/                 # Persistent data
    ├── postgres/         # Database files
    └── uploads/          # File uploads
```

## Security Recommendations

1. **SSH Key Authentication**: Disable password authentication

   ```bash
   sudo nano /etc/ssh/sshd_config
   # Set: PasswordAuthentication no
   sudo systemctl restart sshd
   ```

2. **Fail2ban**: Install to prevent brute force attacks

   ```bash
   sudo apt install fail2ban -y
   sudo systemctl enable fail2ban
   ```

3. **Automatic Updates**: Enable unattended security updates

   ```bash
   sudo apt install unattended-upgrades -y
   sudo dpkg-reconfigure -plow unattended-upgrades
   ```

## Next Steps

- [Aspire Deployment](aspire-deployment.md) - Generate Docker Compose from Aspire
- [Docker Compose Production](docker-compose-prod.md) - Manual Docker Compose setup
