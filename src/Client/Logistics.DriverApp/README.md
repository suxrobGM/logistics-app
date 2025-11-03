# Logistics Driver App - Kotlin Multiplatform

A cross-platform driver application built with **Kotlin Multiplatform (KMP)** and **Compose Multiplatform**, running on both **Android** and **iOS** from a single codebase.

> 📱 **Cross-Platform**: Share ~80% of code between Android and iOS
> 🚀 **Native Performance**: Compiled to native code for each platform
> 🎨 **Modern UI**: Compose Multiplatform for consistent UX
> 📦 **Version Catalog**: Centralized dependency management with `libs.versions.toml`

**Migrated from**: .NET MAUI → Android (Kotlin/Jetpack Compose) → **KMP (Android + iOS)**

## Features

- **Authentication**: OpenID Connect (OIDC) OAuth 2.0 flow with JWT tokens
- **Dashboard**: View truck information and active loads
- **Load Management**: View load details, confirm pickups and deliveries
- **Statistics**: Driver performance metrics with interactive charts
- **Past Loads**: Historical load data for the past 90 days
- **Account Management**: Update user profile information
- **Real-time Location Tracking**: Background location service with proximity detection
- **Push Notifications**: Firebase Cloud Messaging for load updates
- **Maps Integration**: Google Maps for route visualization

## Tech Stack

### Shared (Cross-Platform)
- **Kotlin Multiplatform**: 2.1.0
- **Compose Multiplatform**: 1.7.1 (Shared UI)
- **Ktor Client**: 3.0.2 (HTTP networking)
- **Kotlinx Serialization**: 1.7.3
- **Kotlinx Coroutines**: 1.9.0
- **Kotlinx DateTime**: 0.6.1
- **Koin**: 4.0.0 (Dependency Injection)
- **Voyager**: 1.1.0 (Navigation)
- **Multiplatform Settings**: 1.2.0

### Android Platform
- **Min SDK**: 26 (Android 8.0)
- **Target SDK**: 35 (Android 15)
- **Firebase**: Cloud Messaging, Analytics
- **Google Maps**: Maps Compose 6.2.1
- **AppAuth**: 0.11.1 (OIDC)
- **DataStore**: Preferences storage
- **Timber**: 5.0.1 (Logging)

### iOS Platform
- **iOS Version**: 15.0+
- **Xcode**: 15.0+
- **CocoaPods**: Dependency management
- **Swift Interop**: Native iOS integration

## Project Structure

### KMP Architecture

```
DriverAppKotlin/
├── shared/                    # KMP Shared Module (~80% code sharing)
│   ├── commonMain/            # Platform-independent code
│   │   ├── domain/            # Business logic & models
│   │   ├── data/              # API clients, repositories
│   │   ├── presentation/      # Shared UI (Compose MP)
│   │   └── platform/          # expect declarations
│   ├── androidMain/           # Android-specific implementations
│   └── iosMain/               # iOS-specific implementations
│
├── androidApp/                # Android Application
│   ├── service/               # Android services (location, FCM)
│   └── MainActivity.kt
│
├── iosApp/                    # iOS Application
│   ├── iosApp.xcodeproj
│   └── DriverApp.swift
│
├── gradle/
│   └── libs.versions.toml    # ✨ Centralized dependency versions
│
└── build.gradle.kts
```

## Dependency Management

This project uses **Gradle Version Catalogs** for centralized dependency management.

All versions are defined in [`gradle/libs.versions.toml`](gradle/libs.versions.toml):

```kotlin
// ✅ Modern approach (type-safe, centralized)
dependencies {
    implementation(libs.ktor.client.core)
    implementation(libs.bundles.koin.common)
}

// ❌ Old approach (hardcoded versions)
dependencies {
    implementation("io.ktor:ktor-client-core:3.0.2")
}
```

**See**: [QUICK_VERSION_CATALOG_GUIDE.md](QUICK_VERSION_CATALOG_GUIDE.md) for usage

## Setup Instructions

### Prerequisites

1. **Development Tools**:
   - Android Studio Hedgehog (2023.1.1) or later
   - Xcode 15.0+ (macOS only)
   - JDK 17
   - CocoaPods (iOS): `sudo gem install cocoapods`

2. **Platform SDKs**:
   - Android SDK (API 35)
   - iOS SDK (15.0+)

### Step 1: Clone and Build

```bash
cd DriverAppKotlin

# Build the shared module
./gradlew :shared:build

# Build Android app
./gradlew :androidApp:assembleDebug

# Generate iOS framework (macOS only)
./gradlew :shared:linkDebugFrameworkIosSimulatorArm64
```

### Step 2: Android Setup

1. **Open** `androidApp` in Android Studio
2. **Add** `google-services.json` to `androidApp/` folder
3. **Update** API URLs in shared module (see Configuration below)
4. **Run** on device or emulator

### Step 3: iOS Setup (macOS Only)

1. **Install CocoaPods dependencies**:
   ```bash
   cd iosApp
   pod install
   ```

2. **Open** `iosApp.xcworkspace` in Xcode (not .xcodeproj!)

3. **Add** Firebase config:
   - Download `GoogleService-Info.plist` from Firebase Console
   - Add to Xcode project

4. **Build and Run** in Xcode

## Configuration

### API Endpoints

Update in `shared/src/commonMain/kotlin/.../di/SharedModule.kt`:

```kotlin
fun sharedModule(baseUrl: String = "https://your-api-server/") = module {
    // ...
}
```

### OIDC Authentication

Update in Android/iOS platform-specific auth implementations.

## Quick Links

- 📖 **[KMP_README.md](KMP_README.md)** - Complete KMP setup guide
- 📋 **[KMP_MIGRATION_SUMMARY.md](KMP_MIGRATION_SUMMARY.md)** - Migration details
- ⚡ **[QUICK_START.md](QUICK_START.md)** - 5-minute setup
- 📦 **[QUICK_VERSION_CATALOG_GUIDE.md](QUICK_VERSION_CATALOG_GUIDE.md)** - Dependency management
- 🔄 **[VERSION_CATALOG_MIGRATION.md](VERSION_CATALOG_MIGRATION.md)** - Complete catalog docs

## Build & Run

### Android
```bash
# Debug
./gradlew :androidApp:assembleDebug

# Release
./gradlew :androidApp:bundleRelease

# Run
./gradlew :androidApp:installDebug
```

### iOS (macOS only)
```bash
# Build framework
./gradlew :shared:linkDebugFrameworkIosSimulatorArm64

# Install pods
cd iosApp && pod install

# Open in Xcode
open iosApp.xcworkspace
```

## Testing

```bash
# Shared tests
./gradlew :shared:allTests

# Android tests
./gradlew :androidApp:testDebugUnitTest

# iOS tests (macOS)
./gradlew :shared:iosSimulatorArm64Test
```

## Troubleshooting

### "Unresolved reference: libs"
1. Sync Gradle: `./gradlew --refresh-dependencies`
2. Invalidate caches in Android Studio

### Android: "Could not find shared module"
```bash
./gradlew :shared:build
```

### iOS: "Framework not found 'shared'"
```bash
./gradlew :shared:linkDebugFrameworkIosSimulatorArm64
cd iosApp && pod install
```

## License

[Your License Here]

## Contributors

[Your Name/Team]

---

**Platform Support:**
- ✅ Android (API 26+)
- ✅ iOS (15.0+)
- 🚧 Desktop (Future with Compose for Desktop)
- 🚧 Web (Future with Compose for Web)
