import org.jetbrains.kotlin.gradle.dsl.JvmTarget
import org.jetbrains.kotlin.gradle.tasks.KotlinCompile

plugins {
    alias(libs.plugins.kotlin.multiplatform)
    alias(libs.plugins.android.application)
    alias(libs.plugins.google.services)
    alias(libs.plugins.kotlin.serialization)
    alias(libs.plugins.jetbrains.compose)
    alias(libs.plugins.compose.compiler)
    alias(libs.plugins.openapi.generator)
}

// OpenAPI Generator Configuration
openApiGenerate {
    generatorName.set("kotlin")
    inputSpec.set("$projectDir/src/openapi/api-spec.json")
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
    // Apply expect-actual-classes flag to all targets
    targets.all {
        compilations.all {
            compileTaskProvider.configure {
                compilerOptions {
                    freeCompilerArgs.add("-Xexpect-actual-classes")
                }
            }
        }
    }

    androidTarget {
        compilerOptions {
            jvmTarget.set(JvmTarget.JVM_17)
            freeCompilerArgs.add("-opt-in=kotlin.time.ExperimentalTime")
        }
    }

    sourceSets.all {
        languageSettings {
            optIn("kotlin.time.ExperimentalTime")
            optIn("io.ktor.util.InternalAPI")
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
            // Compose Multiplatform
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
            implementation(libs.androidx.datastore.core)

            // Ktor Client (bundles)
            implementation(libs.bundles.ktor.common)

            // Koin (multiplatform)
            implementation(project.dependencies.platform(libs.koin.bom))
            implementation(libs.koin.core)
            implementation(libs.koin.compose)
            implementation(libs.koin.compose.viewmodel)
        }

        androidMain.dependencies {
            // Android Compose (Activity integration only - UI provided by JetBrains Compose)
            implementation(compose.preview)
            implementation(libs.androidx.activity.compose)

            // Android-specific Ktor
            implementation(libs.ktor.client.okhttp)

            // AndroidX Core
            implementation(libs.androidx.core.ktx)

            // DataStore Android
            implementation(libs.androidx.datastore.preferences)

            // Koin Android
            implementation(libs.koin.androidx.compose)

            // Firebase
            implementation(project.dependencies.platform(libs.firebase.bom))
            implementation(libs.firebase.messaging)
            implementation(libs.firebase.analytics)

            // Google Play Services & Maps
            implementation(libs.play.services.location)
            implementation(libs.bundles.maps)

            // Authentication
            implementation(libs.appauth)
            implementation(libs.jwt.decode)
        }

        iosMain.dependencies {
            // iOS-specific Ktor
            implementation(libs.ktor.client.darwin)
        }

        commonTest.dependencies {
            implementation(libs.kotlin.test)
        }
    }
}

android {
    namespace = "com.jfleets.driver"
    compileSdk = 36

    defaultConfig {
        applicationId = "com.jfleets.driver"
        minSdk = 26
        targetSdk = 36
        versionCode = 1
        versionName = "1.0.0"

        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
        vectorDrawables {
            useSupportLibrary = true
        }

        // Must match AuthService.REDIRECT_URI scheme so AppAuth can receive callbacks.
        manifestPlaceholders["appAuthRedirectScheme"] = "logistics-driver"
    }

    buildTypes {
        release {
            isMinifyEnabled = true
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
        }
        debug {
            isMinifyEnabled = false
        }
    }

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }

    buildFeatures {
        compose = true
        buildConfig = true
    }

    packaging {
        resources {
            excludes += "/META-INF/{AL2.0,LGPL2.1}"
        }
    }
}

dependencies {
    // Compose Tooling (for Previews) - from JetBrains Compose
    debugImplementation(compose.uiTooling)

    // Testing
    testImplementation(libs.junit)
    androidTestImplementation(libs.androidx.test.junit)
    androidTestImplementation(libs.androidx.test.espresso)
}

// Ensure OpenAPI code is generated before Kotlin compilation
tasks.withType<KotlinCompile>().configureEach {
    dependsOn(tasks.named("openApiGenerate"))
}
