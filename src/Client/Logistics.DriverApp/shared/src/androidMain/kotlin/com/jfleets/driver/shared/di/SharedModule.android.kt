package com.jfleets.driver.shared.di

import android.content.Context
import com.jfleets.driver.shared.platform.PlatformSettings
import org.koin.dsl.module

actual fun platformModule() = module {
    single { (context: Context) -> PlatformSettings(context) }
}
