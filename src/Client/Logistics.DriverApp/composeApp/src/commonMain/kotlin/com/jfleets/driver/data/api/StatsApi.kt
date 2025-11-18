package com.jfleets.driver.data.api

import com.jfleets.driver.data.dto.ApiResult
import com.jfleets.driver.data.dto.DailyGrossesDto
import com.jfleets.driver.data.dto.DriverStatsDto
import com.jfleets.driver.data.dto.MonthlyGrossesDto
import io.ktor.client.request.parameter

class StatsApi(private val client: ApiClient) {
    suspend fun getDriverStats(userId: String): ApiResult<DriverStatsDto> {
        return client.get("stats/driver/$userId")
    }

    suspend fun getDailyGrosses(startDate: String, endDate: String): ApiResult<DailyGrossesDto> {
        return client.get("stats/daily-grosses") {
            parameter("startDate", startDate)
            parameter("endDate", endDate)
        }
    }

    suspend fun getMonthlyGrosses(
        startMonth: Int,
        startYear: Int,
        endMonth: Int,
        endYear: Int
    ): ApiResult<MonthlyGrossesDto> {
        return client.get("stats/monthly-grosses") {
            parameter("startMonth", startMonth)
            parameter("startYear", startYear)
            parameter("endMonth", endMonth)
            parameter("endYear", endYear)
        }
    }
}
