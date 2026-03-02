package com.logisticsx.driver.service

import kotlinx.coroutines.flow.StateFlow

/**
 * Cross-platform network connectivity monitor.
 * Exposes a [isConnected] state flow that reflects current network availability.
 * Platform implementations are provided via Koin.
 */
interface NetworkMonitor {
    val isConnected: StateFlow<Boolean>
}
