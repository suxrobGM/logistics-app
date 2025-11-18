package com.jfleets.driver

import android.content.Context
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.map
import kotlinx.datetime.TimeZone
import kotlinx.datetime.toLocalDateTime
import java.time.format.DateTimeFormatter
import kotlin.time.Instant

private val Context.dataStore: DataStore<Preferences> by preferencesDataStore(name = "driver_settings")

actual class PlatformSettings {
    actual constructor()

    private lateinit var context: Context
    private val dataStore: DataStore<Preferences> by lazy { context.dataStore }

    constructor(context: Context) : this() {
        this.context = context
    }

    actual suspend fun getString(key: String): String? {
        val prefKey = stringPreferencesKey(key)
        return dataStore.data.map { prefs -> prefs[prefKey] }.first()
    }

    actual suspend fun putString(key: String, value: String) {
        val prefKey = stringPreferencesKey(key)
        dataStore.edit { prefs -> prefs[prefKey] = value }
    }

    actual suspend fun remove(key: String) {
        val prefKey = stringPreferencesKey(key)
        dataStore.edit { prefs -> prefs.remove(prefKey) }
    }

    actual suspend fun clear() {
        dataStore.edit { prefs -> prefs.clear() }
    }
}

actual class LocationTracker {
    actual fun startTracking() {
        // Implementation in Android-specific service
    }

    actual fun stopTracking() {
        // Implementation in Android-specific service
    }

    actual suspend fun getCurrentLocation(): LocationData? {
        // Implementation in Android-specific code
        return null
    }
}

actual class NotificationManager {
    actual fun showNotification(title: String, message: String) {
        // Implementation in Android-specific code
    }

    actual suspend fun requestPermission(): Boolean {
        // Implementation in Android-specific code
        return true
    }
}

actual class AuthManager {
    actual suspend fun login(): AuthResult? {
        // Implementation in Android-specific code using AppAuth
        return null
    }

    actual suspend fun logout() {
        // Implementation in Android-specific code
    }

    actual suspend fun getAccessToken(): String? {
        // Implementation in Android-specific code
        return null
    }

    actual suspend fun refreshToken(refreshToken: String): AuthResult? {
        // Implementation in Android-specific code
        return null
    }
}

actual fun getPlatformName(): String = "Android"

actual fun Instant.formatShort(): String {
    val localDate = this.toLocalDateTime(TimeZone.currentSystemDefault()).date
    val javaInstant =
        java.time.Instant.ofEpochSecond(this.epochSeconds, this.nanosecondsOfSecond.toLong())
    val formatter = DateTimeFormatter.ofPattern("MMM dd, yyyy")
    return formatter.format(javaInstant)
}

actual fun Instant.formatDateTime(): String {
    val javaInstant =
        java.time.Instant.ofEpochSecond(this.epochSeconds, this.nanosecondsOfSecond.toLong())
    val formatter = DateTimeFormatter.ofPattern("MMM dd, yyyy HH:mm")
    return formatter.format(javaInstant)
}
