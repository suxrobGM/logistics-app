package com.logisticsx.driver.util

actual fun buildDirectionsUrl(origin: String, destination: String, waypoints: String?): String {
    var url = "https://maps.apple.com/?saddr=$origin&daddr=$destination&dirflg=d"
    if (waypoints != null) url += "&via=$waypoints"
    return url
}

actual fun buildLocationUrl(latitude: Double, longitude: Double): String {
    return "https://maps.apple.com/?q=$latitude,$longitude"
}
