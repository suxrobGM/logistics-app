package com.jfleets.driver.data.model

import java.util.Date

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
    val date: Date? = null
)

data class DateRange(
    val name: String,
    val startDate: Date,
    val endDate: Date,
    val useMonthly: Boolean = false
)
