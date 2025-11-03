package com.jfleets.driver.util

import android.location.Location
import java.text.NumberFormat
import java.text.SimpleDateFormat
import java.util.*

// Distance extensions
fun Double.toMiles(): Double = this * 0.000621371
fun Double.toKilometers(): Double = this / 1000.0

fun Double.formatDistance(): String {
    val miles = this.toMiles()
    return String.format("%.1f mi", miles)
}

// Currency extensions
fun Double.formatCurrency(): String {
    val format = NumberFormat.getCurrencyInstance(Locale.US)
    return format.format(this)
}

// Date extensions
fun Date.formatShort(): String {
    val format = SimpleDateFormat("MMM dd, yyyy", Locale.US)
    return format.format(this)
}

fun Date.formatDateTime(): String {
    val format = SimpleDateFormat("MMM dd, yyyy HH:mm", Locale.US)
    return format.format(this)
}

// Location distance calculation
fun Location.distanceTo(lat: Double, lon: Double): Float {
    val targetLocation = Location("").apply {
        latitude = lat
        longitude = lon
    }
    return this.distanceTo(targetLocation)
}

// Calculate distance in meters between two coordinates
fun calculateDistance(lat1: Double, lon1: Double, lat2: Double, lon2: Double): Float {
    val startLocation = Location("").apply {
        latitude = lat1
        longitude = lon1
    }
    val endLocation = Location("").apply {
        latitude = lat2
        longitude = lon2
    }
    return startLocation.distanceTo(endLocation)
}

// String extensions
fun String.toTitleCase(): String {
    return this.lowercase()
        .split(" ")
        .joinToString(" ") { it.replaceFirstChar { char -> char.uppercase() } }
}
