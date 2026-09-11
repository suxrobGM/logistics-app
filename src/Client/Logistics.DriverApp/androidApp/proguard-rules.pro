# Everything kept here is reached reflectively, so R8 cannot see the usage and would strip it.
# Adding a serialized type outside the packages below means adding a keep for it too.

-keepattributes *Annotation*, InnerClasses
-dontnote kotlinx.serialization.AnnotationsKt
-keepclassmembers class kotlinx.serialization.json.** {
    *** Companion;
}
-keepclasseswithmembers class kotlinx.serialization.json.** {
    kotlinx.serialization.KSerializer serializer(...);
}

-keepclassmembers @kotlinx.serialization.Serializable class ** {
    *** Companion;
    kotlinx.serialization.KSerializer serializer(...);
}

-keep class io.ktor.** { *; }
-keep class kotlinx.coroutines.** { *; }
-dontwarn kotlinx.atomicfu.**

# Ktor's shared modules reference its server engines, which this app never links.
-dontwarn io.netty.**
-dontwarn com.typesafe.**
-dontwarn org.slf4j.**
-dontwarn java.lang.management.ManagementFactory
-dontwarn java.lang.management.RuntimeMXBean

-keep class com.logisticsx.driver.api.models.** { *; }
-keep class com.logisticsx.driver.model.** { *; }

# Serialized payloads that sit outside the two packages above.
-keep class com.logisticsx.driver.service.realtime.TruckGeolocation { *; }
-keep class com.logisticsx.driver.service.auth.TokenResponse { *; }
-keep class com.logisticsx.driver.service.auth.TokenErrorResponse { *; }

-keep class com.microsoft.signalr.** { *; }
-dontwarn com.microsoft.signalr.**
