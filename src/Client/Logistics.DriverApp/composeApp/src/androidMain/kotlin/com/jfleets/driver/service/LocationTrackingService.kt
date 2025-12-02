package com.jfleets.driver.service

import android.Manifest
import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.PendingIntent
import android.app.Service
import android.content.Intent
import android.location.Address
import android.location.Geocoder
import android.location.Location
import android.os.Build
import android.os.IBinder
import android.os.Looper
import androidx.annotation.RequiresPermission
import androidx.core.app.NotificationCompat
import com.google.android.gms.location.FusedLocationProviderClient
import com.google.android.gms.location.LocationCallback
import com.google.android.gms.location.LocationRequest
import com.google.android.gms.location.LocationResult
import com.google.android.gms.location.LocationServices
import com.google.android.gms.location.Priority
import com.jfleets.driver.MainActivity
import com.jfleets.driver.R
import com.jfleets.driver.api.DriverApi
import com.jfleets.driver.api.LoadApi
import com.jfleets.driver.api.models.UpdateLoadProximityCommand
import com.jfleets.driver.model.location.toAddressDto
import com.jfleets.driver.model.location.toGeoPoint
import com.jfleets.driver.permission.AppPermission
import com.jfleets.driver.permission.isPermissionGranted
import com.jfleets.driver.service.realtime.SignalRService
import com.jfleets.driver.service.realtime.TruckGeolocation
import com.jfleets.driver.util.Logger
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.cancel
import kotlinx.coroutines.launch
import kotlinx.coroutines.runBlocking
import kotlinx.coroutines.suspendCancellableCoroutine
import org.koin.android.ext.android.inject
import java.util.Locale
import kotlin.coroutines.resume

class LocationTrackingService : Service() {
    private val loadApi: LoadApi by inject()
    private val driverApi: DriverApi by inject()
    private val preferencesManager: PreferencesManager by inject()
    private val signalRService: SignalRService by inject()

    private lateinit var fusedLocationClient: FusedLocationProviderClient
    private lateinit var locationCallback: LocationCallback
    private val serviceScope = CoroutineScope(Dispatchers.IO + SupervisorJob())

    companion object {
        private const val NOTIFICATION_ID = 1001
        private const val CHANNEL_ID = "location_tracking_channel"
        private const val UPDATE_INTERVAL = 30000L // 30 seconds
        private const val FASTEST_INTERVAL = 15000L // 15 seconds
        private const val PROXIMITY_THRESHOLD = 500f // 500 meters (0.5 km)
    }

    override fun onCreate() {
        super.onCreate()
        fusedLocationClient = LocationServices.getFusedLocationProviderClient(this)
        createNotificationChannel()
        startForeground(NOTIFICATION_ID, createNotification())
        connectSignalR()
        startLocationUpdates()
    }

