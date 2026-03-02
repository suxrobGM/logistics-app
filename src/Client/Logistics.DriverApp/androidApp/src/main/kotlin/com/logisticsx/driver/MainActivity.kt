package com.logisticsx.driver

import android.content.Intent
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.ui.platform.LocalContext
import androidx.core.net.toUri
import androidx.lifecycle.lifecycleScope
import com.logisticsx.driver.permission.AppPermission
import com.logisticsx.driver.permission.RequestStartupPermissions
import com.logisticsx.driver.permission.isPermissionGranted
import com.logisticsx.driver.config.AppConfig
import com.logisticsx.driver.service.LocationTracker
import com.logisticsx.driver.service.auth.AuthService
import com.logisticsx.driver.ui.DriverApp
import kotlinx.coroutines.launch
import org.koin.android.ext.android.inject

class MainActivity : ComponentActivity() {
    private val authService: AuthService by inject()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        AppConfig.initialize(
            apiBaseUrl = BuildConfig.API_BASE_URL,
            identityServerUrl = BuildConfig.IDENTITY_SERVER_URL,
            isProduction = !BuildConfig.DEBUG
        )
        initKoin(this)
        enableEdgeToEdge()

        setContent {
            // Request all startup permissions
            RequestStartupPermissions()

            val context = LocalContext.current
            DriverApp(
                onOpenUrl = { url ->
                    val intent = Intent(Intent.ACTION_VIEW, url.toUri())
                    context.startActivity(intent)
                }
            )
        }
    }

    override fun onResume() {
        super.onResume()
        // Start location tracking when activity is resumed (foreground state guaranteed)
        startLocationTrackingIfNeeded()
    }

    private fun startLocationTrackingIfNeeded() {
        // Only start if location permission is granted
        if (!isPermissionGranted(AppPermission.FineLocation)) {
            return
        }

        // Check if user is logged in before starting
        lifecycleScope.launch {
            try {
                if (authService.isLoggedIn()) {
                    LocationTracker.start()
                }
            } catch (_: Exception) {
                // Ignore - user not logged in
            }
        }
    }
}
