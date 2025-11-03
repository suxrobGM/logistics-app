# Version Catalog Migration Guide

This document explains the migration from hardcoded dependency versions to Gradle Version Catalogs using `libs.versions.toml`.

## What Changed

### Before (Hardcoded Versions)

**Root `build.gradle.kts`**:
```kotlin
plugins {
    id("com.android.application") version "8.7.3" apply false
    id("org.jetbrains.kotlin.multiplatform") version "2.1.0" apply false
}
```

**Module `build.gradle.kts`**:
```kotlin
dependencies {
    implementation("io.ktor:ktor-client-core:3.0.2")
    implementation("org.jetbrains.kotlinx:kotlinx-coroutines-core:1.9.0")
}
```

### After (Version Catalog)

**Root `build.gradle.kts`**:
```kotlin
plugins {
    alias(libs.plugins.android.application) apply false
    alias(libs.plugins.kotlin.multiplatform) apply false
}
```

**Module `build.gradle.kts`**:
```kotlin
dependencies {
    implementation(libs.ktor.client.core)
    implementation(libs.kotlinx.coroutines.core)
}
```

**New File: `gradle/libs.versions.toml`**:
```toml
[versions]
kotlin = "2.1.0"
ktor = "3.0.2"
kotlinx-coroutines = "1.9.0"

[libraries]
ktor-client-core = { module = "io.ktor:ktor-client-core", version.ref = "ktor" }
kotlinx-coroutines-core = { module = "org.jetbrains.kotlinx:kotlinx-coroutines-core", version.ref = "kotlinx-coroutines" }

[plugins]
android-application = { id = "com.android.application", version.ref = "agp" }
kotlin-multiplatform = { id = "org.jetbrains.kotlin.multiplatform", version.ref = "kotlin" }
```

## Benefits

### 1. **Centralized Version Management**
All dependency versions are defined in one place (`libs.versions.toml`), making updates easier and preventing version conflicts.

### 2. **Type-Safe Accessors**
IDE autocompletion and compile-time checking for dependencies:
```kotlin
// ✅ Type-safe - IDE autocomplete works
implementation(libs.ktor.client.core)

// ❌ Old way - typos only caught at runtime
implementation("io.ktor:ktor-client-core:3.0.2")
```

### 3. **Dependency Bundles**
Group related dependencies together:
```kotlin
// Define bundle in libs.versions.toml
[bundles]
ktor-common = ["ktor-client-core", "ktor-client-logging", "ktor-client-auth"]

// Use in build.gradle.kts
implementation(libs.bundles.ktor.common)
```

### 4. **Easier Version Updates**
Update a version in one place, affects all modules:
```toml
[versions]
ktor = "3.0.3"  # Update here, affects entire project
```

### 5. **Gradle Version Updates**
Gradle can automatically update versions with `./gradlew versionCatalogUpdate` (with plugin).

## Version Catalog Structure

### `libs.versions.toml` Sections

#### 1. **[versions]** - Version variables
```toml
[versions]
kotlin = "2.1.0"
compose-plugin = "1.7.1"
ktor = "3.0.2"
```

#### 2. **[libraries]** - Dependency declarations
```toml
[libraries]
ktor-client-core = { module = "io.ktor:ktor-client-core", version.ref = "ktor" }
```

**Usage in build.gradle.kts**:
```kotlin
implementation(libs.ktor.client.core)
```

**Naming Convention**: `group.artifact` → `group-artifact` (dashes, not dots)

#### 3. **[plugins]** - Gradle plugins
```toml
[plugins]
android-application = { id = "com.android.application", version.ref = "agp" }
kotlin-multiplatform = { id = "org.jetbrains.kotlin.multiplatform", version.ref = "kotlin" }
```

**Usage**:
```kotlin
plugins {
    alias(libs.plugins.android.application)
}
```

#### 4. **[bundles]** - Dependency groups
```toml
[bundles]
ktor-common = ["ktor-client-core", "ktor-client-logging", "ktor-client-auth"]
voyager-common = ["voyager-navigator", "voyager-koin", "voyager-transitions"]
```

**Usage**:
```kotlin
implementation(libs.bundles.ktor.common)
```

## Our Version Catalog

### Key Version Groups

| Group | Version | Libraries |
|-------|---------|-----------|
| **Kotlin** | 2.1.0 | kotlin-stdlib, coroutines, serialization |
| **Compose** | 1.7.1 | Compose Multiplatform |
| **Ktor** | 3.0.2 | All Ktor client modules |
| **Koin** | 4.0.0 | Dependency injection |
| **Voyager** | 1.1.0-beta03 | Navigation |
| **AndroidX** | Various | Core, Lifecycle, DataStore, etc. |

### Defined Bundles

