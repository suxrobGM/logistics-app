package com.jfleets.driver.data.dto

import com.google.gson.annotations.SerializedName

data class TruckDto(
    @SerializedName("id")
    val id: String?,

    @SerializedName("truckNumber")
    val truckNumber: String?,

    @SerializedName("drivers")
    val drivers: List<DriverDto>?,

    @SerializedName("activeLoads")
    val activeLoads: List<LoadDto>?
)

data class DriverDto(
    @SerializedName("id")
    val id: String?,

    @SerializedName("firstName")
    val firstName: String?,

    @SerializedName("lastName")
    val lastName: String?,

    @SerializedName("userId")
    val userId: String?
)

data class GetTruckQuery(
    @SerializedName("driverId")
    val driverId: String? = null,

    @SerializedName("truckId")
    val truckId: String? = null
)
