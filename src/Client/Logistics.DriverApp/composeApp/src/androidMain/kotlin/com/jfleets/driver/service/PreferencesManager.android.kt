package com.jfleets.driver.service

import android.content.Context
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import kotlinx.coroutines.flow.first

private val Context.dataStore: DataStore<Preferences> by preferencesDataStore(name = "driver_prefs")

actual class PreferencesManager(context: Context) {
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
    actual suspend fun saveAccessToken(token: String) {
        dataStore.edit { prefs -> prefs[ACCESS_TOKEN] = token }
    }

    actual suspend fun getAccessToken(): String? {
        return dataStore.data.first()[ACCESS_TOKEN]
    }

    // Refresh Token
    actual suspend fun saveRefreshToken(token: String) {
        dataStore.edit { prefs -> prefs[REFRESH_TOKEN] = token }
    }

    actual suspend fun getRefreshToken(): String? {
        return dataStore.data.first()[REFRESH_TOKEN]
    }

    // ID Token
    actual suspend fun saveIdToken(token: String) {
        dataStore.edit { prefs -> prefs[ID_TOKEN] = token }
    }

    actual suspend fun getIdToken(): String? {
        return dataStore.data.first()[ID_TOKEN]
    }

    // Tenant ID
    actual suspend fun saveTenantId(tenantId: String) {
        dataStore.edit { prefs -> prefs[TENANT_ID] = tenantId }
    }

    actual suspend fun getTenantId(): String? {
        return dataStore.data.first()[TENANT_ID]
    }

    // User ID
    actual suspend fun saveUserId(userId: String) {
        dataStore.edit { prefs -> prefs[USER_ID] = userId }
    }

    actual suspend fun getUserId(): String? {
        return dataStore.data.first()[USER_ID]
    }

    // Driver ID
    actual suspend fun saveDriverId(driverId: String) {
        dataStore.edit { prefs -> prefs[DRIVER_ID] = driverId }
    }

    actual suspend fun getDriverId(): String? {
        return dataStore.data.first()[DRIVER_ID]
    }

    // Truck ID
    actual suspend fun saveTruckId(truckId: String) {
        dataStore.edit { prefs -> prefs[TRUCK_ID] = truckId }
    }

    actual suspend fun getTruckId(): String? {
        return dataStore.data.first()[TRUCK_ID]
    }

    // Token Expiry
    actual suspend fun saveTokenExpiry(expiry: Long) {
        dataStore.edit { prefs -> prefs[TOKEN_EXPIRY] = expiry.toString() }
    }

    actual suspend fun getTokenExpiry(): Long? {
        return dataStore.data.first()[TOKEN_EXPIRY]?.toLongOrNull()
    }

    // Clear all data
    actual suspend fun clearAll() {
        dataStore.edit { prefs -> prefs.clear() }
    }
}
