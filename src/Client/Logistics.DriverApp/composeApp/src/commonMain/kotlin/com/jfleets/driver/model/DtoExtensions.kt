package com.jfleets.driver.model

import com.jfleets.driver.api.models.Address
import com.jfleets.driver.api.models.DailyGrossDto
import com.jfleets.driver.api.models.EmployeeDto
import com.jfleets.driver.api.models.LoadDto
import com.jfleets.driver.api.models.MonthlyGrossDto
import com.jfleets.driver.api.models.TruckDto
import kotlin.time.Instant

// Address extensions
fun Address.toDisplayString(): String {
    val parts = listOfNotNull(line1, line2, city, state, zipCode).filter { it.isNotBlank() }
    return parts.joinToString(", ")
}

fun LoadDto.getGoogleMapsUrl(): String {
    val origin = "${originLocation.latitude},${originLocation.longitude}"
    val destination = "${destinationLocation.latitude},${destinationLocation.longitude}"
    return "https://www.google.com/maps/dir/?api=1&origin=$origin&destination=$destination&travelmode=driving"
}

// TruckDto extensions
val TruckDto.driversList: List<EmployeeDto>
    get() = listOfNotNull(mainDriver, secondaryDriver)


// EmployeeDto extensions
fun EmployeeDto.fullName(): String {
    return listOfNotNull(firstName, lastName).joinToString(" ")
}

// Chart data extensions for DailyGrossDto and MonthlyGrossDto
fun DailyGrossDto.toChartData(): ChartData = ChartData(
    label = this.date?.toString()?.substring(0, 10) ?: "",
    gross = this.gross ?: 0.0,
    driverShare = this.driverShare ?: 0.0,
    date = date
)

fun MonthlyGrossDto.toChartData(): ChartData = ChartData(
    label = this.date?.toString()?.take(7) ?: "",
    gross = this.gross ?: 0.0,
    driverShare = this.driverShare ?: 0.0,
    date = date
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
