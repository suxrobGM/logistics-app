import Foundation
import CoreLocation

/// Bridge class to expose iOS CoreLocation functionality to Kotlin via Objective-C interop.
@objc public class LocationService: NSObject, CLLocationManagerDelegate {
    private let locationManager = CLLocationManager()
    private let geocoder = CLGeocoder()

    @objc public var onLocationUpdate: ((Double, Double, String?, String?, String?) -> Void)?
    @objc public var onError: ((Error) -> Void)?

    private var isTracking = false

    @objc public override init() {
        super.init()
        locationManager.delegate = self
        locationManager.desiredAccuracy = kCLLocationAccuracyBest
        locationManager.distanceFilter = 50 // Update every 50 meters
        locationManager.allowsBackgroundLocationUpdates = true
        locationManager.pausesLocationUpdatesAutomatically = false
    }

    @objc public func start() {
        guard !isTracking else { return }

        switch locationManager.authorizationStatus {
        case .notDetermined:
            locationManager.requestAlwaysAuthorization()
        case .authorizedAlways, .authorizedWhenInUse:
            startLocationUpdates()
        case .denied, .restricted:
            print("Location permission denied")
        @unknown default:
            break
        }

        isTracking = true
        print("iOS location tracking started")
    }

    @objc public func stop() {
        locationManager.stopUpdatingLocation()
        locationManager.stopMonitoringSignificantLocationChanges()
        isTracking = false
        print("iOS location tracking stopped")
    }

    @objc public func isRunning() -> Bool {
        return isTracking
    }

    private func startLocationUpdates() {
        locationManager.startUpdatingLocation()
        locationManager.startMonitoringSignificantLocationChanges()
    }

    // MARK: - CLLocationManagerDelegate

    public func locationManager(_ manager: CLLocationManager, didUpdateLocations locations: [CLLocation]) {
        guard let location = locations.last else { return }

        // Reverse geocode to get address
        geocoder.reverseGeocodeLocation(location) { [weak self] placemarks, error in
            guard let self = self else { return }

            let placemark = placemarks?.first
            let addressLine = placemark?.thoroughfare
            let city = placemark?.locality
            let state = placemark?.administrativeArea

            self.onLocationUpdate?(
                location.coordinate.latitude,
                location.coordinate.longitude,
                addressLine,
                city,
                state
            )
        }
    }

    public func locationManager(_ manager: CLLocationManager, didFailWithError error: Error) {
        print("Location error: \(error.localizedDescription)")
        onError?(error)
    }

    public func locationManagerDidChangeAuthorization(_ manager: CLLocationManager) {
        switch manager.authorizationStatus {
        case .authorizedAlways, .authorizedWhenInUse:
            if isTracking {
                startLocationUpdates()
            }
        case .denied, .restricted:
            print("Location authorization denied")
            stop()
        default:
            break
        }
    }
}
