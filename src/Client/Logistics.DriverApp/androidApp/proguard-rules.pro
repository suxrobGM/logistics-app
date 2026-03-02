# Add project specific ProGuard rules here.
# You can control the set of applied configuration files using the
# proguardFiles setting in build.gradle.

# Kotlin Serialization
-keepattributes *Annotation*, InnerClasses
-dontnote kotlinx.serialization.AnnotationsKt
-keepclassmembers class kotlinx.serialization.json.** {
    *** Companion;
}
-keepclasseswithmembers class kotlinx.serialization.json.** {
    kotlinx.serialization.KSerializer serializer(...);
}

# Keep all @Serializable classes and their serializers
-keepclassmembers @kotlinx.serialization.Serializable class ** {
    *** Companion;
    kotlinx.serialization.KSerializer serializer(...);
}

# Ktor
-keep class io.ktor.** { *; }
-keep class kotlinx.coroutines.** { *; }
-dontwarn kotlinx.atomicfu.**
-dontwarn io.netty.**
-dontwarn com.typesafe.**
-dontwarn org.slf4j.**

# OpenAPI generated API models
-keep class com.logisticsx.driver.api.models.** { *; }

# App models
-keep class com.logisticsx.driver.model.** { *; }

# Service models
-keep class com.logisticsx.driver.service.realtime.TruckGeolocation { *; }
-keep class com.logisticsx.driver.service.auth.TokenResponse { *; }
-keep class com.logisticsx.driver.service.auth.TokenErrorResponse { *; }

# SignalR client
-keep class com.microsoft.signalr.** { *; }
-dontwarn com.microsoft.signalr.**
