---
paths:
  - "src/Client/Logistics.DriverApp/**/*.kt"
---

# Kotlin Driver App Conventions

Kotlin Multiplatform mobile app for truck drivers using Compose Multiplatform.

## Project Structure

```text
src/Client/Logistics.DriverApp/
├── composeApp/
│   ├── src/
│   │   ├── commonMain/kotlin/com/jfleets/driver/
│   │   │   ├── api/              # ApiFactory and generated API clients
│   │   │   ├── model/            # Domain models, extensions, settings
│   │   │   ├── navigation/       # Routes, Navigator, entry provider
│   │   │   ├── service/          # Services (auth, location, messaging)
│   │   │   ├── ui/
│   │   │   │   ├── components/   # Reusable UI components
│   │   │   │   ├── screens/      # Screen composables
│   │   │   │   └── theme/        # Colors, typography, theme
│   │   │   ├── util/             # Extension functions, utilities
│   │   │   ├── viewmodel/        # ViewModels with UI state
│   │   │   └── Module.kt         # Koin DI module
│   │   ├── androidMain/          # Android-specific implementations
│   │   └── iosMain/              # iOS-specific implementations (Kotlin)
│   └── build.gradle.kts
├── androidApp/                   # Android app entry point
│   └── src/main/kotlin/          # MainActivity, Services
└── iosApp/                       # iOS app entry point (Swift/Xcode)
    ├── iosApp.xcodeproj/         # Xcode project
    ├── Configuration/            # Build config (Config.xcconfig)
    └── iosApp/
        ├── iOSApp.swift          # App entry point
        ├── ContentView.swift     # Root SwiftUI view
        ├── LocationService.swift # Native location service
        ├── SignalRBridge.swift   # SignalR native bridge
        └── Assets.xcassets/      # App icons, colors
```

## Libraries & Tech Stack

| Category | Library |
|----------|---------|
| UI | Compose Multiplatform (Material3) |
| Navigation | Navigation 3 (type-safe, experimental) |
| DI | Koin (multiplatform) |
| Networking | Ktor Client |
| Serialization | kotlinx.serialization |
| State | StateFlow, collectAsState() |
| ViewModel | JetBrains Lifecycle ViewModel |
| Storage | DataStore Preferences |
| API | OpenAPI Generator (auto-generated) |

## API Layer

### Generated APIs

APIs are auto-generated from the backend's `swagger.json` using OpenAPI Generator:

```kotlin
// Generated location: build/generated/openapi/src/main/kotlin/
// Package: com.jfleets.driver.api (clients) and com.jfleets.driver.api.models (DTOs)

// Usage via ApiFactory (registered in Koin)
class ApiFactory(baseUrl: String, preferencesManager: PreferencesManager) {
    val loadApi: LoadApi by lazy { LoadApi(baseUrl, httpClient) }
    val tripApi: TripApi by lazy { TripApi(baseUrl, httpClient) }
    // ... other APIs
}
```

### Regenerate APIs

```bash
cd src/Client/Logistics.DriverApp
./gradlew openApiGenerate
```

### API Conventions

- APIs return `Response<T>` - use `.body()` to get the data
- Include `X-Tenant` header via PreferencesManager
- Handle 401 via AuthEventBus for automatic logout

## Dependency Injection (Koin)

### Module Registration (Module.kt)

```kotlin
fun commonModule(baseUrl: String) = module {
    // Singletons
    singleOf(::PreferencesManager)
    single { ApiFactory(baseUrl, get()) }
    single<LoadApi> { get<ApiFactory>().loadApi }

    // ViewModels (auto-wired dependencies)
    viewModelOf(::DashboardViewModel)
    viewModelOf(::TripsViewModel)

    // ViewModels with parameters
    viewModel { params ->
        TripDetailViewModel(
            tripApi = get(),
            tripId = params.get<String>()
        )
    }
}
```

### Injection in Composables

```kotlin
// Auto-wired ViewModel (no parameters)
@Composable
fun TripsScreen(
    viewModel: TripsViewModel = koinViewModel()
)

// Manual injection for parameterized ViewModels
@Composable
fun TripDetailScreen(viewModel: TripDetailViewModel) // Passed from navigation

// Inject services directly
val tripApi: TripApi = koinInject()
```

## Navigation (Navigation 3)

### Route Definitions (Screen.kt)

```kotlin
@Serializable
data object DashboardRoute : NavKey

@Serializable
data class TripDetailRoute(val tripId: String) : NavKey

// Top-level routes for bottom navigation
val topLevelRoutes: Set<NavKey> = setOf(
    DashboardRoute,
    TripsRoute,
    MessagesRoute,
    PastLoadsRoute,
    AccountRoute
)
```

### Entry Provider (Navigation.kt)

```kotlin
@Composable
fun createEntryProvider(
    navigator: Navigator,
    onOpenUrl: (String) -> Unit
): (NavKey) -> NavEntry<NavKey> = entryProvider {
    // Simple route
    entry<TripsRoute> {
        TripsScreen(
            onTripClick = { tripId -> navigator.navigate(TripDetailRoute(tripId)) }
        )
    }

    // Route with parameters - inject ViewModel manually
    entry<TripDetailRoute> { key ->
        val tripApi: TripApi = koinInject()
        val viewModel = TripDetailViewModel(tripApi, key.tripId)
        TripDetailScreen(viewModel = viewModel, ...)
    }
}
```

### Navigation Actions

```kotlin
navigator.navigate(TripDetailRoute(tripId))           // Push route
navigator.goBack()                                     // Pop back
navigator.clearAndNavigate(LoginRoute)                 // Clear stack
navigator.navigateAndClear(DashboardRoute, LoginRoute) // Replace route
```

