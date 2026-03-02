package com.logisticsx.driver.service.realtime

import com.logisticsx.driver.api.models.Address
import com.logisticsx.driver.api.models.GeoPoint
import kotlinx.serialization.Serializable

/**
 * Location data to send to the server.
 */
@Serializable
data class TruckGeolocation(
    val truckId: String,
    val tenantId: String,
    val currentLocation: GeoPoint,
    val currentAddress: Address? = null,
    val truckNumber: String? = null,
    val driversName: String? = null
)
