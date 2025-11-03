package com.jfleets.driver.shared.di

import com.jfleets.driver.shared.platform.PlatformSettings
import org.koin.dsl.module

actual fun platformModule() = module {
    single { PlatformSettings(get()) }
}
