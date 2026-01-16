package com.jfleets.driver.service

import kotlinx.cinterop.ExperimentalForeignApi
import kotlinx.cinterop.useContents
import kotlinx.coroutines.suspendCancellableCoroutine
import platform.CoreLocation.CLLocation
import platform.CoreLocation.CLLocationManager
import platform.CoreLocation.CLLocationManagerDelegateProtocol
import platform.CoreLocation.kCLLocationAccuracyBest
import platform.Foundation.NSError
import platform.darwin.NSObject
import kotlin.coroutines.resume

/**
 * iOS actual implementation of LocationService using CoreLocation.
 */
actual class LocationService {

    @OptIn(ExperimentalForeignApi::class)
    actual suspend fun getCurrentLocation(): LocationData? {
        return suspendCancellableCoroutine { continuation ->
            val locationManager = CLLocationManager()
            locationManager.desiredAccuracy = kCLLocationAccuracyBest

            val delegate = object : NSObject(), CLLocationManagerDelegateProtocol {
                override fun locationManager(manager: CLLocationManager, didUpdateLocations: List<*>) {
                    val location = didUpdateLocations.lastOrNull() as? CLLocation
                    if (location != null) {
                        val coordinate = location.coordinate.useContents {
                            LocationData(
                                latitude = latitude,
                                longitude = longitude
                            )
                        }
                        locationManager.stopUpdatingLocation()
                        locationManager.delegate = null
                        continuation.resume(coordinate)
                    }
                }

                override fun locationManager(manager: CLLocationManager, didFailWithError: NSError) {
                    locationManager.stopUpdatingLocation()
                    locationManager.delegate = null
                    continuation.resume(null)
                }
            }

            locationManager.delegate = delegate
            locationManager.requestWhenInUseAuthorization()
            locationManager.startUpdatingLocation()

            continuation.invokeOnCancellation {
                locationManager.stopUpdatingLocation()
                locationManager.delegate = null
            }
        }
    }
}
