package com.jfleets.driver.shared.data.mapper

import com.jfleets.driver.shared.data.dto.*
import com.jfleets.driver.shared.domain.model.*
import kotlinx.datetime.Instant

fun LoadDto.toDomain(): Load {
    return Load(
        id = this.id ?: 0.0,
        refId = this.refId,
        name = this.name ?: "",
        sourceAddress = this.sourceAddress ?: "",
        destinationAddress = this.destinationAddress ?: "",
        deliveryCost = this.deliveryCost ?: 0.0,
        distance = this.distance ?: 0.0,
        assignedDispatcherName = this.assignedDispatcherName,
        status = LoadStatus.fromString(this.status),
        createdDate = this.createdDate?.let { parseInstant(it) },
        pickUpDate = this.pickUpDate?.let { parseInstant(it) },
        deliveryDate = this.deliveryDate?.let { parseInstant(it) },
        originLatitude = this.originLatitude,
        originLongitude = this.originLongitude,
        destinationLatitude = this.destinationLatitude,
        destinationLongitude = this.destinationLongitude
    )
}

fun TruckDto.toDomain(): Truck {
    return Truck(
        id = this.id ?: "",
        truckNumber = this.truckNumber ?: "",
        drivers = this.drivers?.map { it.toDomain() } ?: emptyList(),
        activeLoads = this.activeLoads?.map { it.toDomain() } ?: emptyList()
    )
}

fun DriverDto.toDomain(): Driver {
    return Driver(
        id = this.id ?: "",
        firstName = this.firstName ?: "",
        lastName = this.lastName ?: "",
        userId = this.userId
    )
}

fun UserDto.toDomain(): User {
    return User(
        id = this.id ?: "",
        email = this.email ?: "",
        firstName = this.firstName ?: "",
        lastName = this.lastName ?: "",
        phoneNumber = this.phoneNumber
    )
}

fun DriverStatsDto.toDomain(): DriverStats {
    return DriverStats(
        weeklyGross = this.weeklyGross ?: 0.0,
        weeklyIncome = this.weeklyIncome ?: 0.0,
        weeklyDistance = this.weeklyDistance ?: 0.0,
        monthlyGross = this.monthlyGross ?: 0.0,
        monthlyIncome = this.monthlyIncome ?: 0.0,
        monthlyDistance = this.monthlyDistance ?: 0.0
    )
}

fun DailyGrossDto.toChartData(): ChartData {
    val instant = this.date?.let { parseInstant(it) }
    return ChartData(
        label = instant?.toString()?.substring(0, 10) ?: "",
        gross = this.gross ?: 0.0,
        driverShare = this.driverShare ?: 0.0,
        date = instant
    )
}

fun MonthlyGrossDto.toChartData(): ChartData {
    return ChartData(
        label = "${this.month ?: ""} ${this.year ?: ""}",
        gross = this.gross ?: 0.0,
        driverShare = this.driverShare ?: 0.0
    )
}

fun User.toUpdateDto(): UpdateUser {
    return UpdateUser(
        id = this.id,
        firstName = this.firstName,
        lastName = this.lastName,
        phoneNumber = this.phoneNumber
    )
}

private fun parseInstant(dateString: String): Instant? {
    return try {
        Instant.parse(dateString)
    } catch (e: Exception) {
        null
    }
}
