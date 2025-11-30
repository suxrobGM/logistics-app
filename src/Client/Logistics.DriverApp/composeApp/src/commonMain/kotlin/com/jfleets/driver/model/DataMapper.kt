package com.jfleets.driver.model

import com.jfleets.driver.api.models.Address
import com.jfleets.driver.api.models.DailyGrossDto
import com.jfleets.driver.api.models.DriverStatsDto
import com.jfleets.driver.api.models.EmployeeDto
import com.jfleets.driver.api.models.LoadDto
import com.jfleets.driver.api.models.MonthlyGrossDto
import com.jfleets.driver.api.models.TruckDto
import com.jfleets.driver.api.models.UpdateUserCommand
import com.jfleets.driver.api.models.UserDto

fun LoadDto.toDomain(): Load {
    return Load(
        id = this.id ?: "",
        refId = this.number,
        name = this.name ?: "",
        sourceAddress = this.originAddress.toDisplayString(),
        destinationAddress = this.destinationAddress.toDisplayString(),
        deliveryCost = this.deliveryCost ?: 0.0,
        distance = this.distance ?: 0.0,
        assignedDispatcherName = this.assignedDispatcherName,
        status = LoadStatus.fromApiStatus(this.status),
        createdDate = this.createdAt,
        pickUpDate = this.pickedUpAt,
        deliveryDate = this.deliveredAt,
        originLatitude = this.originLocation.latitude,
        originLongitude = this.originLocation.longitude,
        destinationLatitude = this.destinationLocation.latitude,
        destinationLongitude = this.destinationLocation.longitude,
        canConfirmPickup = this.canConfirmPickUp ?: false,
        canConfirmDelivery = this.canConfirmDelivery ?: false
    )
}

fun Address.toDisplayString(): String {
    val parts = listOfNotNull(line1, line2, city, state, zipCode).filter { it.isNotBlank() }
    return parts.joinToString(", ")
}

fun TruckDto.toDomain(): Truck {
    val drivers = listOfNotNull(mainDriver, secondaryDriver).map { it.toDomain() }
    return Truck(
        id = this.id ?: "",
        truckNumber = this.number ?: "",
        drivers = drivers,
        activeLoads = this.loads?.map { it.toDomain() } ?: emptyList()
    )
}

fun EmployeeDto.toDomain(): Driver {
    return Driver(
        id = this.id ?: "",
        firstName = this.firstName ?: "",
        lastName = this.lastName ?: "",
        userId = null
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
        weeklyGross = this.thisWeekGross ?: 0.0,
        weeklyIncome = this.thisWeekShare ?: 0.0,
        weeklyDistance = this.thisWeekDistance ?: 0.0,
        monthlyGross = this.thisMonthGross ?: 0.0,
        monthlyIncome = this.thisMonthShare ?: 0.0,
        monthlyDistance = this.thisMonthDistance ?: 0.0
    )
}

fun DailyGrossDto.toChartData(): ChartData {
    return ChartData(
        label = this.date?.toString()?.substring(0, 10) ?: "",
        gross = this.gross ?: 0.0,
        driverShare = this.driverShare ?: 0.0,
        date = this.date
    )
}

fun MonthlyGrossDto.toChartData(): ChartData {
    val label = this.date?.toString()?.take(7) ?: ""
    return ChartData(
        label = label,
        gross = this.gross ?: 0.0,
        driverShare = this.driverShare ?: 0.0,
        date = this.date
    )
}

fun User.toUpdateCommand(): UpdateUserCommand {
    return UpdateUserCommand(
        id = this.id,
        firstName = this.firstName,
        lastName = this.lastName,
        phoneNumber = this.phoneNumber
    )
}
