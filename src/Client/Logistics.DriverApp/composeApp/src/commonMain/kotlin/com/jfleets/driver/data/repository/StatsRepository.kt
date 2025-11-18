package com.jfleets.driver.data.repository

import com.jfleets.driver.data.api.StatsApi
import com.jfleets.driver.data.dto.GetDailyGrossesQuery
import com.jfleets.driver.data.dto.GetMonthlyGrossesQuery
import com.jfleets.driver.data.mapper.toChartData
import com.jfleets.driver.data.mapper.toDomain
import com.jfleets.driver.model.ChartData
import com.jfleets.driver.model.DriverStats
import kotlinx.datetime.TimeZone
import kotlinx.datetime.toLocalDateTime
import kotlin.time.Instant

class StatsRepository(
    private val statsApi: StatsApi
) {
    suspend fun getDriverStats(userId: String): Result<DriverStats> {
        return try {
            val dto = statsApi.getDriverStats(userId)
            Result.success(dto.toDomain())
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun getDailyGrosses(startDate: Instant, endDate: Instant): Result<List<ChartData>> {
        return try {
            val query = GetDailyGrossesQuery(
                startDate = startDate.toLocalDateTime(TimeZone.UTC).date.toString(),
                endDate = endDate.toLocalDateTime(TimeZone.UTC).date.toString()
            )
            val dtos = statsApi.getDailyGrosses(query)
            val chartData = dtos.map { it.toChartData() }
            Result.success(chartData)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun getMonthlyGrosses(
        startMonth: Int,
        startYear: Int,
        endMonth: Int,
        endYear: Int
    ): Result<List<ChartData>> {
        return try {
            val query = GetMonthlyGrossesQuery(
                startMonth = startMonth,
                startYear = startYear,
                endMonth = endMonth,
                endYear = endYear
            )
            val dtos = statsApi.getMonthlyGrosses(query)
            val chartData = dtos.map { it.toChartData() }
            Result.success(chartData)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}
