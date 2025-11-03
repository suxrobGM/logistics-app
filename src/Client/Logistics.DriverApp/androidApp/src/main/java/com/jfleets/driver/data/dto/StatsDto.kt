package com.jfleets.driver.data.dto

import com.google.gson.annotations.SerializedName
import java.util.Date

data class DriverStatsDto(
    @SerializedName("weeklyGross")
    val weeklyGross: Double?,

    @SerializedName("weeklyIncome")
    val weeklyIncome: Double?,

    @SerializedName("weeklyDistance")
    val weeklyDistance: Double?,

    @SerializedName("monthlyGross")
    val monthlyGross: Double?,

    @SerializedName("monthlyIncome")
    val monthlyIncome: Double?,

    @SerializedName("monthlyDistance")
    val monthlyDistance: Double?
)

data class DailyGrossDto(
    @SerializedName("date")
    val date: Date?,

    @SerializedName("gross")
    val gross: Double?,

    @SerializedName("driverShare")
    val driverShare: Double?
)

data class MonthlyGrossDto(
    @SerializedName("month")
    val month: String?,

    @SerializedName("year")
    val year: Int?,

    @SerializedName("gross")
    val gross: Double?,

    @SerializedName("driverShare")
    val driverShare: Double?
)

data class GetDailyGrossesQuery(
    @SerializedName("startDate")
    val startDate: String,

    @SerializedName("endDate")
    val endDate: String
)

data class GetMonthlyGrossesQuery(
    @SerializedName("startMonth")
    val startMonth: Int,

    @SerializedName("startYear")
    val startYear: Int,

    @SerializedName("endMonth")
    val endMonth: Int,

    @SerializedName("endYear")
    val endYear: Int
)