    private fun connectSignalR() {
        serviceScope.launch {
            try {
                signalRService.connect()
                Logger.d("SignalR connected in LocationTrackingService")
            } catch (e: Exception) {
                Logger.e("Failed to connect SignalR in LocationTrackingService", e)
            }
        }
    }

    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        return START_STICKY
    }

    override fun onBind(intent: Intent?): IBinder? = null

    private fun createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            val channel = NotificationChannel(
                CHANNEL_ID,
                "Location Tracking",
                NotificationManager.IMPORTANCE_LOW
            ).apply {
                description = "Tracking your location for load management"
            }

            val notificationManager = getSystemService(NotificationManager::class.java)
            notificationManager.createNotificationChannel(channel)
        }
    }

    private fun createNotification() =
        NotificationCompat.Builder(this, CHANNEL_ID)
            .setContentTitle(getString(R.string.location_tracking_notification_title))
            .setContentText(getString(R.string.location_tracking_notification_text))
            .setSmallIcon(R.drawable.ic_launcher_foreground)
            .setOngoing(true)
            .setContentIntent(
                PendingIntent.getActivity(
                    this,
                    0,
                    Intent(this, MainActivity::class.java),
                    PendingIntent.FLAG_IMMUTABLE
                )
            )
            .build()

    @RequiresPermission(allOf = [Manifest.permission.ACCESS_FINE_LOCATION, Manifest.permission.ACCESS_COARSE_LOCATION])
    private fun startLocationUpdates() {
        if (!this.isPermissionGranted(AppPermission.FineLocation)) {
            Logger.w("Location permission not granted")
            stopSelf()
            return
        }

        val locationRequest = LocationRequest.Builder(
            Priority.PRIORITY_HIGH_ACCURACY,
            UPDATE_INTERVAL
        )
            .setMinUpdateIntervalMillis(FASTEST_INTERVAL)
            .build()

        locationCallback = object : LocationCallback() {
            override fun onLocationResult(result: LocationResult) {
                result.lastLocation?.let { location ->
                    Logger.d("Location update: ${location.latitude}, ${location.longitude}")
                    handleLocationUpdate(location)
                }
            }
        }

        fusedLocationClient.requestLocationUpdates(
            locationRequest,
            locationCallback,
            Looper.getMainLooper()
        )
    }

    private fun handleLocationUpdate(location: Location) {
        serviceScope.launch {
            try {
                // Get address from coordinates
                val address = getAddressFromLocation(location.latitude, location.longitude)

                // Send location to server via SignalR
                sendLocationViaSignalR(location, address)

                // Check proximity to active loads
                checkLoadProximity(location)
            } catch (e: Exception) {
                Logger.e("Error handling location update", e)
            }
        }
    }

    private suspend fun sendLocationViaSignalR(
        location: Location,
        address: Address?
    ) {
        try {
            val locationUpdate = TruckGeolocation(
                currentLocation = location.toGeoPoint(),
                truckId = preferencesManager.getTruckId() ?: "",
                tenantId = preferencesManager.getTenantId() ?: "",
                truckNumber = preferencesManager.getTruckNumber(),
                driversName = preferencesManager.getDriverName(),
                currentAddress = address.toAddressDto(),
            )
            signalRService.sendLocationUpdate(locationUpdate)
        } catch (e: Exception) {
            Logger.e("Failed to send location via SignalR", e)
        }
    }

    private suspend fun checkLoadProximity(location: Location) {
        try {
            // Get active loads
            val response = loadApi.getLoads(onlyActiveLoads = true)
            val result = response.body()
            if (result.success != true || result.data == null) {
                Logger.e("Error getting active loads: ${result.error}")
                return
            }

            val loads = result.data

            loads.forEach { load ->
                val originLat = load.originLocation.latitude
                val originLon = load.originLocation.longitude
                val destLat = load.destinationLocation.latitude
                val destLon = load.destinationLocation.longitude
                val originLocation = Location("").apply {
                    latitude = originLat ?: 0.0
                    longitude = originLon ?: 0.0
                }
                val destLocation = Location("").apply {
                    latitude = destLat ?: 0.0
                    longitude = destLon ?: 0.0
                }

                if (originLat != null && originLon != null &&
                    destLat != null && destLon != null
                ) {
                    val distanceToOrigin = location.distanceTo(originLocation)
                    val distanceToDest = location.distanceTo(destLocation)

                    val nearOrigin = distanceToOrigin <= PROXIMITY_THRESHOLD
                    val nearDestination = distanceToDest <= PROXIMITY_THRESHOLD

                    // Update load proximity if near origin or destination
                    if (nearOrigin || nearDestination) {
                        val command = UpdateLoadProximityCommand(
                            loadId = load.id,
                            canConfirmPickUp = nearOrigin,
                            canConfirmDelivery = nearDestination
                        )
                        driverApi.updateLoadProximity(command)
                        Logger.d(
                            "Load ${load.id} proximity updated: origin=$nearOrigin, dest=$nearDestination"
                        )
                    }
                }
            }
        } catch (e: Exception) {
            Logger.e("Error checking load proximity", e)
        }
    }

    private suspend fun getAddressFromLocation(latitude: Double, longitude: Double): Address? {
        return try {
            val geocoder = Geocoder(this, Locale.getDefault())

            val addresses: List<Address>? =
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
                    // Android 13+ uses async API with GeocodeListener
                    suspendCancellableCoroutine<List<Address>> { continuation ->
                        geocoder.getFromLocation(
                            latitude,
                            longitude,
                            1
                        ) { result: MutableList<Address> ->
                            continuation.resume(result)
                        }
                    }
                } else {
                    @Suppress("DEPRECATION")
                    geocoder.getFromLocation(latitude, longitude, 1)
                }

            if (!addresses.isNullOrEmpty()) {
                addresses[0]
            } else {
                Logger.w("No address found for location: $latitude, $longitude")
                null
            }
        } catch (e: Exception) {
            Logger.e("Error getting address from location", e)
            null
        }
    }

    override fun onDestroy() {
        super.onDestroy()

        fusedLocationClient.removeLocationUpdates(locationCallback)

        runBlocking {
            signalRService.disconnect()
        }

        serviceScope.cancel()
        Logger.d("LocationTrackingService destroyed")
    }
}
