# Logistics Driver App - Kotlin Multiplatform (KMP)

A cross-platform driver application built with **Kotlin Multiplatform (KMP)** and **Compose Multiplatform**, running on both Android and iOS from a single codebase.

## 🎯 Overview

This is the cross-platform version of the Logistics Driver App, migrated from .NET MAUI and then enhanced with KMP to support both Android and iOS with maximum code sharing.

### Architecture

```
┌─────────────────────────────────────────────────────┐
│                   iosApp (iOS)                       │
│              SwiftUI + Kotlin Framework              │
└──────────────────────┬──────────────────────────────┘
                       │
┌──────────────────────┴──────────────────────────────┐
│              shared (KMP Module)                     │
│  ┌────────────────────────────────────────────────┐ │
│  │         commonMain (Shared Code)               │ │
│  │  • Domain Models                               │ │
│  │  • Business Logic                              │ │
│  │  • API Client (Ktor)                           │ │
│  │  • Repositories                                │ │
│  │  • Compose Multiplatform UI                    │ │
│  └────────────────────────────────────────────────┘ │
│          ↓              ↓              ↓             │
│  ┌──────────┐   ┌──────────┐   ┌──────────┐        │
│  │androidMain│   │  iosMain │   │commonMain│        │
│  │Platform  │   │Platform  │   │  Tests   │        │
│  │Specific  │   │Specific  │   │          │        │
│  └──────────┘   └──────────┘   └──────────┘        │
└──────────────────────┬──────────────────────────────┘
                       │
┌──────────────────────┴──────────────────────────────┐
│               androidApp (Android)                   │
│          Jetpack Compose + Android SDK               │
└─────────────────────────────────────────────────────┘
```

## 📦 Project Structure

```
DriverAppKotlin/
├── shared/                          # KMP Shared Module
│   ├── src/
│   │   ├── commonMain/              # Platform-independent code
│   │   │   └── kotlin/com/jfleets/driver/shared/
│   │   │       ├── data/            # Data layer
│   │   │       │   ├── api/         # Ktor API clients
│   │   │       │   ├── dto/         # Data Transfer Objects
│   │   │       │   ├── mapper/      # DTO ↔ Domain mappers
│   │   │       │   └── repository/  # Repository implementations
│   │   │       ├── domain/          # Business logic
│   │   │       │   └── model/       # Domain models
│   │   │       ├── presentation/    # UI layer (Compose MP)
│   │   │       ├── platform/        # Platform interfaces
│   │   │       └── di/              # Koin DI modules
│   │   ├── androidMain/             # Android-specific code
│   │   │   └── kotlin/com/jfleets/driver/shared/platform/
│   │   │       └── Platform.android.kt
│   │   └── iosMain/                 # iOS-specific code
│   │       └── kotlin/com/jfleets/driver/shared/platform/
│   │           └── Platform.ios.kt
│   └── build.gradle.kts
│
├── androidApp/                      # Android Application
│   ├── src/main/
│   │   ├── java/com/jfleets/driver/
│   │   │   ├── service/             # Android services
│   │   │   ├── DriverApplication.kt
│   │   │   └── MainActivity.kt
│   │   ├── res/
│   │   └── AndroidManifest.xml
│   └── build.gradle.kts
│
├── iosApp/                          # iOS Application
│   ├── iosApp/
│   │   ├── ContentView.swift
│   │   ├── DriverApp.swift
│   │   └── Info.plist
│   ├── iosApp.xcodeproj
│   ├── Podfile
│   └── build.gradle.kts
│
├── build.gradle.kts                 # Root build config
├── settings.gradle.kts              # Module configuration
└── gradle.properties
```

## 🔧 Technology Stack

