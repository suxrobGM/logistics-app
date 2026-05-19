package com.logisticsx.driver.service

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
import com.logisticsx.driver.MainActivity
import com.logisticsx.driver.R
import com.logisticsx.driver.model.location.toAddressDto
import com.logisticsx.driver.model.location.toGeoPoint
import com.logisticsx.driver.permission.AppPermission
import com.logisticsx.driver.permission.isPermissionGranted
import com.logisticsx.driver.service.realtime.SignalRService
import com.logisticsx.driver.service.realtime.TruckGeolocation
import com.logisticsx.driver.util.Logger
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.cancel
import kotlinx.coroutines.launch
import kotlinx.coroutines.suspendCancellableCoroutine
import kotlinx.coroutines.withTimeoutOrNull
import org.koin.android.ext.android.inject
import java.util.Locale
import kotlin.coroutines.resume

/**
 * Foreground service that streams the truck's GPS position to the backend
 * (SignalR) while the driver is On Duty. Proximity-to-load detection is owned
 * by [LoadProximityWatcher] and reaches this service indirectly via
 * [LocationUpdateBus].
 *
 * Lifecycle is owned by [DutyStatusManager] / [LocationTracker]; the user's
 * duty toggle is the only thing that should start or stop this service.
 */
class LocationTrackingService : Service() {
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
    }

    override fun onCreate() {
        super.onCreate()
        fusedLocationClient = LocationServices.getFusedLocationProviderClient(this)
        createNotificationChannel()
        startForegroundWithTruckNumber()
        connectSignalR()
        startLocationUpdates()
    }

    private fun startForegroundWithTruckNumber() {
        // Synchronous placeholder first — Service must call startForeground()
        // within 5s of startForegroundService() or the system kills it
        // (ForegroundServiceDidNotStartInTimelyException, API 26+). Then enrich
        // with the truck number once DataStore resolves, via NotificationManager
        // so we don't race with the placeholder.
        startForeground(NOTIFICATION_ID, createNotification(null))
        serviceScope.launch {
            val truckNumber = preferencesManager.getTruckNumber()
            if (!truckNumber.isNullOrBlank()) {
                val nm = getSystemService(NotificationManager::class.java)
                nm.notify(NOTIFICATION_ID, createNotification(truckNumber))
            }
        }
    }

    private fun connectSignalR() {
        serviceScope.launch {
            try {
                signalRService.connect()
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
        val channel = NotificationChannel(
            CHANNEL_ID,
            "Location Tracking",
            NotificationManager.IMPORTANCE_LOW
        ).apply {
            description = "Active while you are On Duty"
        }

        val notificationManager = getSystemService(NotificationManager::class.java)
        notificationManager.createNotificationChannel(channel)
    }

    private fun createNotification(truckNumber: String?) =
        NotificationCompat.Builder(this, CHANNEL_ID)
            .setContentTitle(getString(R.string.location_tracking_notification_title))
            .setContentText(
                if (truckNumber.isNullOrBlank()) {
                    getString(R.string.location_tracking_notification_text)
                } else {
                    getString(R.string.location_tracking_notification_text_with_truck, truckNumber)
                }
            )
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
        LocationUpdateBus.tryEmit(LocationFix(location.latitude, location.longitude))

        serviceScope.launch {
            try {
                val address = getAddressFromLocation(location.latitude, location.longitude)
                sendLocationViaSignalR(location, address)
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

    private suspend fun getAddressFromLocation(latitude: Double, longitude: Double): Address? {
        return try {
            val geocoder = Geocoder(this, Locale.getDefault())

            val addresses: List<Address>? =
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
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

        // Disconnect on a detached scope with a hard timeout. onDestroy runs
        // on the main thread; a blocking WebSocket close handshake here can
        // ANR the app. Cancelling serviceScope first cancels any in-flight
        // SignalR send coroutines so this disconnect doesn't have to race them.
        serviceScope.cancel()
        @OptIn(kotlinx.coroutines.DelicateCoroutinesApi::class)
        GlobalScope.launch(Dispatchers.IO) {
            withTimeoutOrNull(3_000) {
                runCatching { signalRService.disconnect() }
            }
        }
        Logger.d("LocationTrackingService destroyed")
    }
}
