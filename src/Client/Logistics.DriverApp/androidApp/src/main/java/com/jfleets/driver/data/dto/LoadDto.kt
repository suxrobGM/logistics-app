package com.jfleets.driver.data.dto

import com.google.gson.annotations.SerializedName
import java.util.Date

data class LoadDto(
    @SerializedName("id")
    val id: Double?,

    @SerializedName("refId")
    val refId: Long?,

    @SerializedName("name")
    val name: String?,

    @SerializedName("sourceAddress")
    val sourceAddress: String?,

    @SerializedName("destinationAddress")
    val destinationAddress: String?,

    @SerializedName("deliveryCost")
    val deliveryCost: Double?,

    @SerializedName("distance")
    val distance: Double?,

    @SerializedName("assignedDispatcherName")
    val assignedDispatcherName: String?,

    @SerializedName("assignedTruckId")
    val assignedTruckId: String?,

    @SerializedName("assignedDriverId")
    val assignedDriverId: String?,

    @SerializedName("status")
    val status: String?,

    @SerializedName("createdDate")
    val createdDate: Date?,

    @SerializedName("pickUpDate")
    val pickUpDate: Date?,

    @SerializedName("deliveryDate")
    val deliveryDate: Date?,

    @SerializedName("originLatitude")
    val originLatitude: Double?,

    @SerializedName("originLongitude")
    val originLongitude: Double?,

    @SerializedName("destinationLatitude")
    val destinationLatitude: Double?,

    @SerializedName("destinationLongitude")
    val destinationLongitude: Double?
)

data class DriverActiveLoadsDto(
    @SerializedName("loads")
    val loads: List<LoadDto>?
)

data class ConfirmLoadStatus(
    @SerializedName("id")
    val id: Double,

    @SerializedName("status")
    val status: String
)

data class UpdateLoadProximityCommand(
    @SerializedName("loadId")
    val loadId: Double,

    @SerializedName("isNearOrigin")
    val isNearOrigin: Boolean,

    @SerializedName("isNearDestination")
    val isNearDestination: Boolean
)