### Shared (commonMain)
- **Kotlin**: 2.1.0
- **Compose Multiplatform**: 1.7.1
- **Ktor Client**: 3.0.2 (HTTP networking)
- **Kotlinx Serialization**: 1.7.3
- **Kotlinx Datetime**: 0.6.1
- **Kotlinx Coroutines**: 1.9.0
- **Koin**: 4.0.0 (Dependency Injection)
- **Voyager**: 1.1.0 (Navigation)
- **Multiplatform Settings**: 1.2.0

### Android-Specific
- **Target SDK**: 35 (Android 15)
- **Min SDK**: 26 (Android 8.0)
- **Firebase**: Cloud Messaging, Analytics
- **Google Maps**: 19.0.0
- **AppAuth**: 0.11.1 (OIDC)
- **DataStore**: Preferences storage

### iOS-Specific
- **iOS Version**: 15.0+
- **Xcode**: 15.0+
- **CocoaPods**: For dependency management
- **Swift**: Interop with Kotlin framework

## 🚀 Setup Instructions

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

## ⚙️ Configuration

### API Endpoints

Update in `shared/src/commonMain/kotlin/.../di/SharedModule.kt`:

```kotlin
fun sharedModule(baseUrl: String = "https://your-api-server/") = module {
    // ...
}
```

### OIDC Authentication

Update in Android/iOS platform-specific auth implementations:

```kotlin
// shared/src/androidMain/.../platform/Platform.android.kt
private const val AUTHORITY = "https://your-auth-server"
private const val CLIENT_ID = "logistics.driverapp"
private const val CLIENT_SECRET = "your-client-secret"
private const val REDIRECT_URI = "logistics-driver://callback"
```

## 🔄 Code Sharing Strategy

### ✅ Shared Code (~80%)
- Domain models
- Business logic
- API clients (Ktor)
- Repositories
- Data mappers
- ViewModels/ScreenModels
- Most UI (Compose Multiplatform)

### 📱 Platform-Specific Code (~20%)
- **Android**:
  - Foreground location service
  - Firebase Cloud Messaging service
  - OAuth browser flow
  - DataStore implementation

- **iOS**:
  - CLLocationManager integration
  - APNs push notifications
  - OAuth via ASWebAuthenticationSession
  - UserDefaults implementation

### expect/actual Pattern

```kotlin
// commonMain
expect class PlatformSettings() {
    suspend fun getString(key: String): String?
    suspend fun putString(key: String, value: String)
}

// androidMain
actual class PlatformSettings(private val context: Context) {
    private val dataStore = context.dataStore
    actual suspend fun getString(key: String): String? {
        // DataStore implementation
    }
}

// iosMain
actual class PlatformSettings {
    private val userDefaults = NSUserDefaults.standardUserDefaults
    actual suspend fun getString(key: String): String? {
        // UserDefaults implementation
    }
}
```

## 📲 Platform-Specific Features

### Android
- Background location tracking with foreground service
- Firebase Cloud Messaging for push notifications
- Google Maps integration
- Material 3 theming
- App shortcuts and widgets (future)

### iOS
- Background location updates
- Apple Push Notification Service (APNs)
- Apple Maps integration
- SF Symbols and native iOS controls
- Siri shortcuts (future)

## 🧪 Testing

### Shared Tests
```bash
# Run common tests
./gradlew :shared:allTests

# Android-specific tests
./gradlew :androidApp:testDebugUnitTest

# iOS-specific tests (macOS only)
./gradlew :shared:iosSimulatorArm64Test
```

### UI Tests
```bash
# Android instrumented tests
./gradlew :androidApp:connectedAndroidTest

# iOS UI tests - run in Xcode
```

## 📦 Building for Release

### Android Release APK/AAB

```bash
# Build release APK
./gradlew :androidApp:assembleRelease

# Build release AAB (for Play Store)
./gradlew :androidApp:bundleRelease
```

### iOS Release IPA

1. Open in Xcode
2. Product → Archive
3. Distribute App → App Store Connect

## 🔐 Code Signing

