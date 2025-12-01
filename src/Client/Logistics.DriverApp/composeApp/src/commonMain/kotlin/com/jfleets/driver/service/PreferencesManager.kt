package com.jfleets.driver.service

import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.longPreferencesKey
import androidx.datastore.preferences.core.stringPreferencesKey
import kotlinx.coroutines.flow.first

/**
 * Cross-platform preferences manager using DataStore.
 * Only the DataStore instance creation is platform-specific.
 */
class PreferencesManager(private val dataStore: DataStore<Preferences>) {

    private companion object {
        val ACCESS_TOKEN = stringPreferencesKey("access_token")
        val REFRESH_TOKEN = stringPreferencesKey("refresh_token")
        val ID_TOKEN = stringPreferencesKey("id_token")
        val TENANT_ID = stringPreferencesKey("tenant_id")
        val USER_ID = stringPreferencesKey("user_id")
        val TRUCK_ID = stringPreferencesKey("truck_id")
        val TOKEN_EXPIRY = longPreferencesKey("token_expiry")
    }

    // Access Token
    suspend fun saveAccessToken(token: String) {
        dataStore.edit { prefs -> prefs[ACCESS_TOKEN] = token }
    }

    suspend fun getAccessToken(): String? {
        return dataStore.data.first()[ACCESS_TOKEN]
    }

    // Refresh Token
    suspend fun saveRefreshToken(token: String) {
        dataStore.edit { prefs -> prefs[REFRESH_TOKEN] = token }
    }

    suspend fun getRefreshToken(): String? {
        return dataStore.data.first()[REFRESH_TOKEN]
    }

    // ID Token
    suspend fun saveIdToken(token: String) {
        dataStore.edit { prefs -> prefs[ID_TOKEN] = token }
    }

    suspend fun getIdToken(): String? {
        return dataStore.data.first()[ID_TOKEN]
    }

    // Tenant ID
    suspend fun saveTenantId(tenantId: String) {
        dataStore.edit { prefs -> prefs[TENANT_ID] = tenantId }
    }

    suspend fun getTenantId(): String? {
        return dataStore.data.first()[TENANT_ID]
    }

    // User ID
    suspend fun saveUserId(userId: String) {
        dataStore.edit { prefs -> prefs[USER_ID] = userId }
    }

    suspend fun getUserId(): String? {
        return dataStore.data.first()[USER_ID]
    }

    // Truck ID
    suspend fun saveTruckId(truckId: String) {
        dataStore.edit { prefs -> prefs[TRUCK_ID] = truckId }
    }

    suspend fun getTruckId(): String? {
        return dataStore.data.first()[TRUCK_ID]
    }

    // Token Expiry
    suspend fun saveTokenExpiry(expiry: Long) {
        dataStore.edit { prefs -> prefs[TOKEN_EXPIRY] = expiry }
    }

    suspend fun getTokenExpiry(): Long? {
        return dataStore.data.first()[TOKEN_EXPIRY]
    }

    // Clear all data
    suspend fun clearAll() {
        dataStore.edit { prefs -> prefs.clear() }
    }
}
