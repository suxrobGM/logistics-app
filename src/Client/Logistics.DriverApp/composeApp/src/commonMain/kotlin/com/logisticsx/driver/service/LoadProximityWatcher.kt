package com.logisticsx.driver.service

import com.logisticsx.driver.api.DriverApi
import com.logisticsx.driver.api.LoadApi
import com.logisticsx.driver.api.models.LoadDto
import com.logisticsx.driver.api.models.UpdateLoadProximityCommand
import com.logisticsx.driver.util.Logger
import com.logisticsx.driver.util.distanceMeters
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.Job
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.async
import kotlinx.coroutines.awaitAll
import kotlinx.coroutines.cancel
import kotlinx.coroutines.coroutineScope
import kotlinx.coroutines.flow.launchIn
import kotlinx.coroutines.flow.onEach
import kotlin.time.Clock

/**
 * Watches the [LocationUpdateBus] for new fixes while the driver is on duty
 * and pings the backend with [UpdateLoadProximityCommand] on state transitions
 * (truck enters / leaves a 500m radius around an active load's origin or
 * destination). Repeated ticks inside the radius are not re-posted.
 *
 * Lifecycle is owned by [DutyStatusManager]; on-duty starts the watcher,
 * off-duty stops it. The active-loads list is cached for 5 minutes to avoid
 * an API call on every location update.
 */
class LoadProximityWatcher(
    private val loadApi: LoadApi,
    private val driverApi: DriverApi
) {
    private companion object {
        const val PROXIMITY_THRESHOLD_METERS = 500.0
        const val LOAD_CACHE_DURATION_MS = 5 * 60 * 1000L
    }

    private var scope: CoroutineScope? = null
    private var collectJob: Job? = null
    private var cachedLoads: List<LoadDto> = emptyList()
    private var lastLoadFetchTime: Long = 0
    private val loadsInProximity = mutableSetOf<String>()

    fun start() {
        if (scope != null) {
            return
        }
        val newScope = CoroutineScope(Dispatchers.Default + SupervisorJob())
        scope = newScope
        collectJob = LocationUpdateBus.updates
            .onEach { fix -> checkLoadProximity(fix) }
            .launchIn(newScope)
        Logger.d("LoadProximityWatcher: started")
    }

    fun stop() {
        scope?.cancel()
        collectJob = null
        scope = null
        cachedLoads = emptyList()
        lastLoadFetchTime = 0
        loadsInProximity.clear()
        Logger.d("LoadProximityWatcher: stopped")
    }

    private suspend fun checkLoadProximity(fix: LocationFix) {
        try {
            val loads = getActiveLoads()
            val transitions = loads.mapNotNull { load -> proximityTransition(load, fix) }
            if (transitions.isEmpty()) return

            coroutineScope {
                transitions.map { transition ->
                    async {
                        driverApi.updateLoadProximity(
                            UpdateLoadProximityCommand(
                                loadId = transition.loadId,
                                isInProximity = transition.isInProximity
                            )
                        )
                        Logger.d("Load ${transition.loadId} proximity -> ${transition.isInProximity}")
                    }
                }.awaitAll()
            }
        } catch (e: Exception) {
            Logger.e("LoadProximityWatcher: error checking proximity", e)
        }
    }

    private fun proximityTransition(load: LoadDto, fix: LocationFix): ProximityTransition? {
        val loadId = load.id ?: return null
        val originLat = load.originLocation.latitude
        val originLon = load.originLocation.longitude
        val destLat = load.destinationLocation.latitude
        val destLon = load.destinationLocation.longitude
        if (originLat == null || originLon == null || destLat == null || destLon == null) {
            return null
        }
        val nearOrigin = distanceMeters(fix.latitude, fix.longitude, originLat, originLon) <= PROXIMITY_THRESHOLD_METERS
        val nearDestination = distanceMeters(fix.latitude, fix.longitude, destLat, destLon) <= PROXIMITY_THRESHOLD_METERS
        val nowNear = nearOrigin || nearDestination
        val wasNear = loadId in loadsInProximity

        return when {
            nowNear && !wasNear -> {
                loadsInProximity.add(loadId)
                ProximityTransition(loadId, isInProximity = true)
            }
            !nowNear && wasNear -> {
                loadsInProximity.remove(loadId)
                ProximityTransition(loadId, isInProximity = false)
            }
            else -> null
        }
    }

    private suspend fun getActiveLoads(): List<LoadDto> {
        val now = Clock.System.now().toEpochMilliseconds()
        if (now - lastLoadFetchTime > LOAD_CACHE_DURATION_MS || cachedLoads.isEmpty()) {
            val result = loadApi.getLoads(onlyActiveLoads = true).body()
            cachedLoads = result.items ?: emptyList()
            lastLoadFetchTime = now
        }
        return cachedLoads
    }

    private data class ProximityTransition(val loadId: String, val isInProximity: Boolean)
}
