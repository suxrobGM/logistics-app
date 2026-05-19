package com.logisticsx.driver.service

import kotlinx.coroutines.channels.BufferOverflow
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.asSharedFlow

data class LocationFix(val latitude: Double, val longitude: Double)

object LocationUpdateBus {
    // No replay — a watcher started after a duty cycle should wait for a real
    // GPS fix, never see a stale coordinate from the previous shift.
    private val _updates = MutableSharedFlow<LocationFix>(
        replay = 0,
        extraBufferCapacity = 16,
        onBufferOverflow = BufferOverflow.DROP_OLDEST
    )

    val updates: SharedFlow<LocationFix> = _updates.asSharedFlow()

    suspend fun emit(fix: LocationFix) {
        _updates.emit(fix)
    }

    fun tryEmit(fix: LocationFix): Boolean = _updates.tryEmit(fix)
}
