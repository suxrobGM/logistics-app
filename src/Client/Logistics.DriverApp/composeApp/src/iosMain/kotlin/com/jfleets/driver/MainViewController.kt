package com.jfleets.driver

import androidx.compose.ui.window.ComposeUIViewController
import com.jfleets.driver.ui.DriverApp
import platform.Foundation.NSURL
import platform.UIKit.UIApplication
import platform.UIKit.UIViewController

fun MainViewController(): UIViewController {
    initKoin()
    return ComposeUIViewController {
        DriverApp(
            onOpenUrl = { url ->
                NSURL.URLWithString(url)?.let { nsUrl ->
                    UIApplication.sharedApplication.openURL(nsUrl)
                }
            }
        )
    }
}
