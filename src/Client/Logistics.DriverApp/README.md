# Logistics Driver App - Kotlin Multiplatform

A cross-platform driver application built with **Kotlin Multiplatform (KMP)** and **Compose Multiplatform**, running on both **Android** and **iOS** from a single codebase.

> **Cross-Platform**: Share ~90% of code between Android and iOS
> **Native Performance**: Compiled to native code for each platform
> **Modern UI**: Compose Multiplatform for consistent UX

**Migrated from**: .NET MAUI → **KMP (Android + iOS)**

## Features

- **Authentication**: Login and secure session management using ROPC flow
- **Dashboard**: View truck information and active loads
- **Load Management**: View load details, confirm pickups and deliveries
- **Statistics**: Driver performance metrics with interactive charts
- **Past Loads**: Historical load data for the past 90 days
- **Account Management**: Update user profile information
- **Real-time Updates**: SignalR for live load status updates
- **Real-time Location Tracking**: Background location service with proximity detection
- **Push Notifications**: Firebase Cloud Messaging for load updates
- **Maps Integration**: Google Maps for route visualization

## Tech Stack

### Core Technologies

| Technology | Version |
|------------|---------|
| Kotlin | 2.2.21 |
| Compose Multiplatform | 1.9.2 |
| Ktor | 3.3.1 |
| Koin | 4.1.0 |
| Kotlinx Serialization | 1.9.0 |
| Kotlinx Coroutines | 1.10.2 |

### Android Platform

- **Min SDK**: 26 (Android 8.0)
- **Target SDK**: 36 (Android 16)
- **Firebase BOM**: 34.5.0 (Cloud Messaging)
- **Google Maps**: Maps Compose 6.12.1
- **SignalR**: 10.0.0

### iOS Platform

- **iOS Version**: 15.0+
- **Xcode**: 15.0+
- **Ktor Darwin**: Native networking
- **Swift Interop**: Native iOS integration

## Project Structure

```text
Logistics.DriverApp/
├── composeApp/                     # KMP Application Module
│   ├── src/
│   │   ├── commonMain/             # Shared code (~90%)
│   │   │   └── kotlin/com/jfleets/driver/
│   │   │       ├── api/            # API clients & networking
│   │   │       ├── model/          # Domain models
│   │   │       ├── navigation/     # Navigation routes & graphs
│   │   │       ├── permission/     # Permission handling
│   │   │       ├── service/        # Business services
│   │   │       ├── ui/             # Compose UI screens & components
│   │   │       ├── util/           # Utilities & extensions
│   │   │       ├── viewmodel/      # ViewModels (MVVM)
│   │   │       └── Module.kt       # Koin DI module
│   │   │
│   │   ├── androidMain/            # Android-specific code
│   │   │   └── kotlin/             # Platform implementations
│   │   │
│   │   ├── iosMain/                # iOS-specific code
│   │   │   └── kotlin/             # Platform implementations
│   │   │
│   │   └── openapi/                # OpenAPI spec for code generation
│   │       └── api-spec.json
│   │
│   └── build.gradle.kts
│
├── iosApp/                         # iOS Application
│   ├── iosApp.xcodeproj/
│   ├── iosApp/
│   └── Configuration/
│
├── docs/                           # Documentation
│   ├── project-proposal.pdf
│   └── uml/
│
├── gradle/
│   └── libs.versions.toml          # Centralized dependency versions
│
├── build.gradle.kts
└── settings.gradle.kts
```

## Architecture

The app follows **Clean Architecture** with **MVVM** pattern:

- **UI Layer**: Compose Multiplatform screens with Material 3
- **ViewModel Layer**: State management with JetBrains Lifecycle ViewModel
- **Service Layer**: Business logic and data orchestration
- **API Layer**: Ktor-based HTTP clients with OpenAPI-generated models
- **Navigation**: JetBrains Navigation Compose for type-safe navigation

### Dependency Injection

Koin is used for multiplatform DI, configured in `Module.kt` with platform-specific modules in `androidMain` and `iosMain`.

### API Generation

API clients and models are auto-generated from OpenAPI spec using the OpenAPI Generator Gradle plugin (v7.10.0).
