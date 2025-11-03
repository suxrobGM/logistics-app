package com.jfleets.driver.data.model

import java.util.Date

data class Load(
    val id: Double,
    val refId: Long?,
    val name: String,
    val sourceAddress: String,
    val destinationAddress: String,
    val deliveryCost: Double,
    val distance: Double,
    val assignedDispatcherName: String?,
    val status: LoadStatus,
    val createdDate: Date?,
    val pickUpDate: Date?,
    val deliveryDate: Date?,
    val originLatitude: Double?,
    val originLongitude: Double?,
    val destinationLatitude: Double?,
    val destinationLongitude: Double?,
    val canConfirmPickup: Boolean = false,
    val canConfirmDelivery: Boolean = false
)

enum class LoadStatus {
    DISPATCHED,
    PICKED_UP,
    DELIVERED,
    CANCELLED;

    companion object {
        fun fromString(status: String?): LoadStatus {
            return when (status?.uppercase()) {
                "DISPATCHED" -> DISPATCHED
                "PICKEDUP", "PICKED_UP" -> PICKED_UP
                "DELIVERED" -> DELIVERED
                "CANCELLED" -> CANCELLED
                else -> DISPATCHED
            }
        }
    }

    fun toApiString(): String {
        return when (this) {
            DISPATCHED -> "Dispatched"
            PICKED_UP -> "PickedUp"
            DELIVERED -> "Delivered"
            CANCELLED -> "Cancelled"
        }
    }
}
