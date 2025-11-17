package com.jfleets.driver.shared.data.dto

import kotlinx.serialization.SerialName
import kotlinx.serialization.Serializable

@Serializable
data class LoadDto(
    @SerialName("id") val id: Double? = null,
    @SerialName("refId") val refId: Long? = null,
    @SerialName("name") val name: String? = null,
    @SerialName("sourceAddress") val sourceAddress: String? = null,
    @SerialName("destinationAddress") val destinationAddress: String? = null,
    @SerialName("deliveryCost") val deliveryCost: Double? = null,
    @SerialName("distance") val distance: Double? = null,
    @SerialName("assignedDispatcherName") val assignedDispatcherName: String? = null,
    @SerialName("status") val status: String? = null,
    @SerialName("createdDate") val createdDate: String? = null,
    @SerialName("pickUpDate") val pickUpDate: String? = null,
    @SerialName("deliveryDate") val deliveryDate: String? = null,
    @SerialName("originLatitude") val originLatitude: Double? = null,
    @SerialName("originLongitude") val originLongitude: Double? = null,
    @SerialName("destinationLatitude") val destinationLatitude: Double? = null,
    @SerialName("destinationLongitude") val destinationLongitude: Double? = null
)

@Serializable
data class TruckDto(
    @SerialName("id") val id: String? = null,
    @SerialName("truckNumber") val truckNumber: String? = null,
    @SerialName("drivers") val drivers: List<DriverDto>? = null,
    @SerialName("activeLoads") val activeLoads: List<LoadDto>? = null
)

@Serializable
data class DriverDto(
    @SerialName("id") val id: String? = null,
    @SerialName("firstName") val firstName: String? = null,
    @SerialName("lastName") val lastName: String? = null,
    @SerialName("userId") val userId: String? = null
)

@Serializable
data class UserDto(
    @SerialName("id") val id: String? = null,
    @SerialName("email") val email: String? = null,
    @SerialName("firstName") val firstName: String? = null,
    @SerialName("lastName") val lastName: String? = null,
    @SerialName("phoneNumber") val phoneNumber: String? = null
)

@Serializable
data class DriverStatsDto(
    @SerialName("weeklyGross") val weeklyGross: Double? = null,
    @SerialName("weeklyIncome") val weeklyIncome: Double? = null,
    @SerialName("weeklyDistance") val weeklyDistance: Double? = null,
    @SerialName("monthlyGross") val monthlyGross: Double? = null,
    @SerialName("monthlyIncome") val monthlyIncome: Double? = null,
    @SerialName("monthlyDistance") val monthlyDistance: Double? = null
)

@Serializable
data class DailyGrossDto(
    @SerialName("date") val date: String? = null,
    @SerialName("gross") val gross: Double? = null,
    @SerialName("driverShare") val driverShare: Double? = null
)

@Serializable
data class MonthlyGrossDto(
    @SerialName("month") val month: String? = null,
    @SerialName("year") val year: Int? = null,
    @SerialName("gross") val gross: Double? = null,
    @SerialName("driverShare") val driverShare: Double? = null
)

@Serializable
data class DriverActiveLoadsDto(
    @SerialName("loads") val loads: List<LoadDto>? = null
)

@Serializable
data class ConfirmLoadStatus(
    @SerialName("id") val id: Double,
    @SerialName("status") val status: String
)

@Serializable
data class UpdateLoadProximityCommand(
    @SerialName("loadId") val loadId: Double,
    @SerialName("isNearOrigin") val isNearOrigin: Boolean,
    @SerialName("isNearDestination") val isNearDestination: Boolean
)

@Serializable
data class DeviceTokenDto(
    @SerialName("token") val token: String
)

@Serializable
data class UpdateUser(
    @SerialName("id") val id: String,
    @SerialName("firstName") val firstName: String?,
    @SerialName("lastName") val lastName: String?,
    @SerialName("phoneNumber") val phoneNumber: String?
)

@Serializable
data class GetDailyGrossesQuery(
    @SerialName("startDate") val startDate: String,
    @SerialName("endDate") val endDate: String
)

@Serializable
data class GetMonthlyGrossesQuery(
    @SerialName("startMonth") val startMonth: Int,
    @SerialName("startYear") val startYear: Int,
    @SerialName("endMonth") val endMonth: Int,
    @SerialName("endYear") val endYear: Int
)

@Serializable
data class GetTruckQuery(
    @SerialName("driverId") val driverId: String? = null,
    @SerialName("truckId") val truckId: String? = null
)

@Serializable
data class EmployeeDto(
    @SerialName("id") val id: String? = null,
    @SerialName("firstName") val firstName: String? = null,
    @SerialName("lastName") val lastName: String? = null,
    @SerialName("email") val email: String? = null,
    @SerialName("role") val role: String? = null,
    @SerialName("userId") val userId: String? = null
)

// Auth DTOs
@Serializable
data class LoginRequest(
    @SerialName("code") val code: String,
    @SerialName("codeVerifier") val codeVerifier: String? = null
)

@Serializable
data class TokenResponse(
    @SerialName("access_token") val accessToken: String,
    @SerialName("refresh_token") val refreshToken: String? = null,
    @SerialName("id_token") val idToken: String? = null,
    @SerialName("token_type") val tokenType: String,
    @SerialName("expires_in") val expiresIn: Int
)

@Serializable
data class RefreshTokenRequest(
    @SerialName("refresh_token") val refreshToken: String
)
