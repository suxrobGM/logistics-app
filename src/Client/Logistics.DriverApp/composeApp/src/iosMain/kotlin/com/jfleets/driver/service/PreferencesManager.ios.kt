package com.jfleets.driver.service

actual class PreferencesManager {
    actual suspend fun saveAccessToken(token: String) {
    }

    actual suspend fun getAccessToken(): String? {
        TODO("Not yet implemented")
    }

    actual suspend fun saveRefreshToken(token: String) {
    }

    actual suspend fun getRefreshToken(): String? {
        TODO("Not yet implemented")
    }

    actual suspend fun saveIdToken(token: String) {
    }

    actual suspend fun getIdToken(): String? {
        TODO("Not yet implemented")
    }

    actual suspend fun saveTenantId(tenantId: String) {
    }

    actual suspend fun getTenantId(): String? {
        TODO("Not yet implemented")
    }

    actual suspend fun saveUserId(userId: String) {
    }

    actual suspend fun getUserId(): String? {
        TODO("Not yet implemented")
    }

    actual suspend fun saveDriverId(driverId: String) {
    }

    actual suspend fun getDriverId(): String? {
        TODO("Not yet implemented")
    }

    actual suspend fun saveTruckId(truckId: String) {
    }

    actual suspend fun getTruckId(): String? {
        TODO("Not yet implemented")
    }

    actual suspend fun saveTokenExpiry(expiry: Long) {
    }

    actual suspend fun getTokenExpiry(): Long? {
        TODO("Not yet implemented")
    }

    actual suspend fun clearAll() {
    }
}