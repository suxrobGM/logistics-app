package com.jfleets.driver

import android.content.Intent
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.ui.platform.LocalContext
import androidx.core.net.toUri
import com.jfleets.driver.permission.RequestStartupPermissions
import com.jfleets.driver.ui.DriverApp

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

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
}
