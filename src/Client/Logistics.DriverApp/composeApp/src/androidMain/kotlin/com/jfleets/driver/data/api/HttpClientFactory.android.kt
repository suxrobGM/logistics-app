package com.jfleets.driver.data.api

import com.jfleets.driver.BuildConfig
import com.jfleets.driver.data.security.DebugTrustAllSsl
import io.ktor.client.HttpClient
import io.ktor.client.HttpClientConfig
import io.ktor.client.engine.okhttp.OkHttp

actual fun createPlatformHttpClient(
    allowSelfSigned: Boolean,
    block: HttpClientConfig<*>.() -> Unit
): HttpClient {
    return HttpClient(OkHttp) {
        if (allowSelfSigned && BuildConfig.DEBUG) {
            engine {
                config {
                    val trustManager = DebugTrustAllSsl.trustManager
                    sslSocketFactory(DebugTrustAllSsl.sslSocketFactory, trustManager)
                    hostnameVerifier(DebugTrustAllSsl.hostnameVerifier)
                }
            }
        }

        block(this)
    }
}
