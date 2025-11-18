package com.jfleets.driver.service

import android.Manifest
import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.PendingIntent
import android.app.Service
import android.content.Intent
import android.content.pm.PackageManager
import android.location.Geocoder
import android.os.Build
import android.os.IBinder
import android.os.Looper
import androidx.core.app.ActivityCompat
import androidx.core.app.NotificationCompat
import com.google.android.gms.location.FusedLocationProviderClient
import com.google.android.gms.location.LocationCallback
import com.google.android.gms.location.LocationRequest
import com.google.android.gms.location.LocationResult
import com.google.android.gms.location.LocationServices
import com.google.android.gms.location.Priority
import com.jfleets.driver.MainActivity
import com.jfleets.driver.R
import com.jfleets.driver.data.api.LoadApi
import com.jfleets.driver.data.dto.UpdateLoadProximityCommand
import com.jfleets.driver.data.local.PreferencesManager
import com.jfleets.driver.data.mapper.toDomain
import com.jfleets.driver.util.calculateDistance
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.cancel
import kotlinx.coroutines.launch
import org.koin.android.ext.android.inject
import timber.log.Timber
import java.util.Locale

class LocationTrackingService : Service() {

    private val preferencesManager: PreferencesManager by inject()
    private val loadApi: LoadApi by inject()

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
        startLocationUpdates()
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

    private fun startLocationUpdates() {
        if (ActivityCompat.checkSelfPermission(
                this,
                Manifest.permission.ACCESS_FINE_LOCATION
            ) != PackageManager.PERMISSION_GRANTED
        ) {
            Timber.w("Location permission not granted")
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
                    Timber.d("Location update: ${location.latitude}, ${location.longitude}")
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

    private fun handleLocationUpdate(location: android.location.Location) {
        serviceScope.launch {
            try {
                // Get address from coordinates
                val address = getAddressFromLocation(location.latitude, location.longitude)

                // Send location to server via SignalR
                // TODO: Implement SignalR connection and send location
                Timber.d("Location: $address")

                // Check proximity to active loads
                checkLoadProximity(location)
            } catch (e: Exception) {
                Timber.e(e, "Error handling location update")
            }
        }
    }

    private suspend fun checkLoadProximity(location: android.location.Location) {
        try {
            // Get active loads
            val result = loadApi.getActiveLoads()
            if (!result.success || result.data == null) {
                Timber.e("Error getting active loads: ${result.error}")
                return
            }

            val loads = result.data.loads?.map { it.toDomain() } ?: emptyList()

            loads.forEach { load ->
                val originLat = load.originLatitude
                val originLon = load.originLongitude
                val destLat = load.destinationLatitude
                val destLon = load.destinationLongitude

                if (originLat != null && originLon != null &&
                    destLat != null && destLon != null
                ) {
                    val distanceToOrigin = calculateDistance(
                        location.latitude, location.longitude,
                        originLat, originLon
                    )
                    val distanceToDest = calculateDistance(
                        location.latitude, location.longitude,
                        destLat, destLon
                    )

                    val nearOrigin = distanceToOrigin <= PROXIMITY_THRESHOLD
                    val nearDestination = distanceToDest <= PROXIMITY_THRESHOLD

                    // Update load proximity if near origin or destination
                    if (nearOrigin || nearDestination) {
                        val command = UpdateLoadProximityCommand(
                            loadId = load.id,
                            isNearOrigin = nearOrigin,
                            isNearDestination = nearDestination
                        )
                        loadApi.updateLoadProximity(command)
                        Timber.d("Load ${load.id} proximity updated: origin=$nearOrigin, dest=$nearDestination")
                    }
                }
            }
        } catch (e: Exception) {
            Timber.e(e, "Error checking load proximity")
        }
    }

    private fun getAddressFromLocation(latitude: Double, longitude: Double): String {
        return try {
            val geocoder = Geocoder(this, Locale.getDefault())
            val addresses = if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
                // For Android 13+, use the new async API
                // For simplicity, we'll use the old synchronous API for now
                @Suppress("DEPRECATION")
                geocoder.getFromLocation(latitude, longitude, 1)
            } else {
                @Suppress("DEPRECATION")
                geocoder.getFromLocation(latitude, longitude, 1)
            }

            if (!addresses.isNullOrEmpty()) {
                val address = addresses[0]
                "${address.locality}, ${address.adminArea}"
            } else {
                "Unknown location"
            }
        } catch (e: Exception) {
            Timber.e(e, "Error getting address from location")
            "Unknown location"
        }
    }

    override fun onDestroy() {
        super.onDestroy()
        fusedLocationClient.removeLocationUpdates(locationCallback)
        serviceScope.cancel()
        Timber.d("LocationTrackingService destroyed")
    }
}
