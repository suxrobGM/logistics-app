package com.jfleets.driver.data.security

import android.net.Uri
import net.openid.appauth.connectivity.ConnectionBuilder
import java.net.HttpURLConnection
import java.net.URL
import javax.net.ssl.HttpsURLConnection

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
