package com.jfleets.driver.security

import java.security.SecureRandom
import java.security.cert.X509Certificate
import javax.net.ssl.HostnameVerifier
import javax.net.ssl.HttpsURLConnection
import javax.net.ssl.SSLContext
import javax.net.ssl.SSLSocketFactory
import javax.net.ssl.TrustManager
import javax.net.ssl.X509TrustManager

/**
 * Dev-only helpers that bypass SSL validation for self-signed setups.
 */
internal object DebugTrustAllSsl {
    val trustManager: X509TrustManager = object : X509TrustManager {
        override fun checkClientTrusted(chain: Array<X509Certificate>, authType: String) = Unit
        override fun checkServerTrusted(chain: Array<X509Certificate>, authType: String) = Unit
        override fun getAcceptedIssuers(): Array<X509Certificate> = emptyArray()
    }

    val sslSocketFactory: SSLSocketFactory by lazy(LazyThreadSafetyMode.PUBLICATION) {
        SSLContext.getInstance("TLS").apply {
            init(null, arrayOf<TrustManager>(trustManager), SecureRandom())
        }.socketFactory
    }

    val hostnameVerifier: HostnameVerifier = HostnameVerifier { _, _ -> true }

    fun applyTo(connection: HttpsURLConnection) {
        connection.sslSocketFactory = sslSocketFactory
        connection.hostnameVerifier = hostnameVerifier
    }
}
