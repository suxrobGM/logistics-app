package com.jfleets.driver.data.api

import com.jfleets.driver.data.dto.ApiResult
import com.jfleets.driver.data.dto.DeviceTokenDto
import com.jfleets.driver.data.dto.DriverDto
import com.jfleets.driver.data.dto.Result

class DriverApi(private val client: ApiClient) {
    suspend fun getDriver(userId: String): ApiResult<DriverDto> {
        return client.get("drivers/$userId")
    }

    suspend fun sendDeviceToken(token: DeviceTokenDto): Result {
        return client.post("drivers/device-token", token)
    }
}
