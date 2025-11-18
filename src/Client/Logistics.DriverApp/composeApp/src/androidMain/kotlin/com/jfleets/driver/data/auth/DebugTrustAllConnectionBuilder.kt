package com.jfleets.driver.data.auth

import android.net.Uri
import net.openid.appauth.connectivity.ConnectionBuilder
import java.net.HttpURLConnection
import java.net.URL
import java.security.SecureRandom
import java.security.cert.X509Certificate
import javax.net.ssl.HostnameVerifier
import javax.net.ssl.HttpsURLConnection
import javax.net.ssl.SSLContext
import javax.net.ssl.TrustManager
import javax.net.ssl.X509TrustManager
import com.jfleets.driver.data.security.DebugTrustAllSsl

/**
 * Lightweight ConnectionBuilder that trusts every certificate/host.
 * Use only for local development with self-signed certs.
 */
object DebugTrustAllConnectionBuilder : ConnectionBuilder {
    override fun openConnection(uri: Uri): HttpURLConnection {
        val connection = URL(uri.toString()).openConnection() as HttpURLConnection
        if (connection is HttpsURLConnection) {
           DebugTrustAllSsl.applyTo(connection)
        }
        return connection
    }
}
