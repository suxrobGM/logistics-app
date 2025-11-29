package com.jfleets.driver.data.local

import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableStateFlow
import platform.Foundation.NSUserDefaults

/**
 * iOS implementation of PreferencesManager using NSUserDefaults.
 */
class IosPreferencesManager : PreferencesManager {
    private val userDefaults = NSUserDefaults.standardUserDefaults

    private companion object {
        const val ACCESS_TOKEN = "access_token"
        const val REFRESH_TOKEN = "refresh_token"
        const val ID_TOKEN = "id_token"
        const val TENANT_ID = "tenant_id"
        const val USER_ID = "user_id"
        const val DRIVER_ID = "driver_id"
        const val TRUCK_ID = "truck_id"
        const val TOKEN_EXPIRY = "token_expiry"
    }

    private val _accessTokenFlow = MutableStateFlow<String?>(null)

    init {
        _accessTokenFlow.value = userDefaults.stringForKey(ACCESS_TOKEN)
    }

    // Access Token
    override suspend fun saveAccessToken(token: String) {
        userDefaults.setObject(token, ACCESS_TOKEN)
        _accessTokenFlow.value = token
    }

    override suspend fun getAccessToken(): String? {
        return userDefaults.stringForKey(ACCESS_TOKEN)
    }

    override val accessTokenFlow: Flow<String?> = _accessTokenFlow

    // Refresh Token
    override suspend fun saveRefreshToken(token: String) {
        userDefaults.setObject(token, REFRESH_TOKEN)
    }

    override suspend fun getRefreshToken(): String? {
        return userDefaults.stringForKey(REFRESH_TOKEN)
    }

    // ID Token
    override suspend fun saveIdToken(token: String) {
        userDefaults.setObject(token, ID_TOKEN)
    }

    override suspend fun getIdToken(): String? {
        return userDefaults.stringForKey(ID_TOKEN)
    }

    // Tenant ID
    override suspend fun saveTenantId(tenantId: String) {
        userDefaults.setObject(tenantId, TENANT_ID)
    }

    override suspend fun getTenantId(): String? {
        return userDefaults.stringForKey(TENANT_ID)
    }

    // User ID
    override suspend fun saveUserId(userId: String) {
        userDefaults.setObject(userId, USER_ID)
    }

    override suspend fun getUserId(): String? {
        return userDefaults.stringForKey(USER_ID)
    }

    // Driver ID
    override suspend fun saveDriverId(driverId: String) {
        userDefaults.setObject(driverId, DRIVER_ID)
    }

    override suspend fun getDriverId(): String? {
        return userDefaults.stringForKey(DRIVER_ID)
    }

    // Truck ID
    override suspend fun saveTruckId(truckId: String) {
        userDefaults.setObject(truckId, TRUCK_ID)
    }

    override suspend fun getTruckId(): String? {
        return userDefaults.stringForKey(TRUCK_ID)
    }

    // Token Expiry
    override suspend fun saveTokenExpiry(expiry: Long) {
        userDefaults.setObject(expiry.toString(), TOKEN_EXPIRY)
    }

    override suspend fun getTokenExpiry(): Long? {
        return userDefaults.stringForKey(TOKEN_EXPIRY)?.toLongOrNull()
    }

    // Clear all data
    override suspend fun clearAll() {
        userDefaults.removeObjectForKey(ACCESS_TOKEN)
        userDefaults.removeObjectForKey(REFRESH_TOKEN)
        userDefaults.removeObjectForKey(ID_TOKEN)
        userDefaults.removeObjectForKey(TENANT_ID)
        userDefaults.removeObjectForKey(USER_ID)
        userDefaults.removeObjectForKey(DRIVER_ID)
        userDefaults.removeObjectForKey(TRUCK_ID)
        userDefaults.removeObjectForKey(TOKEN_EXPIRY)
        _accessTokenFlow.value = null
    }

    // Check if user is logged in
    override suspend fun isLoggedIn(): Boolean {
        return getAccessToken() != null
    }
}
