plugins {
    alias(libs.plugins.androidApplication)
    alias(libs.plugins.googleServices)
    alias(libs.plugins.firebaseCrashlytics)
    alias(libs.plugins.composeCompiler)
}

android {
    namespace = "com.logisticsx.driver"
    compileSdk = 36

    defaultConfig {
        applicationId = "com.logisticsx.driver"
        minSdk = 26
        targetSdk = 36
        versionCode = 1
        versionName = "1.0.0"

        vectorDrawables {
            useSupportLibrary = true
        }
    }

    flavorDimensions += "environment"
    productFlavors {
        create("dev") {
            dimension = "environment"
            versionNameSuffix = "-dev"
            buildConfigField("String", "API_BASE_URL", "\"http://10.0.2.2:7000/\"")
            buildConfigField("String", "IDENTITY_SERVER_URL", "\"http://10.0.2.2:7001/\"")
            manifestPlaceholders["allowCleartext"] = "true"
        }
        create("staging") {
            dimension = "environment"
            versionNameSuffix = "-staging"
            buildConfigField("String", "API_BASE_URL", "\"https://api.logisticsx.app/\"")
            buildConfigField("String", "IDENTITY_SERVER_URL", "\"https://id.logisticsx.app/\"")
            manifestPlaceholders["allowCleartext"] = "false"
        }
        create("prod") {
            dimension = "environment"
            buildConfigField("String", "API_BASE_URL", "\"https://api.logisticsx.app/\"")
            buildConfigField("String", "IDENTITY_SERVER_URL", "\"https://id.logisticsx.app/\"")
            manifestPlaceholders["allowCleartext"] = "false"
        }
    }

    signingConfigs {
        create("release") {
            val keystoreFile = providers.gradleProperty("RELEASE_KEYSTORE_FILE").orNull
            if (keystoreFile != null) {
                storeFile = file(keystoreFile)
                storePassword = providers.gradleProperty("RELEASE_KEYSTORE_PASSWORD").get()
                keyAlias = providers.gradleProperty("RELEASE_KEY_ALIAS").get()
                keyPassword = providers.gradleProperty("RELEASE_KEY_PASSWORD").get()
            }
        }
    }

    buildTypes {
        release {
            isMinifyEnabled = true
            signingConfig = signingConfigs.findByName("release")
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
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
