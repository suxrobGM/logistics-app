package com.jfleets.driver.platform

import kotlin.time.Instant

// Platform-specific expect declarations

expect class PlatformSettings() {
    suspend fun getString(key: String): String?
    suspend fun putString(key: String, value: String)
    suspend fun remove(key: String)
    suspend fun clear()
}

expect class LocationTracker() {
    fun startTracking()
    fun stopTracking()
    suspend fun getCurrentLocation(): LocationData?
}

expect class NotificationManager() {
    fun showNotification(title: String, message: String)
    suspend fun requestPermission(): Boolean
}

expect class AuthManager() {
    suspend fun login(): AuthResult?
    suspend fun logout()
    suspend fun getAccessToken(): String?
    suspend fun refreshToken(refreshToken: String): AuthResult?
}

data class LocationData(
    val latitude: Double,
    val longitude: Double,
    val altitude: Double? = null,
    val accuracy: Float? = null
)

data class AuthResult(
    val accessToken: String,
    val refreshToken: String?,
    val idToken: String?,
    val expiresIn: Int
)

// Platform name
expect fun getPlatformName(): String

// Formatting utilities
expect fun Instant.formatShort(): String
expect fun Instant.formatDateTime(): String
