package com.jfleets.driver.service

import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.longPreferencesKey
import androidx.datastore.preferences.core.stringPreferencesKey
import com.jfleets.driver.model.DistanceUnit
import com.jfleets.driver.model.Language
import com.jfleets.driver.model.UserSettings
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.map

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
        val TRUCK_NUMBER = stringPreferencesKey("truck_number")
        val TOKEN_EXPIRY = longPreferencesKey("token_expiry")
        val DRIVER_NAME = stringPreferencesKey("driver_name")
        val DISTANCE_UNIT = stringPreferencesKey("distance_unit")
        val LANGUAGE = stringPreferencesKey("language")
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

    // Truck Number
    suspend fun saveTruckNumber(truckNumber: String) {
        dataStore.edit { prefs -> prefs[TRUCK_NUMBER] = truckNumber }
    }

    suspend fun getTruckNumber(): String? {
        return dataStore.data.first()[TRUCK_NUMBER]
    }

    // Token Expiry
    suspend fun saveTokenExpiry(expiry: Long) {
        dataStore.edit { prefs -> prefs[TOKEN_EXPIRY] = expiry }
    }

    suspend fun getTokenExpiry(): Long? {
        return dataStore.data.first()[TOKEN_EXPIRY]
    }

    // Driver Name
    suspend fun saveDriverName(name: String) {
        dataStore.edit { prefs -> prefs[DRIVER_NAME] = name }
    }

    suspend fun getDriverName(): String? {
        return dataStore.data.first()[DRIVER_NAME]
    }

    // Distance Unit
    suspend fun saveDistanceUnit(unit: DistanceUnit) {
        dataStore.edit { prefs -> prefs[DISTANCE_UNIT] = unit.code }
    }

    suspend fun getDistanceUnit(): DistanceUnit {
        val code = dataStore.data.first()[DISTANCE_UNIT]
        return DistanceUnit.fromCode(code ?: DistanceUnit.MILES.code)
    }

    fun getDistanceUnitFlow(): Flow<DistanceUnit> {
        return dataStore.data.map { prefs ->
            DistanceUnit.fromCode(prefs[DISTANCE_UNIT] ?: DistanceUnit.MILES.code)
        }
    }

    // Language
    suspend fun saveLanguage(language: Language) {
        dataStore.edit { prefs -> prefs[LANGUAGE] = language.code }
    }

    suspend fun getLanguage(): Language {
        val code = dataStore.data.first()[LANGUAGE]
        return Language.fromCode(code ?: Language.ENGLISH.code)
    }

    fun getLanguageFlow(): Flow<Language> {
        return dataStore.data.map { prefs ->
            Language.fromCode(prefs[LANGUAGE] ?: Language.ENGLISH.code)
        }
    }

    // User Settings (combined)
    fun getUserSettingsFlow(): Flow<UserSettings> {
        return dataStore.data.map { prefs ->
            UserSettings(
                distanceUnit = DistanceUnit.fromCode(prefs[DISTANCE_UNIT] ?: DistanceUnit.MILES.code),
                language = Language.fromCode(prefs[LANGUAGE] ?: Language.ENGLISH.code)
            )
        }
    }

    // Clear all data
    suspend fun clearAll() {
        dataStore.edit { prefs -> prefs.clear() }
    }
}
