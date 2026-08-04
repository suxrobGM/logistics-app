plugins {
    alias(libs.plugins.androidApplication)
    alias(libs.plugins.googleServices)
    alias(libs.plugins.firebaseCrashlytics)
    alias(libs.plugins.composeCompiler)
}

// Env-only release signing; the config is registered only when the secrets exist so IDE sync
// and debug builds work without them.
val releaseStorePassword: String? = System.getenv("KEYSTORE_PASSWORD")
val releaseKeyPassword: String? = System.getenv("KEY_PASSWORD")
val hasReleaseSigningSecrets = releaseStorePassword != null && releaseKeyPassword != null

if (!hasReleaseSigningSecrets) {
    // Checked against the resolved graph, not the typed task names: `./gradlew build` packages a
    // release too, and :composeApp's iOS `link*Release*` tasks need no keystore.
    gradle.taskGraph.whenReady {
        val packagesRelease = allTasks.any {
            it.project == project && it.name.startsWith("package") && it.name.endsWith("Release")
        }
        if (packagesRelease) {
            error(
                "KEYSTORE_PASSWORD and KEY_PASSWORD must be set in the environment to build a signed " +
                    "release of :androidApp"
            )
        }
    }
}

android {
    namespace = "com.logisticsx.driver"
    compileSdk = 37

    defaultConfig {
        applicationId = "com.logisticsx.driver"
        minSdk = 26
        targetSdk = 36
        versionCode = 4
        versionName = "1.0.0"

        vectorDrawables {
            useSupportLibrary = true
        }
    }

    flavorDimensions += "environment"
    // Keep URLs in sync with iosApp/Configuration/{Dev,Prod}.xcconfig.
    productFlavors {
        create("dev") {
            dimension = "environment"
            versionNameSuffix = "-dev"
            buildConfigField("String", "API_BASE_URL", "\"http://10.0.2.2:7000\"")
            buildConfigField("String", "IDENTITY_SERVER_URL", "\"http://10.0.2.2:7001\"")
            manifestPlaceholders["allowCleartext"] = "true"
        }
        create("prod") {
            dimension = "environment"
            buildConfigField("String", "API_BASE_URL", "\"https://api.logisticsx.app\"")
            buildConfigField("String", "IDENTITY_SERVER_URL", "\"https://id.logisticsx.app\"")
            manifestPlaceholders["allowCleartext"] = "false"
        }
    }

    signingConfigs {
        if (hasReleaseSigningSecrets) {
            create("release") {
                storeFile = rootProject.file("release-keystore.jks")
                storePassword = releaseStorePassword
                keyAlias = "release"
                keyPassword = releaseKeyPassword
            }
        }
    }

    buildTypes {
        release {
            isMinifyEnabled = true
            isShrinkResources = true
            signingConfig = signingConfigs.findByName("release")
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
            ndk {
                debugSymbolLevel = "FULL"
            }
        }
        debug {
            isMinifyEnabled = false
        }
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
    // Depend on the shared KMP library module
    implementation(project(":composeApp"))

    // Android Compose
    implementation(libs.androidx.activity.compose)

    // AndroidX Core
    implementation(libs.androidx.core.ktx)

    // Koin Android
    implementation(platform(libs.koin.bom))
    implementation(libs.koin.core)
    implementation(libs.koin.android)
    implementation(libs.koin.androidx.compose)

    // Firebase
    implementation(platform(libs.firebase.bom))
    implementation(libs.firebase.crashlytics)
    implementation(libs.firebase.messaging)

    // Google Play Services & Maps
    implementation(libs.play.services.location)
    implementation(libs.bundles.maps)
}
