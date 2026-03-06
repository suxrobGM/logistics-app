package com.logisticsx.driver.util

expect fun buildDirectionsUrl(origin: String, destination: String, waypoints: String? = null): String

expect fun buildLocationUrl(latitude: Double, longitude: Double): String
