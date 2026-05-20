import org.jetbrains.kotlin.gradle.tasks.KotlinCompilationTask

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
    remoteInputSpec.set(
        providers.gradleProperty("openApiSpecUrl")
            .getOrElse("http://localhost:7000/swagger/v1/swagger.json")
    )
    outputDir.set(layout.buildDirectory.dir("generated/openapi").get().asFile.absolutePath)
    packageName.set("com.logisticsx.driver")
    apiPackage.set("com.logisticsx.driver.api")
    modelPackage.set("com.logisticsx.driver.api.models")
    configOptions.set(
        mapOf(
            "library" to "multiplatform",
            "useCoroutines" to "true",
            "enumPropertyNaming" to "UPPERCASE",
            "dateLibrary" to "kotlinx-datetime"
        )
    )
}

// Post-process OpenAPI-generated Kotlin code to fix known generator bugs for the KMP target.
val openApiGeneratedDir: Provider<Directory> =
    layout.buildDirectory.dir("generated/openapi/src/main/kotlin")

tasks.named("openApiGenerate") {
    val generatedDirProvider = openApiGeneratedDir
    doLast {
        val generatedDir = generatedDirProvider.get().asFile
        if (!generatedDir.exists()) return@doLast

        val hashMapPattern = Regex("""\)\s*:\s*kotlin\.collections\.HashMap<[^>]+>\(\)\s*\{""")
        generatedDir.walkTopDown().filter { it.extension == "kt" }.forEach { file ->
            val original = file.readText()
            var content = original

            // Fix 1: `append(File)` -> `append(file)` (uppercase File references java.io.File, not the parameter)
            content = content.replace("append(File)", "append(file)")

            // Fix 2: Double constructor call `()()` on HashMap inheritance
            content = content.replace(">()() {", ">() {")

            // Fix 3: Remove HashMap inheritance (final class in Kotlin, generated for additionalProperties)
            content = hashMapPattern.replace(content, ") {")

            if (content != original) {
                file.writeText(content)
                logger.lifecycle("openapi-fix: patched ${file.name}")
            }
        }
    }
}

kotlin {
    compilerOptions {
        freeCompilerArgs.add("-Xexpect-actual-classes")
    }

    android {
        namespace = "com.logisticsx.driver.shared"
        compileSdk = 37
        minSdk = 26

        // Required for Compose Multiplatform resources in Android library targets (AGP 8.8.0+)
        androidResources.enable = true
    }

    sourceSets.all {
        languageSettings {
            optIn("kotlin.time.ExperimentalTime")
            optIn("io.ktor.utils.io.InternalAPI")
        }
    }

    listOf(
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
            // Compose Multiplatform (direct artifacts — 'compose.X' shortcuts deprecated in 1.11)
            implementation(libs.bundles.compose)
            implementation(libs.compose.material.icons.extended)

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
            implementation(libs.androidx.activity.compose)
            implementation(libs.ktor.client.okhttp)
            implementation(libs.androidx.core.ktx)
            implementation(libs.koin.androidx.compose)

            // Google Play Services Location (for LocationTracker.android.kt)
            implementation(libs.play.services.location)

            implementation(libs.signalr.client)
            implementation(libs.bundles.camerax)
            implementation(libs.mlkit.barcode)
        }

        iosMain.dependencies {
            implementation(libs.ktor.client.darwin)
        }
    }
}

// Ensure OpenAPI sources are generated before any Kotlin compilation
tasks.withType<KotlinCompilationTask<*>>().configureEach {
    dependsOn(tasks.named("openApiGenerate"))
}
