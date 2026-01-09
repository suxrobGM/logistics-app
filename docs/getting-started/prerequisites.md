# Prerequisites

Required software to run Logistics TMS locally.

## Required Software

### .NET 10 SDK

Download and install from [Microsoft](https://dotnet.microsoft.com/download/dotnet/10.0).

Verify installation:

```bash
dotnet --version
# Should output: 10.0.x
```

### Bun Runtime

Required for the Angular Office App.

**Windows** (PowerShell):

```powershell
irm bun.sh/install.ps1 | iex
```

**macOS/Linux**:

```bash
curl -fsSL https://bun.sh/install | bash
```

Verify installation:

```bash
bun --version
```

### PostgreSQL 17

Download from [PostgreSQL](https://www.postgresql.org/download/).

**Windows**: Use the installer, remember the password you set for `postgres` user.

**macOS** (Homebrew):

```bash
brew install postgresql@17
brew services start postgresql@17
```

**Ubuntu/Debian**:

```bash
sudo apt install postgresql postgresql-contrib
sudo systemctl start postgresql
```

Verify installation:

```bash
psql --version
```

### Docker (Optional)

Required only if using Aspire for local development.

Download [Docker Desktop](https://docs.docker.com/get-docker/) for Windows/macOS, or install Docker Engine on Linux.

Verify installation:

```bash
docker --version
docker compose version
```

## Optional Software

### IDE Recommendations

| IDE | Best For |
|-----|----------|
| **Visual Studio 2022** | Full .NET development, debugging |
| **VS Code** | Lightweight, Angular development |
| **JetBrains Rider** | Cross-platform .NET development |
| **Android Studio** | Driver mobile app (Kotlin) |

### VS Code Extensions

For the best experience with VS Code:

- C# Dev Kit
- Angular Language Service
- ESLint
- Prettier
- Docker
- PostgreSQL (by Chris Kolkman)

### Database Tools

- **pgAdmin 4**: Included with PostgreSQL, or download separately
- **DBeaver**: Free universal database tool
- **DataGrip**: JetBrains commercial tool

## System Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| CPU | 4 cores | 8 cores |
| RAM | 8 GB | 16 GB |
| Storage | 20 GB free | 50 GB SSD |
| OS | Windows 10, macOS 12, Ubuntu 22.04 | Latest versions |

## Port Requirements

Ensure these ports are available:

| Port | Service |
|------|---------|
| 5432 | PostgreSQL |
| 7000 | API |
| 7001 | Identity Server |
| 7002 | Admin App |
| 7003 | Office App |
| 8100 | Aspire Dashboard |

Check port availability:

**Windows**:

```powershell
netstat -an | findstr "7000 7001 7002 7003"
```

**macOS/Linux**:

```bash
lsof -i :7000,:7001,:7002,:7003
```

## Next Steps

- [Local Development](local-development.md) - Manual setup without Docker
- [Docker Development](docker-development.md) - Run with Aspire and Docker
