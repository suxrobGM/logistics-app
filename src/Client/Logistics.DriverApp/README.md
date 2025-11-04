# Logistics Driver App - Kotlin Multiplatform

A cross-platform driver application built with **Kotlin Multiplatform (KMP)** and **Compose Multiplatform**, running on both **Android** and **iOS** from a single codebase.

> **Cross-Platform**: Share ~80% of code between Android and iOS
> **Native Performance**: Compiled to native code for each platform
> **Modern UI**: Compose Multiplatform for consistent UX

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
- **Kotlin Multiplatform**
- **Compose Multiplatform**
- **Ktor Client**
- **Kotlinx Serialization**
- **Kotlinx Coroutines**
- **Kotlinx DateTime**
- **Koin**
- **Voyager**
- **Multiplatform Settings**

### Android Platform
- **Min SDK**: 26 (Android 8.0)
- **Target SDK**: 36 (Android 16)
- **Firebase**: Cloud Messaging, Analytics
- **Google Maps**: Maps Compose 6.2.1
- **AppAuth**: OIDC
- **DataStore**: Preferences storage
- **Timber**: Logging

### iOS Platform
- **iOS Version**: 15.0+
- **Xcode**: 15.0+
- **CocoaPods**: Dependency management
- **Swift Interop**: Native iOS integration

## Project Structure

### KMP Architecture

```
Logistics.DriverApp/
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

## Setup Instructions

### Prerequisites

1. **Development Tools**:
   - Android Studio Hedgehog
   - Xcode (macOS only)
   - JDK 17
   - CocoaPods (iOS): `sudo gem install cocoapods`

2. **Platform SDKs**:
   - Android SDK (API 36)
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
