package com.jfleets.driver.data.api

import com.jfleets.driver.data.dto.ApiResult
import com.jfleets.driver.data.dto.TruckDto

class TruckApi(private val client: ApiClient) {
    suspend fun getTruckByDriver(driverId: String): ApiResult<TruckDto> {
        return client.get("trucks/$driverId")
    }

    suspend fun getTruck(id: String): ApiResult<TruckDto> {
        return client.get("trucks/$id")
    }
}
