package com.jfleets.driver.data.auth

import com.jfleets.driver.data.local.PreferencesManager

class LoginService(
    private val oAuthService: OAuthService,
    private val preferencesManager: PreferencesManager
) {
    suspend fun login(username: String, password: String): Result<Unit> {
        return oAuthService.login(username, password).mapCatching { tokenResponse ->
            saveTokens(tokenResponse)
        }
    }

    suspend fun refreshToken(): Result<Unit> {
        val refreshToken = preferencesManager.getRefreshToken()
            ?: return Result.failure(
                OAuthException(
                    "no_refresh_token",
                    "No refresh token available"
                )
            )

        return oAuthService.refreshToken(refreshToken).mapCatching { tokenResponse ->
            saveTokens(tokenResponse)
        }
    }

    suspend fun logout() {
        preferencesManager.clearAll()
    }

    private suspend fun saveTokens(tokenResponse: TokenResponse) {
        preferencesManager.saveAccessToken(tokenResponse.accessToken)
        tokenResponse.refreshToken?.let { preferencesManager.saveRefreshToken(it) }
        tokenResponse.idToken?.let { preferencesManager.saveIdToken(it) }

        val expiryTime = currentTimeMillis() + (tokenResponse.expiresIn * 1000L)
        preferencesManager.saveTokenExpiry(expiryTime)

        // Extract user info from token
        extractAndSaveUserInfo(tokenResponse.accessToken)
    }

    private suspend fun extractAndSaveUserInfo(accessToken: String) {
        try {
            val payload = decodeJwtPayload(accessToken)

            // Extract user ID (sub claim)
            payload["sub"]?.let { userId ->
                preferencesManager.saveUserId(userId)
            }

            // Extract tenant ID
            payload["tenant"]?.let { tenantId ->
                preferencesManager.saveTenantId(tenantId)
            }
        } catch (e: Exception) {
            // Handle JWT parsing error
            e.printStackTrace()
        }
    }

    private fun decodeJwtPayload(jwt: String): Map<String, String> {
        val parts = jwt.split(".")
        if (parts.size != 3) return emptyMap()

        val payload = parts[1]
        val decoded = decodeBase64Url(payload)
        return parseJsonClaims(decoded)
    }

    @OptIn(kotlin.io.encoding.ExperimentalEncodingApi::class)
    private fun decodeBase64Url(input: String): String {
        // Add padding if necessary
        val padded = when (input.length % 4) {
            2 -> "$input=="
            3 -> "$input="
            else -> input
        }
        // Convert base64url to base64
        val base64 = padded.replace('-', '+').replace('_', '/')
        val decoded = kotlin.io.encoding.Base64.decode(base64)
        return decoded.decodeToString()
    }

    private fun parseJsonClaims(json: String): Map<String, String> {
        val result = mutableMapOf<String, String>()
        // Simple JSON parsing for string values
        val cleanJson = json.trim().removeSurrounding("{", "}")
        val pairs = cleanJson.split(",")

        for (pair in pairs) {
            val keyValue = pair.split(":", limit = 2)
            if (keyValue.size == 2) {
                val key = keyValue[0].trim().removeSurrounding("\"")
                val value = keyValue[1].trim().removeSurrounding("\"")
                // Only store string values (not arrays or objects)
                if (!value.startsWith("[") && !value.startsWith("{")) {
                    result[key] = value
                }
            }
        }
        return result
    }
}


