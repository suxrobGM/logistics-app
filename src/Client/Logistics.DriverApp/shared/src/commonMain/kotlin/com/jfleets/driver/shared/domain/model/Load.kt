package com.jfleets.driver.shared.domain.model

import kotlinx.datetime.Instant

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
    val createdDate: Instant?,
    val pickUpDate: Instant?,
    val deliveryDate: Instant?,
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

data class Truck(
    val id: String,
    val truckNumber: String,
    val drivers: List<Driver>,
    val activeLoads: List<Load>
)

data class Driver(
    val id: String,
    val firstName: String,
    val lastName: String,
    val userId: String?
) {
    val fullName: String
        get() = "$firstName $lastName"
}

data class User(
    val id: String,
    val email: String,
    val firstName: String,
    val lastName: String,
    val phoneNumber: String?
) {
    val fullName: String
        get() = "$firstName $lastName"
}

data class DriverStats(
    val weeklyGross: Double,
    val weeklyIncome: Double,
    val weeklyDistance: Double,
    val monthlyGross: Double,
    val monthlyIncome: Double,
    val monthlyDistance: Double
)

data class ChartData(
    val label: String,
    val gross: Double,
    val driverShare: Double,
    val date: Instant? = null
)

data class DateRange(
    val name: String,
    val startDate: Instant,
    val endDate: Instant,
    val useMonthly: Boolean = false
)
