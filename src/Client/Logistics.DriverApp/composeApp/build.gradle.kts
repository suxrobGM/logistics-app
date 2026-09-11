import org.jetbrains.kotlin.gradle.tasks.KotlinCompilationTask

plugins {
    alias(libs.plugins.kotlinMultiplatform)
    alias(libs.plugins.androidKotlinMultiplatformLibrary)
    alias(libs.plugins.kotlinSerialization)
    alias(libs.plugins.jetbrainsCompose)
    alias(libs.plugins.composeCompiler)
    alias(libs.plugins.openApiGenerator)
}

// Generates from the checked-in Angular spec by default, so no running API is needed.
// Pass -PopenApiSpecUrl=<url> to regenerate against a live instance instead.
openApiGenerate {
    generatorName.set("kotlin")
    val specUrl = providers.gradleProperty("openApiSpecUrl").orNull
    if (specUrl != null) {
        remoteInputSpec.set(specUrl)
    } else {
        inputSpec.set(
            rootProject.layout.projectDirectory
                .file("../Logistics.Angular/openapi.json").asFile.absolutePath
        )
    }
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

            // Uppercase `File` resolves to java.io.File, not the parameter.
            content = content.replace("append(File)", "append(file)")

            // A doubled constructor call on the HashMap supertype.
            content = content.replace(">()() {", ">() {")

            // additionalProperties generates a HashMap supertype, which is final in Kotlin.
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

    listOf(
        iosArm64(),
        iosSimulatorArm64()
    ).forEach { iosTarget ->
        iosTarget.binaries.framework {
            baseName = "ComposeApp"
            isStatic = true
        }
    }

    sourceSets.commonMain {
        kotlin.srcDir(layout.buildDirectory.dir("generated/openapi/src/main/kotlin"))
    }

    sourceSets {
        commonMain.dependencies {
            implementation(libs.bundles.compose)

            implementation(libs.bundles.jetbrains.compose.multiplatform)

            implementation(libs.kotlinx.coroutines.core)
            implementation(libs.kotlinx.serialization.json)
            implementation(libs.kotlinx.datetime)

            implementation(libs.androidx.datastore)
            implementation(libs.androidx.datastore.preferences)

            implementation(libs.bundles.ktor.common)

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

            // Used by LocationTracker.android.kt.
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

// Everything downstream of the generated sources has to wait for them. AGP also treats the
// generated `src/main` folder as a source-set root and scans a sibling baselineProfiles path,
// so its ART profile tasks need the same ordering or Gradle fails validation.
tasks.withType<KotlinCompilationTask<*>>().configureEach {
    dependsOn(tasks.named("openApiGenerate"))
}

tasks.matching { it.name.endsWith("ArtProfile") }.configureEach {
    dependsOn(tasks.named("openApiGenerate"))
}
