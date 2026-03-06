package com.logisticsx.driver.util

actual fun buildDirectionsUrl(origin: String, destination: String, waypoints: String?): String {
    var url = "https://www.google.com/maps/dir/?api=1&origin=$origin&destination=$destination&travelmode=driving"
    if (waypoints != null) url += "&waypoints=$waypoints"
    return url
}

actual fun buildLocationUrl(latitude: Double, longitude: Double): String {
    return "https://www.google.com/maps/search/?api=1&query=$latitude,$longitude"
}
