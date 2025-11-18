package com.jfleets.driver.service

import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.PendingIntent
import android.content.Intent
import android.os.Build
import androidx.core.app.NotificationCompat
import com.google.firebase.messaging.FirebaseMessagingService
import com.google.firebase.messaging.RemoteMessage
import com.jfleets.driver.MainActivity
import com.jfleets.driver.R
import com.jfleets.driver.data.repository.UserRepository
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.launch
import org.koin.android.ext.android.inject
import timber.log.Timber

class DriverFirebaseMessagingService : FirebaseMessagingService() {

    private val userRepository: UserRepository by inject()

    private val serviceScope = CoroutineScope(Dispatchers.IO + SupervisorJob())

    companion object {
        private const val CHANNEL_ID = "logistics_driver_channel"
        private const val NOTIFICATION_ID = 2001
    }

    override fun onNewToken(token: String) {
        super.onNewToken(token)
        Timber.d("New FCM token: $token")

        // Send token to server
        serviceScope.launch {
            try {
                userRepository.sendDeviceToken(token)
                Timber.d("Device token sent to server")
            } catch (e: Exception) {
                Timber.e(e, "Failed to send device token")
            }
        }
    }

    override fun onMessageReceived(message: RemoteMessage) {
        super.onMessageReceived(message)

        Timber.d("FCM message received from: ${message.from}")

        // Handle notification payload
        message.notification?.let { notification ->
            val title = notification.title ?: "Logistics Driver"
            val body = notification.body ?: "New notification"
            showNotification(title, body)
        }

        // Handle data payload
        message.data.isNotEmpty().let {
            Timber.d("Message data payload: ${message.data}")
            handleDataPayload(message.data)
        }
    }

    private fun handleDataPayload(data: Map<String, String>) {
        // Handle different notification types
        when (data["type"]) {
            "load_update" -> {
                Timber.d("Load update notification received")
                // Trigger load refresh in the app
                // You can use a broadcast receiver or shared flow to notify the app
            }

            "new_load" -> {
                Timber.d("New load notification received")
                val loadId = data["loadId"]
                showNotification("New Load Assigned", "You have been assigned a new load")
            }

            else -> {
                Timber.d("Unknown notification type")
            }
        }
    }

    private fun showNotification(title: String, body: String) {
        createNotificationChannel()

        val intent = Intent(this, MainActivity::class.java).apply {
            flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
        }

        val pendingIntent = PendingIntent.getActivity(
            this,
            0,
            intent,
            PendingIntent.FLAG_IMMUTABLE
        )

        val notification = NotificationCompat.Builder(this, CHANNEL_ID)
            .setSmallIcon(R.drawable.ic_launcher_foreground)
            .setContentTitle(title)
            .setContentText(body)
            .setPriority(NotificationCompat.PRIORITY_HIGH)
            .setAutoCancel(true)
            .setContentIntent(pendingIntent)
            .build()

        val notificationManager = getSystemService(NotificationManager::class.java)
        notificationManager.notify(NOTIFICATION_ID, notification)
    }

    private fun createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            val channel = NotificationChannel(
                CHANNEL_ID,
                "Logistics Notifications",
                NotificationManager.IMPORTANCE_HIGH
            ).apply {
                description = "Notifications for load updates and assignments"
            }

            val notificationManager = getSystemService(NotificationManager::class.java)
            notificationManager.createNotificationChannel(channel)
        }
    }
}
