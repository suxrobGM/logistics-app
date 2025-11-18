package com.jfleets.driver

actual fun getTokenProvider(): TokenProvider {
    // TODO: Implement iOS token provider when iOS support is added
    return object : TokenProvider {
        override suspend fun getAccessToken(): String? {
            return null
        }

        override suspend fun getTenantId(): String? {
            return null
        }

        override suspend fun getUserId(): String? {
            return null
        }
    }
}