## ViewModel Pattern

### Structure

```kotlin
class TripsViewModel(
    private val tripApi: TripApi,
    private val preferencesManager: PreferencesManager
) : ViewModel() {

    private val _uiState = MutableStateFlow<TripsUiState>(TripsUiState.Loading)
    val uiState: StateFlow<TripsUiState> = _uiState.asStateFlow()

    init {
        loadTrips()
    }

    private fun loadTrips() {
        viewModelScope.launch {
            _uiState.value = TripsUiState.Loading
            try {
                val result = tripApi.getTrips(orderBy = "-CreatedAt").body()
                _uiState.value = TripsUiState.Success(result?.items ?: emptyList())
            } catch (e: Exception) {
                _uiState.value = TripsUiState.Error(e.message ?: "Error")
            }
        }
    }

    fun refresh() = loadTrips()
}

// Sealed class for UI states
sealed class TripsUiState {
    object Loading : TripsUiState()
    data class Success(val trips: List<TripDto>) : TripsUiState()
    data class Error(val message: String) : TripsUiState()
}
```

## UI Components

### Screen Structure

```kotlin
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun TripDetailScreen(
    onNavigateBack: () -> Unit,      // Navigation callbacks
    onLoadClick: (String) -> Unit,
    viewModel: TripDetailViewModel
) {
    val uiState by viewModel.uiState.collectAsState()

    Scaffold(
        topBar = {
            AppTopBar(
                title = "Trip Details",
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, "Back")
                    }
                }
            )
        }
    ) { paddingValues ->
        when (val state = uiState) {
            is TripDetailUiState.Loading -> LoadingIndicator()
            is TripDetailUiState.Success -> TripContent(state.trip, ...)
            is TripDetailUiState.Error -> ErrorView(state.message, onRetry = { viewModel.refresh() })
        }
    }
}
```

### Reusable Components

| Component | Purpose |
|-----------|---------|
| `CardContainer` | Styled card wrapper with elevation |
| `SectionCard` | Card with title header |
| `DetailRow` | Label-value pair display |
| `LoadingIndicator` | Centered progress indicator |
| `ErrorView` | Error message with retry button |
| `EmptyStateView` | Empty list placeholder |
| `AppTopBar` | Consistent top app bar |

### Component Example

```kotlin
@Composable
fun SectionCard(
    title: String,
    modifier: Modifier = Modifier,
    content: @Composable () -> Unit = {}
) {
    CardContainer(modifier = modifier) {
        Column(modifier = Modifier.padding(16.dp)) {
            Text(
                text = title,
                style = MaterialTheme.typography.titleMedium,
                fontWeight = FontWeight.Bold
            )
            Spacer(modifier = Modifier.height(12.dp))
            content()
        }
    }
}
```

## Platform-Specific Code (expect/actual)

### Declaration in commonMain

```kotlin
// util/CurrencyExtensions.kt
expect fun Double.formatCurrency(): String

// util/DistanceExtensions.kt
expect fun Double.formatDistance(unit: DistanceUnit): String
```

### Implementation in androidMain/iosMain

```kotlin
// androidMain/.../CurrencyExtensions.android.kt
actual fun Double.formatCurrency(): String {
    return NumberFormat.getCurrencyInstance(Locale.US).format(this)
}

// iosMain/.../CurrencyExtensions.ios.kt
actual fun Double.formatCurrency(): String {
    val formatter = NSNumberFormatter()
    formatter.numberStyle = NSNumberFormatterCurrencyStyle
    return formatter.stringFromNumber(NSNumber(this)) ?: "$${this}"
}
```

## Backend API Conventions

### OrderBy Format

Use `-PropertyName` prefix for descending order:

```kotlin
// Correct
tripApi.getTrips(orderBy = "-CreatedAt")  // Descending by CreatedAt
tripApi.getTrips(orderBy = "Name")         // Ascending by Name

// Wrong - will cause backend error
tripApi.getTrips(orderBy = "createdAt desc")
```

### Pagination

```kotlin
val result = tripApi.getTrips(
    page = 1,
    pageSize = 20,
    orderBy = "-CreatedAt"
).body()

val items = result?.items ?: emptyList()
val totalCount = result?.totalCount ?: 0
```

## Common Patterns

### Pull-to-Refresh

```kotlin
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun TripsScreen(viewModel: TripsViewModel = koinViewModel()) {
    val uiState by viewModel.uiState.collectAsState()
    val isRefreshing = uiState is TripsUiState.Loading

    PullToRefreshBox(
        isRefreshing = isRefreshing,
        onRefresh = { viewModel.refresh() }
    ) {
        // Content
    }
}
```

### User Settings (CompositionLocal)

```kotlin
// Access user settings anywhere in composition
val userSettings = LocalUserSettings.current
val distanceUnit = userSettings.distanceUnit
```

### Extension Functions for DTOs

```kotlin
// model/DtoExtensions.kt
fun EmployeeDto.fullName(): String = "$firstName $lastName"
fun TruckDto.driversList: List<EmployeeDto> get() = listOfNotNull(mainDriver, coDriver)
fun Address.toDisplayString(): String = "$line1, $city, $state $postalCode"
```

## Build Commands

```bash
# Build Android
./gradlew assembleDebug

# Build iOS (requires Mac)
./gradlew :composeApp:linkDebugFrameworkIosArm64

# Regenerate API clients
./gradlew openApiGenerate

# Clean build
./gradlew clean build
```
