package com.logisticsx.driver.service

import com.logisticsx.driver.util.Logger
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

/**
 * Owns the driver's on-duty/off-duty state and gates location tracking on it.
 *
 * Tracking is started only via [goOnDuty] (a user action). Going off-duty or
 * logging out calls [goOffDuty] which stops the foreground service / iOS
 * location updates. State is persisted via [PreferencesManager] so the
 * driver's last choice survives app restarts.
 */
class DutyStatusManager(
    private val preferencesManager: PreferencesManager,
    private val loadProximityWatcher: LoadProximityWatcher
) {
    private val scope = CoroutineScope(Dispatchers.Default + SupervisorJob())
    private val _isOnDuty = MutableStateFlow(false)
    val isOnDuty: StateFlow<Boolean> = _isOnDuty.asStateFlow()

    /**
     * Re-syncs in-memory state from persisted preferences. Call once at app
     * start (after Koin is initialized) so we know whether to resume tracking.
     */
    suspend fun loadPersisted() {
        _isOnDuty.value = preferencesManager.getIsOnDuty()
    }

    /**
     * Starts location tracking. Caller must ensure location permission is
     * granted. Persists the on-duty flag so tracking can resume on relaunch.
     */
    suspend fun goOnDuty() {
        if (_isOnDuty.value) {
            Logger.d("DutyStatusManager: already on duty")
            return
        }
        preferencesManager.saveIsOnDuty(true)
        _isOnDuty.value = true
        LocationTracker.start()
        loadProximityWatcher.start()
        Logger.d("DutyStatusManager: on duty")
    }

    /**
     * Stops location tracking and clears the persisted flag.
     */
    suspend fun goOffDuty() {
        preferencesManager.saveIsOnDuty(false)
        if (!_isOnDuty.value) {
            return
        }
        _isOnDuty.value = false
        loadProximityWatcher.stop()
        LocationTracker.stop()
        Logger.d("DutyStatusManager: off duty")
    }

    /**
     * If the persisted state says on-duty, resume tracking. Used at app boot.
     * Fires and forgets — failure to resume is logged but non-fatal.
     */
    fun resumeIfPersisted() {
        scope.launch {
            loadPersisted()
            if (_isOnDuty.value && !LocationTracker.isRunning()) {
                LocationTracker.start()
                loadProximityWatcher.start()
                Logger.d("DutyStatusManager: resumed tracking from persisted state")
            }
        }
    }
}
