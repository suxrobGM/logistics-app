package com.jfleets.driver.data.repository

import com.jfleets.driver.data.api.StatsApiService
import com.jfleets.driver.data.dto.GetDailyGrossesQuery
import com.jfleets.driver.data.dto.GetMonthlyGrossesQuery
import com.jfleets.driver.data.mapper.toDomain
import com.jfleets.driver.data.mapper.toChartData
import com.jfleets.driver.data.model.ChartData
import com.jfleets.driver.data.model.DriverStats
import com.jfleets.driver.util.Result
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.text.SimpleDateFormat
import java.util.*
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class StatsRepository @Inject constructor(
    private val statsApiService: StatsApiService
) {
    suspend fun getDriverStats(): Result<DriverStats> = withContext(Dispatchers.IO) {
        try {
            val response = statsApiService.getDriverStats()
            if (response.isSuccessful && response.body() != null) {
                Result.Success(response.body()!!.toDomain())
            } else {
                Result.Error("Failed to load stats: ${response.message()}")
            }
        } catch (e: Exception) {
            Result.Error(e.message ?: "Unknown error")
        }
    }

    suspend fun getDailyGrosses(startDate: Date, endDate: Date): Result<List<ChartData>> =
        withContext(Dispatchers.IO) {
            try {
                val dateFormat = SimpleDateFormat("yyyy-MM-dd", Locale.US)
                val query = GetDailyGrossesQuery(
                    startDate = dateFormat.format(startDate),
                    endDate = dateFormat.format(endDate)
                )
                val response = statsApiService.getDailyGrosses(query)
                if (response.isSuccessful && response.body() != null) {
                    val chartData = response.body()!!.map { it.toChartData() }
                    Result.Success(chartData)
                } else {
                    Result.Error("Failed to load chart data: ${response.message()}")
                }
            } catch (e: Exception) {
                Result.Error(e.message ?: "Unknown error")
            }
        }

    suspend fun getMonthlyGrosses(
        startMonth: Int,
        startYear: Int,
        endMonth: Int,
        endYear: Int
    ): Result<List<ChartData>> = withContext(Dispatchers.IO) {
        try {
            val query = GetMonthlyGrossesQuery(
                startMonth = startMonth,
                startYear = startYear,
                endMonth = endMonth,
                endYear = endYear
            )
            val response = statsApiService.getMonthlyGrosses(query)
            if (response.isSuccessful && response.body() != null) {
                val chartData = response.body()!!.map { it.toChartData() }
                Result.Success(chartData)
            } else {
                Result.Error("Failed to load chart data: ${response.message()}")
            }
        } catch (e: Exception) {
            Result.Error(e.message ?: "Unknown error")
        }
    }
}
