package com.logisticsx.driver

import android.content.Intent
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.ui.platform.LocalContext
import androidx.core.net.toUri
import com.logisticsx.driver.permission.RequestStartupPermissions
import com.logisticsx.driver.config.AppConfig
import com.logisticsx.driver.service.DutyStatusManager
import com.logisticsx.driver.ui.DriverApp
import org.koin.android.ext.android.inject

class MainActivity : ComponentActivity() {
    private val dutyStatusManager: DutyStatusManager by inject()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        AppConfig.initialize(
            apiBaseUrl = BuildConfig.API_BASE_URL,
            identityServerUrl = BuildConfig.IDENTITY_SERVER_URL,
            isProduction = !BuildConfig.DEBUG
        )
        initKoin(this)
        enableEdgeToEdge()

        // If the driver was previously On Duty, resume tracking so closing
        // the app from recents doesn't silently end their shift. Location
        // permission is implicitly granted because they couldn't have gone
        // On Duty without it.
        dutyStatusManager.resumeIfPersisted()

        setContent {
            // Request non-location startup permissions (camera, notifications).
            // Location permission is requested from the disclosure screen.
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
}
