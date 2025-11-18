package com.jfleets.driver.data.api

import io.ktor.client.HttpClient
import io.ktor.client.HttpClientConfig
import io.ktor.client.engine.darwin.Darwin

@Suppress("UNUSED_PARAMETER")
actual fun createPlatformHttpClient(
    allowSelfSigned: Boolean,
    block: HttpClientConfig<*>.() -> Unit
): HttpClient {
    return HttpClient(Darwin) {
        block(this)
    }
}
