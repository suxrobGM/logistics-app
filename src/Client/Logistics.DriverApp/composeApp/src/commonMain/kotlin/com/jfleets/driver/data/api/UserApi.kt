package com.jfleets.driver.data.api

import com.jfleets.driver.data.dto.ApiResult
import com.jfleets.driver.data.dto.Result
import com.jfleets.driver.data.dto.UpdateUser
import com.jfleets.driver.data.dto.UserDto

class UserApi(private val client: ApiClient) {
    suspend fun getUser(userId: String): ApiResult<UserDto> {
        return client.get("users/$userId")
    }

    suspend fun updateUser(id: String, user: UpdateUser): Result {
        return client.put("users/$id", user)
    }
}
