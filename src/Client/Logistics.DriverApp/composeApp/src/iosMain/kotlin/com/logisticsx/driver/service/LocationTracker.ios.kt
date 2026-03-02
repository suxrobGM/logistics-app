package com.logisticsx.driver.service

import com.logisticsx.driver.api.models.Address
import com.logisticsx.driver.api.models.GeoPoint
import com.logisticsx.driver.service.realtime.SignalRService
import com.logisticsx.driver.service.realtime.TruckGeolocation
import com.logisticsx.driver.util.Logger
import kotlinx.cinterop.ExperimentalForeignApi
import kotlinx.cinterop.useContents
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.cancel
import kotlinx.coroutines.launch
import org.koin.core.component.KoinComponent
import org.koin.core.component.inject
import platform.CoreLocation.CLGeocoder
import platform.CoreLocation.CLLocation
import platform.CoreLocation.CLLocationManager
import platform.CoreLocation.CLLocationManagerDelegateProtocol
import platform.CoreLocation.CLPlacemark
import platform.CoreLocation.kCLAuthorizationStatusAuthorizedAlways
import platform.CoreLocation.kCLAuthorizationStatusAuthorizedWhenInUse
import platform.CoreLocation.kCLAuthorizationStatusNotDetermined
import platform.CoreLocation.kCLLocationAccuracyBest
import platform.Foundation.NSError
import platform.darwin.NSObject

/**
 * iOS implementation of LocationTracker using platform.CoreLocation directly.
 * Mirrors the Android LocationTrackingService behavior: tracks location,
 * reverse geocodes, and sends updates via SignalR.
 */
actual object LocationTracker : KoinComponent {
    private val signalRService: SignalRService by inject()
    private val preferencesManager: PreferencesManager by inject()

    private var locationManager: CLLocationManager? = null
    private var delegate: LocationTrackerDelegate? = null
    private var scope: CoroutineScope? = null
    private var isTracking = false

    actual fun start() {
        if (isTracking) {
            Logger.d("iOS Location: Already tracking")
            return
        }

        val manager = CLLocationManager()
        manager.desiredAccuracy = kCLLocationAccuracyBest
        manager.distanceFilter = 50.0
        manager.allowsBackgroundLocationUpdates = true
        manager.pausesLocationUpdatesAutomatically = false

        val trackerScope = CoroutineScope(Dispatchers.Main + SupervisorJob())
        val trackerDelegate = LocationTrackerDelegate(
            signalRService = signalRService,
            preferencesManager = preferencesManager,
            scope = trackerScope
        )
        manager.delegate = trackerDelegate

        when (manager.authorizationStatus) {
            kCLAuthorizationStatusAuthorizedAlways,
            kCLAuthorizationStatusAuthorizedWhenInUse -> {
                manager.startUpdatingLocation()
                manager.startMonitoringSignificantLocationChanges()
                Logger.d("iOS Location: Tracking started")
            }
            kCLAuthorizationStatusNotDetermined -> {
                manager.requestAlwaysAuthorization()
                Logger.d("iOS Location: Requesting authorization")
            }
            else -> {
                Logger.w("iOS Location: Permission denied")
            }
        }

        locationManager = manager
        delegate = trackerDelegate
        scope = trackerScope
        isTracking = true
    }

    actual fun stop() {
        locationManager?.stopUpdatingLocation()
        locationManager?.stopMonitoringSignificantLocationChanges()
        locationManager?.delegate = null
        locationManager = null
        delegate = null
        scope?.cancel()
        scope = null
        isTracking = false
        Logger.d("iOS Location: Tracking stopped")
    }

    actual fun isRunning(): Boolean = isTracking
}

/**
 * CLLocationManagerDelegate that sends location updates via SignalR.
 */
@OptIn(ExperimentalForeignApi::class)
private class LocationTrackerDelegate(
    private val signalRService: SignalRService,
    private val preferencesManager: PreferencesManager,
    private val scope: CoroutineScope
) : NSObject(), CLLocationManagerDelegateProtocol {
    private val geocoder = CLGeocoder()

    override fun locationManager(manager: CLLocationManager, didUpdateLocations: List<*>) {
        val location = didUpdateLocations.lastOrNull() as? CLLocation ?: return

        val latitude: Double
        val longitude: Double
        location.coordinate.useContents {
            latitude = this.latitude
            longitude = this.longitude
        }

        // Reverse geocode and send via SignalR
        geocoder.reverseGeocodeLocation(location) { placemarks, error ->
            if (error != null) {
                Logger.w("iOS Location: Geocode error: ${error.localizedDescription}")
            }

            val placemark = (placemarks?.firstOrNull() as? CLPlacemark)
            val addressLine = placemark?.thoroughfare
            val city = placemark?.locality
            val state = placemark?.administrativeArea

            scope.launch {
                sendLocationUpdate(latitude, longitude, addressLine, city, state)
            }
        }
    }

    private suspend fun sendLocationUpdate(
        latitude: Double,
        longitude: Double,
        addressLine: String?,
        city: String?,
        state: String?
    ) {
        try {
            val geolocation = TruckGeolocation(
                truckId = preferencesManager.getTruckId() ?: "",
                tenantId = preferencesManager.getTenantId() ?: "",
                currentLocation = GeoPoint(latitude = latitude, longitude = longitude),
                currentAddress = Address(line1 = addressLine, city = city, state = state),
                truckNumber = preferencesManager.getTruckNumber(),
                driversName = preferencesManager.getDriverName()
            )
            signalRService.sendLocationUpdate(geolocation)
            Logger.d("iOS Location: Sent update $latitude, $longitude")
        } catch (e: Exception) {
            Logger.e("iOS Location: Failed to send update: ${e.message}")
        }
    }

    override fun locationManager(manager: CLLocationManager, didFailWithError: NSError) {
        Logger.e("iOS Location: Error: ${didFailWithError.localizedDescription}")
    }

    override fun locationManagerDidChangeAuthorization(manager: CLLocationManager) {
        when (manager.authorizationStatus) {
            kCLAuthorizationStatusAuthorizedAlways,
            kCLAuthorizationStatusAuthorizedWhenInUse -> {
                manager.startUpdatingLocation()
                manager.startMonitoringSignificantLocationChanges()
                Logger.d("iOS Location: Authorization granted, started updates")
            }
            else -> {
                Logger.w("iOS Location: Authorization changed to non-authorized")
            }
        }
    }
}
