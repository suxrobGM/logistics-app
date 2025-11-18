package com.jfleets.driver.data.local

import com.auth0.android.jwt.JWT
import com.jfleets.driver.model.UserInfo

class TokenManager(
    private val preferencesManager: PreferencesManager
) {
    suspend fun saveTokens(
        accessToken: String,
        refreshToken: String?,
        idToken: String?,
        expiresIn: Int
    ) {
        preferencesManager.saveAccessToken(accessToken)
        refreshToken?.let { preferencesManager.saveRefreshToken(it) }
        idToken?.let { preferencesManager.saveIdToken(it) }

        val expiryTime = System.currentTimeMillis() + (expiresIn * 1000L)
        preferencesManager.saveTokenExpiry(expiryTime)

        // Extract user info from token
        extractAndSaveUserInfo(accessToken)
    }

    suspend fun getAccessToken(): String? {
        return preferencesManager.getAccessToken()
    }

    suspend fun getRefreshToken(): String? {
        return preferencesManager.getRefreshToken()
    }

    suspend fun isTokenExpired(): Boolean {
        val expiry = preferencesManager.getTokenExpiry() ?: return true
        // Consider token expired 5 minutes before actual expiry
        return System.currentTimeMillis() > (expiry - 300000)
    }

    suspend fun clearTokens() {
        preferencesManager.clearAll()
    }

    private suspend fun extractAndSaveUserInfo(accessToken: String) {
        try {
            val jwt = JWT(accessToken)

            // Extract user ID
            jwt.getClaim("sub").asString()?.let { userId ->
                preferencesManager.saveUserId(userId)
            }

            // Extract tenant ID
            jwt.getClaim("tenant").asString()?.let { tenantId ->
                preferencesManager.saveTenantId(tenantId)
            }
        } catch (e: Exception) {
            // Handle JWT parsing error
            e.printStackTrace()
        }
    }

    suspend fun getUserInfo(): UserInfo? {
        val accessToken = getAccessToken() ?: return null
        val userId = preferencesManager.getUserId() ?: return null
        val tenantId = preferencesManager.getTenantId() ?: return null

        return try {
            val jwt = JWT(accessToken)

            val fullName = jwt.getClaim("name").asString() ?: ""
            val rolesArray = jwt.getClaim("role").asArray(String::class.java)
            val roles = rolesArray?.toList() ?: emptyList()
            val permissionsArray = jwt.getClaim("permission").asArray(String::class.java)
            val permissions = permissionsArray?.toList() ?: emptyList()

            UserInfo(
                userId = userId,
                fullName = fullName,
                tenantId = tenantId,
                roles = roles,
                permissions = permissions
            )
        } catch (e: Exception) {
            e.printStackTrace()
            null
        }
    }
}
