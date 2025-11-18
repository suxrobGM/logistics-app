package com.jfleets.driver.data.api

import io.ktor.client.HttpClient
import io.ktor.client.HttpClientConfig

expect fun createPlatformHttpClient(
    allowSelfSigned: Boolean,
    block: HttpClientConfig<*>.() -> Unit
): HttpClient