1. **ktor-common**: All common Ktor dependencies
   - `ktor-client-core`
   - `ktor-client-content-negotiation`
   - `ktor-serialization-json`
   - `ktor-client-logging`
   - `ktor-client-auth`

2. **koin-common**: Koin for multiplatform
   - `koin-core`
   - `koin-compose`
   - `koin-compose-viewmodel`

3. **voyager-common**: Voyager navigation
   - `voyager-navigator`
   - `voyager-screenmodel`
   - `voyager-bottom-sheet`
   - `voyager-tab-navigator`
   - `voyager-transitions`
   - `voyager-koin`

4. **androidx-compose**: Compose Android
   - `androidx-activity-compose`

5. **maps**: Google Maps
   - `play-services-maps`
   - `maps-compose`
   - `maps-compose-utils`

## Migration Checklist

- [x] Create `gradle/libs.versions.toml`
- [x] Migrate root `build.gradle.kts` plugins
- [x] Migrate `shared/build.gradle.kts` dependencies
- [x] Migrate `androidApp/build.gradle.kts` dependencies
- [x] Test Gradle sync
- [x] Verify all dependencies resolve

## How to Add New Dependencies

### 1. Add Version (if new)
```toml
[versions]
coil = "2.7.0"
```

### 2. Add Library Declaration
```toml
[libraries]
coil-compose = { module = "io.coil-kt:coil-compose", version.ref = "coil" }
```

### 3. Use in Module
```kotlin
dependencies {
    implementation(libs.coil.compose)
}
```

## How to Update Versions

### Update Single Library
Edit `gradle/libs.versions.toml`:
```toml
[versions]
ktor = "3.0.3"  # Update version here
```

All modules using Ktor will get the new version automatically.

### Update with Gradle Plugin (Recommended)

Add to root `build.gradle.kts`:
```kotlin
plugins {
    id("nl.littlerobots.version-catalog-update") version "0.8.4"
}
```

Run:
```bash
./gradlew versionCatalogUpdate
```

This will check for dependency updates and suggest them.

## Best Practices

### ✅ Do's

1. **Group related versions**:
   ```toml
   [versions]
   androidx-lifecycle = "2.8.7"

   [libraries]
   androidx-lifecycle-runtime = { module = "androidx.lifecycle:lifecycle-runtime-ktx", version.ref = "androidx-lifecycle" }
   androidx-lifecycle-viewmodel = { module = "androidx.lifecycle:lifecycle-viewmodel-ktx", version.ref = "androidx-lifecycle" }
   ```

2. **Use bundles for common sets**:
   ```toml
   [bundles]
   retrofit = ["retrofit-core", "retrofit-gson", "okhttp-logging"]
   ```

3. **Keep naming consistent**:
   - Use kebab-case: `ktor-client-core`
   - Match artifact structure: `group.artifact` → `group-artifact`

### ❌ Don'ts

1. **Don't hardcode versions in build files**:
   ```kotlin
   // ❌ Bad
   implementation("io.ktor:ktor-client-core:3.0.2")

   // ✅ Good
   implementation(libs.ktor.client.core)
   ```

2. **Don't duplicate version numbers**:
   ```toml
   # ❌ Bad - duplicates version
   [libraries]
   lib1 = "group:artifact:1.0.0"
   lib2 = "group:artifact2:1.0.0"

   # ✅ Good - references version
   [versions]
   mylib = "1.0.0"
   [libraries]
   lib1 = { module = "group:artifact", version.ref = "mylib" }
   lib2 = { module = "group:artifact2", version.ref = "mylib" }
   ```

## Troubleshooting

### Issue: "Unresolved reference: libs"

**Cause**: Gradle hasn't synced the version catalog.

**Solution**:
1. Click "Sync Now" in Android Studio
2. Or run: `./gradlew --refresh-dependencies`

### Issue: "Could not find libs.plugins.xyz"

**Cause**: Plugin not defined in `[plugins]` section.

**Solution**: Add to `libs.versions.toml`:
```toml
[plugins]
xyz = { id = "com.example.xyz", version = "1.0.0" }
```

### Issue: Autocomplete not working

**Cause**: IDE indexing or cache issue.

**Solution**:
1. File → Invalidate Caches → Invalidate and Restart
2. Reimport Gradle project

## Resources

- [Gradle Version Catalogs Official Docs](https://docs.gradle.org/current/userguide/platforms.html)
- [Gradle Version Catalog Update Plugin](https://github.com/littlerobots/version-catalog-update-plugin)
- [Version Catalog Best Practices](https://developer.android.com/build/migrate-to-catalogs)

## Summary

The migration to Version Catalogs provides:

✅ **Centralized dependency management**
✅ **Type-safe dependency accessors**
✅ **Easier version updates**
✅ **Better IDE support**
✅ **Reduced duplication**
✅ **Cleaner build files**

All dependencies are now managed through `gradle/libs.versions.toml`, making the project easier to maintain and update.
