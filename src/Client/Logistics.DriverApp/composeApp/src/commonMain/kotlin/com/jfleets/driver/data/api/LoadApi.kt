package com.jfleets.driver.data.api

import com.jfleets.driver.data.dto.ApiResult
import com.jfleets.driver.data.dto.ConfirmLoadStatus
import com.jfleets.driver.data.dto.DriverActiveLoadsDto
import com.jfleets.driver.data.dto.LoadDto
import com.jfleets.driver.data.dto.Result
import com.jfleets.driver.data.dto.UpdateLoadProximityCommand
import io.ktor.client.request.parameter

class LoadApi(private val client: ApiClient) {
    suspend fun getLoad(id: Double): ApiResult<LoadDto> {
        return client.get("loads/$id")
    }

    suspend fun confirmLoadStatus(request: ConfirmLoadStatus): Result {
        return client.post("loads/confirm-status", request)
    }

    suspend fun updateLoadProximity(request: UpdateLoadProximityCommand): Result {
        return client.post("loads/update-proximity", request)
    }

    suspend fun getActiveLoads(): ApiResult<DriverActiveLoadsDto> {
        return client.get("loads/active")
    }

    suspend fun getPastLoads(startDate: String, endDate: String): ApiResult<List<LoadDto>> {
        return client.get("loads/past") {
            parameter("startDate", startDate)
            parameter("endDate", endDate)
        }
    }
}
