package com.jfleets.driver

import com.jfleets.driver.platform.PlatformSettings
import org.koin.dsl.module

actual fun platformModule() = module {
    single { PlatformSettings(get()) }
}