### Android
Add to `local.properties`:
```properties
RELEASE_STORE_FILE=../path/to/keystore.jks
RELEASE_STORE_PASSWORD=your_password
RELEASE_KEY_ALIAS=your_alias
RELEASE_KEY_PASSWORD=your_key_password
```

### iOS
Configure in Xcode:
1. Signing & Capabilities
2. Select your Team
3. Automatic signing or manual provisioning profile

## 🐛 Troubleshooting

### Android

**Issue**: "Could not find shared module"
```bash
# Solution: Build shared module first
./gradlew :shared:build
```

**Issue**: Firebase not initialized
```bash
# Solution: Ensure google-services.json is in androidApp/
ls androidApp/google-services.json
```

### iOS

**Issue**: "Framework not found 'shared'"
```bash
# Solution: Build iOS framework
cd iosApp
pod install
./gradlew :shared:embedAndSignAppleFrameworkForXcode
```

**Issue**: "Module 'shared' not found"
```bash
# Solution: Clean and rebuild
rm -rf iosApp/Pods iosApp/Podfile.lock
cd iosApp && pod install
```

### General

**Issue**: Ktor SSL errors
- For development, trust self-signed certificates in device settings
- For production, use proper SSL certificates

**Issue**: Multiplatform Settings not working
```kotlin
// Ensure proper initialization in each platform
// Android: Provide context
// iOS: Use NSUserDefaults directly
```

## 🎨 UI Architecture

### Compose Multiplatform

The UI is built entirely with Compose Multiplatform, sharing most UI code between platforms:

```kotlin
// shared/src/commonMain/.../presentation/screens/Dashboard.kt
@Composable
fun DashboardScreen(viewModel: DashboardViewModel = koinViewModel()) {
    val state by viewModel.state.collectAsState()

    Scaffold(
        topBar = { DashboardTopBar() }
    ) { padding ->
        // Shared UI code
    }
}
```

### Platform-Specific UI

When needed, use `expect`/`actual`:

```kotlin
// commonMain
@Composable
expect fun MapView(latitude: Double, longitude: Double)

// androidMain
@Composable
actual fun MapView(latitude: Double, longitude: Double) {
    GoogleMap(/* ... */)
}

// iosMain
@Composable
actual fun MapView(latitude: Double, longitude: Double) {
    // Apple Maps UIViewRepresentable
}
```

## 📊 Code Sharing Metrics

- **Shared Code**: ~80% (business logic, models, repos, most UI)
- **Android-Specific**: ~10% (services, platform APIs)
- **iOS-Specific**: ~10% (native integrations)

### Breakdown
- **Domain Models**: 100% shared
- **Data Layer**: 100% shared
- **Business Logic**: 100% shared
- **UI**: 90% shared (platform-specific maps, auth)
- **Services**: 0% shared (platform-specific implementations)

## 🚀 Benefits of KMP

1. **Code Reuse**: Write once, run on Android and iOS
2. **Type Safety**: Kotlin's type system across platforms
3. **Native Performance**: Compiles to native code
4. **Incremental Migration**: Can adopt gradually
5. **Platform Access**: Full access to native APIs via expect/actual

## 📚 Resources

- [Kotlin Multiplatform Docs](https://kotlinlang.org/docs/multiplatform.html)
- [Compose Multiplatform](https://www.jetbrains.com/lp/compose-multiplatform/)
- [Ktor Client](https://ktor.io/docs/client.html)
- [Koin Multiplatform](https://insert-koin.io/docs/reference/koin-mp/kmp)
- [Voyager Navigation](https://voyager.adriel.cafe/)

## 🤝 Contributing

When contributing to the KMP project:

1. Add shared code to `commonMain` when possible
2. Use `expect`/`actual` for platform-specific code
3. Test on both Android and iOS
4. Update documentation for platform differences

## 📄 License

[Your License Here]

---

**Platform Support:**
- ✅ Android (API 26+)
- ✅ iOS (15.0+)
- 🚧 Desktop (Future)
- 🚧 Web (Future with Compose for Web)
