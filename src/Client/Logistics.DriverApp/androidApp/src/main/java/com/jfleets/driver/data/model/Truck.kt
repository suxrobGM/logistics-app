package com.jfleets.driver.data.model

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
