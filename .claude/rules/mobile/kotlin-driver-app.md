---
paths:
  - "src/Client/Logistics.DriverApp/**/*.kt"
---

# Kotlin Driver App Conventions

Kotlin Multiplatform + Compose Multiplatform. Source root is
`composeApp/src/commonMain/kotlin/com/logisticsx/driver/`, with `androidMain/` and `iosMain/`
for expect/actual. The folder layout and the library set are self-evident from `ls` and
`gradle/libs.versions.toml` - what follows is what isn't.

## API layer

- Clients are **generated** from the backend swagger: `./gradlew openApiGenerate`. Never hand-edit
  anything under `com.logisticsx.driver.api` - regenerate.
- Reach them through `ApiFactory` (Koin singletons). Calls return `Response<T>`; `.body()` unwraps.
- Every request must carry the **`X-Tenant` header** sourced from `PreferencesManager`. Without it the
  API resolves no tenant and the call fails on the server, not the client. It is added by the
  `TenantHeader` plugin in `ApiFactory`, not by `defaultRequest` - that block is not suspend.
- Ktor's `Auth` bearer plugin owns token refresh: one refresh per 401, de-duplicated across
  concurrent calls, and `AuthEventBus` on failure so logout happens once. Don't handle 401 per-call.
- Install `Auth` in `ApiFactory` only. `AuthService` shares `HttpClientFactory` for `/connect/token`,
  so installing it there makes refresh re-enter itself.
- Sort params use the API's `-PropertyName` syntax (see `.claude/rules/backend/csharp-conventions.md`).

## Koin DI

- Register in `Module.kt`: `singleOf(::Service)`, `viewModelOf(::ViewModel)`.
- Parameterized VMs: `viewModel { params -> VM(get(), params.get<String>()) }`.
- In composables: `koinViewModel()` for VMs, `koinInject()` for services.

## Navigation 3

- Routes are `@Serializable data object XRoute : NavKey` (or a `data class` when parameterized).
- Bottom-nav destinations must also be in the `topLevelRoutes` set - a route missing from it still
  navigates but loses its tab state.
- Entry provider maps routes via `entry<XRoute> { ... }`.

## ViewModel + UI

- `MutableStateFlow<UiState>` + `asStateFlow()`; sealed `Loading` / `Success(data)` / `Error(message)`.
  Load in `init {}`, expose `refresh()` for pull-to-refresh.
- Screen composables take navigation callbacks as parameters and the ViewModel **last**.
- Reuse `CardContainer`, `SectionCard`, `DetailRow`, `EmptyStateView`, `AppTopBar` before writing new chrome.
- Currency/distance formatting is expect/actual (`formatCurrency()`, `formatDistance()`) - don't inline
  platform formatting in a composable.

## Icons

- Icons come from `AppIcons`, vendored in `ui/icons/`. There is no `material-icons` dependency -
  don't add one.
- To add an icon, copy its source file from compose-material-icons 1.7.3 into `ui/icons/` and add a
  line to `AppIcons`. Never hand-write path data.
- An icon that must flip in right-to-left layouts has to come from the upstream `automirrored` set;
  `autoMirror = true` in the copied file is what flips it.
