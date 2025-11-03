# Quick Start Guide - Logistics Driver KMP

Get the Logistics Driver app running on Android and iOS in minutes!

## ⚡ Quick Setup

### For Android (5 minutes)

1. **Open in Android Studio**
   ```bash
   cd DriverAppKotlin
   # Open in Android Studio Hedgehog or later
   ```

2. **Sync Gradle**
   - Let Android Studio sync dependencies (~2-3 minutes)

3. **Add Firebase** (optional for testing)
   ```bash
   # Download google-services.json from Firebase Console
   # Place in: androidApp/google-services.json
   ```

4. **Run**
   - Click "Run" (▶️) or press `Shift+F10`
   - Choose emulator or connected device

### For iOS (10 minutes, macOS only)

1. **Build shared framework**
   ```bash
   cd DriverAppKotlin
   ./gradlew :shared:linkDebugFrameworkIosSimulatorArm64
   ```

2. **Install CocoaPods**
   ```bash
   cd iosApp
   pod install
   ```

3. **Open in Xcode**
   ```bash
   open iosApp.xcworkspace
   ```

4. **Run**
   - Select simulator (iPhone 15)
   - Click "Run" (▶️) or press `⌘R`

## 🛠️ Prerequisites Checklist

### Required for Android
- [ ] JDK 17+
- [ ] Android Studio Hedgehog (2023.1.1+)
- [ ] Android SDK API 35

### Required for iOS (macOS only)
- [ ] Xcode 15.0+
- [ ] CocoaPods: `gem install cocoapods`
- [ ] iOS Simulator or physical device

## 📋 Configuration

### API Endpoints

**File**: `shared/src/commonMain/kotlin/com/jfleets/driver/shared/di/SharedModule.kt`

```kotlin
fun sharedModule(baseUrl: String = "https://10.0.2.2:7000/") = module {
    // Change baseUrl to your API server
}
```

### OIDC Settings

**Android**: `shared/src/androidMain/kotlin/com/jfleets/driver/shared/platform/Platform.android.kt`

**iOS**: `shared/src/iosMain/kotlin/com/jfleets/driver/shared/platform/Platform.ios.kt`

```kotlin
private const val AUTHORITY = "https://your-auth-server"
private const val CLIENT_ID = "logistics.driverapp"
private const val CLIENT_SECRET = "your-secret"
private const val REDIRECT_URI = "logistics-driver://callback"
```

## 🧪 Test Without Backend

The app can run in "demo mode" without a backend:

1. **Mock Repositories** (optional)
   - Comment out real API calls in repositories
   - Return mock data

2. **Skip Authentication**
   - Comment out OIDC flow
   - Navigate directly to Dashboard

## 🐛 Common Issues

### Android

**Issue**: "SDK location not found"
```bash
# Solution: Create local.properties
echo "sdk.dir=/Users/YOUR_USERNAME/Library/Android/sdk" > local.properties
```

**Issue**: "Could not find shared module"
```bash
# Solution: Build shared first
./gradlew :shared:build
```

### iOS

**Issue**: "Framework not found 'shared'"
```bash
# Solution: Build framework and reinstall pods
./gradlew :shared:linkDebugFrameworkIosSimulatorArm64
cd iosApp && pod install
```

**Issue**: "No such module 'shared'"
```bash
# Solution: Clean and rebuild
rm -rf iosApp/Pods iosApp/Podfile.lock
cd iosApp && pod install
```

## 📱 Build Variants

### Android
```bash
# Debug
./gradlew :androidApp:assembleDebug

# Release
./gradlew :androidApp:assembleRelease
```

### iOS
```bash
# Debug (Simulator)
xcodebuild -workspace iosApp.xcworkspace \
           -scheme iosApp \
           -configuration Debug \
           -sdk iphonesimulator

# Release (Device)
xcodebuild -workspace iosApp.xcworkspace \
           -scheme iosApp \
           -configuration Release \
           -sdk iphoneos
```

## 🎯 Next Steps

1. **Read Full Documentation**
   - [KMP_README.md](KMP_README.md) - Complete KMP guide
   - [README.md](README.md) - Project overview

2. **Configure Backend**
   - Update API endpoints
   - Configure OAuth/OIDC
   - Add Firebase configuration

3. **Customize**
   - Update app icons
   - Modify theme colors
   - Add your branding

4. **Deploy**
   - Android: Google Play Console
   - iOS: App Store Connect

## 💡 Tips

- **Shared Code Hot Reload**: Edit shared code and see changes on both platforms
- **Platform-Specific Debugging**: Use native debuggers (Android Studio for Android, Xcode for iOS)
- **Logs**: Use `println()` in shared code, visible in both platform logs
- **Common Errors**: Check [Troubleshooting](KMP_README.md#troubleshooting) section

## 🎓 Learning Resources

- [Kotlin Multiplatform Docs](https://kotlinlang.org/docs/multiplatform.html)
- [Compose Multiplatform](https://www.jetbrains.com/lp/compose-multiplatform/)
- [Ktor Client Guide](https://ktor.io/docs/getting-started-ktor-client-multiplatform-mobile.html)
- [Koin KMP](https://insert-koin.io/docs/reference/koin-mp/kmp)

## 📞 Support

- **Issues**: Check [KMP_README.md Troubleshooting](KMP_README.md#troubleshooting)
- **Questions**: Review [KMP_MIGRATION_SUMMARY.md](KMP_MIGRATION_SUMMARY.md)
- **Bug Reports**: Create GitHub issue with logs

---

**Ready?** Start with `./gradlew :shared:build` and you're good to go! 🚀
