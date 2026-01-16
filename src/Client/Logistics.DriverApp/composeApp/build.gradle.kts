plugins {
    alias(libs.plugins.kotlinMultiplatform)
    alias(libs.plugins.androidKotlinMultiplatformLibrary)
    alias(libs.plugins.kotlinSerialization)
    alias(libs.plugins.jetbrainsCompose)
    alias(libs.plugins.composeCompiler)
    alias(libs.plugins.openApiGenerator)
}

// OpenAPI Generator Configuration
openApiGenerate {
    generatorName.set("kotlin")
    remoteInputSpec.set("http://localhost:7000/swagger/v1/swagger.json")
    outputDir.set(layout.buildDirectory.dir("generated/openapi").get().asFile.absolutePath)
    packageName.set("com.jfleets.driver")
    apiPackage.set("com.jfleets.driver.api")
    modelPackage.set("com.jfleets.driver.api.models")
    configOptions.set(
        mapOf(
            "library" to "multiplatform",
            "useCoroutines" to "true",
            "enumPropertyNaming" to "UPPERCASE",
            "dateLibrary" to "kotlinx-datetime"
        )
    )
}

kotlin {
    androidLibrary {
        namespace = "com.jfleets.driver.shared"
        compileSdk = 36
        minSdk = 26
    }

    sourceSets.all {
        languageSettings {
            optIn("kotlin.time.ExperimentalTime")
            optIn("io.ktor.utils.io.InternalAPI")
        }
    }

    listOf(
        iosX64(),
        iosArm64(),
        iosSimulatorArm64()
    ).forEach { iosTarget ->
        iosTarget.binaries.framework {
            baseName = "ComposeApp"
            isStatic = true
        }
    }

    // Add generated OpenAPI sources to commonMain
    sourceSets.commonMain {
        kotlin.srcDir(layout.buildDirectory.dir("generated/openapi/src/main/kotlin"))
    }

    sourceSets {
        commonMain.dependencies {
            // Compose Multiplatform (using compose accessor - deprecated but functional)
            implementation(compose.runtime)
            implementation(compose.foundation)
            implementation(compose.material3)
            implementation(compose.materialIconsExtended)
            implementation(compose.ui)
            implementation(compose.components.resources)
            implementation(compose.components.uiToolingPreview)

            // JetBrains Compose Multiplatform (Lifecycle, ViewModel, Navigation)
            implementation(libs.bundles.jetbrains.compose.multiplatform)

            // Kotlin Libraries
            implementation(libs.kotlinx.coroutines.core)
            implementation(libs.kotlinx.serialization.json)
            implementation(libs.kotlinx.datetime)

            // DataStore
            implementation(libs.androidx.datastore)
            implementation(libs.androidx.datastore.preferences)

            // Ktor Client (bundles)
            implementation(libs.bundles.ktor.common)

            // Koin (multiplatform)
            implementation(project.dependencies.platform(libs.koin.bom))
            implementation(libs.koin.core)
            implementation(libs.koin.compose)
            implementation(libs.koin.compose.viewmodel)
        }

        androidMain.dependencies {
            // Android Compose
            implementation(libs.androidx.activity.compose)

            // Android-specific Ktor
            implementation(libs.ktor.client.okhttp)

            // AndroidX Core
            implementation(libs.androidx.core.ktx)

            // Koin Android
            implementation(libs.koin.androidx.compose)

            // Google Play Services Location (for LocationTracker.android.kt)
            implementation(libs.play.services.location)

            // SignalR Client
            implementation(libs.signalr.client)
        }

        iosMain.dependencies {
            implementation(libs.ktor.client.darwin)
        }
    }
}

// Apply OpenAPI generator post-processing fix
apply(from = "../gradle/openapi-fix.gradle.kts")

// Ensure OpenAPI code is generated before Kotlin compilation
//tasks.withType<KotlinCompile>().configureEach {
//    dependsOn(tasks.named("openApiGenerate"))
//}
