package com.jfleets.driver

import org.koin.dsl.module

actual fun platformModule() = module {
    single { PlatformSettings(get()) }
}
