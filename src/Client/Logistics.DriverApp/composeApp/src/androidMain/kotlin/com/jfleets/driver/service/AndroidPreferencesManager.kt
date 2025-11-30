package com.jfleets.driver.service

import android.content.Context
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.map

private val Context.dataStore: DataStore<Preferences> by preferencesDataStore(name = "driver_prefs")

class AndroidPreferencesManager(context: Context) : PreferencesManager {
    private val dataStore = context.dataStore

    companion object {
        val ACCESS_TOKEN = stringPreferencesKey("access_token")
        val REFRESH_TOKEN = stringPreferencesKey("refresh_token")
        val ID_TOKEN = stringPreferencesKey("id_token")
        val TENANT_ID = stringPreferencesKey("tenant_id")
        val USER_ID = stringPreferencesKey("user_id")
        val DRIVER_ID = stringPreferencesKey("driver_id")
        val TRUCK_ID = stringPreferencesKey("truck_id")
        val TOKEN_EXPIRY = stringPreferencesKey("token_expiry")
    }

    // Access Token
    override suspend fun saveAccessToken(token: String) {
        dataStore.edit { prefs -> prefs[ACCESS_TOKEN] = token }
    }

    override suspend fun getAccessToken(): String? {
        return dataStore.data.first()[ACCESS_TOKEN]
    }

    override val accessTokenFlow: Flow<String?> = dataStore.data.map { prefs ->
        prefs[ACCESS_TOKEN]
    }

    // Refresh Token
    override suspend fun saveRefreshToken(token: String) {
        dataStore.edit { prefs -> prefs[REFRESH_TOKEN] = token }
    }

    override suspend fun getRefreshToken(): String? {
        return dataStore.data.first()[REFRESH_TOKEN]
    }

    // ID Token
    override suspend fun saveIdToken(token: String) {
        dataStore.edit { prefs -> prefs[ID_TOKEN] = token }
    }

    override suspend fun getIdToken(): String? {
        return dataStore.data.first()[ID_TOKEN]
    }

    // Tenant ID
    override suspend fun saveTenantId(tenantId: String) {
        dataStore.edit { prefs -> prefs[TENANT_ID] = tenantId }
    }

    override suspend fun getTenantId(): String? {
        return dataStore.data.first()[TENANT_ID]
    }

    // User ID
    override suspend fun saveUserId(userId: String) {
        dataStore.edit { prefs -> prefs[USER_ID] = userId }
    }

    override suspend fun getUserId(): String? {
        return dataStore.data.first()[USER_ID]
    }

    // Driver ID
    override suspend fun saveDriverId(driverId: String) {
        dataStore.edit { prefs -> prefs[DRIVER_ID] = driverId }
    }

    override suspend fun getDriverId(): String? {
        return dataStore.data.first()[DRIVER_ID]
    }

    // Truck ID
    override suspend fun saveTruckId(truckId: String) {
        dataStore.edit { prefs -> prefs[TRUCK_ID] = truckId }
    }

    override suspend fun getTruckId(): String? {
        return dataStore.data.first()[TRUCK_ID]
    }

    // Token Expiry
    override suspend fun saveTokenExpiry(expiry: Long) {
        dataStore.edit { prefs -> prefs[TOKEN_EXPIRY] = expiry.toString() }
    }

    override suspend fun getTokenExpiry(): Long? {
        return dataStore.data.first()[TOKEN_EXPIRY]?.toLongOrNull()
    }

    // Clear all data
    override suspend fun clearAll() {
        dataStore.edit { prefs -> prefs.clear() }
    }

    // Check if user is logged in
    override suspend fun isLoggedIn(): Boolean {
        return getAccessToken() != null
    }
}
