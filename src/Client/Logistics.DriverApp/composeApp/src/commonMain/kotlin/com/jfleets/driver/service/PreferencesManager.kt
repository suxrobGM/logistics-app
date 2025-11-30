package com.jfleets.driver.service

import kotlinx.coroutines.flow.Flow

/**
 * Multiplatform interface for storing user preferences and tokens.
 * Platform-specific implementations use DataStore (Android) and NSUserDefaults (iOS).
 */
interface PreferencesManager {
    // Access Token
    suspend fun saveAccessToken(token: String)
    suspend fun getAccessToken(): String?
    val accessTokenFlow: Flow<String?>

    // Refresh Token
    suspend fun saveRefreshToken(token: String)
    suspend fun getRefreshToken(): String?

    // ID Token
    suspend fun saveIdToken(token: String)
    suspend fun getIdToken(): String?

    // Tenant ID
    suspend fun saveTenantId(tenantId: String)
    suspend fun getTenantId(): String?

    // User ID
    suspend fun saveUserId(userId: String)
    suspend fun getUserId(): String?

    // Driver ID
    suspend fun saveDriverId(driverId: String)
    suspend fun getDriverId(): String?

    // Truck ID
    suspend fun saveTruckId(truckId: String)
    suspend fun getTruckId(): String?

    // Token Expiry
    suspend fun saveTokenExpiry(expiry: Long)
    suspend fun getTokenExpiry(): Long?

    // Clear all data
    suspend fun clearAll()

    // Check if user is logged in
    suspend fun isLoggedIn(): Boolean
}