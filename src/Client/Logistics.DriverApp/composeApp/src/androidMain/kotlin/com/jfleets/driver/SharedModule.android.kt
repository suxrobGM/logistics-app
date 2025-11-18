package com.jfleets.driver

import com.jfleets.driver.data.local.PreferencesManager
import org.koin.core.context.GlobalContext

actual fun getTokenProvider(): TokenProvider {
    return object : TokenProvider {
        private val preferencesManager: PreferencesManager by lazy {
            GlobalContext.get().get()
        }

        override suspend fun getAccessToken(): String? {
            return preferencesManager.getAccessToken()
        }

        override suspend fun getTenantId(): String? {
            return preferencesManager.getTenantId()
        }

        override suspend fun getUserId(): String? {
            return preferencesManager.getUserId()
        }
    }
}
