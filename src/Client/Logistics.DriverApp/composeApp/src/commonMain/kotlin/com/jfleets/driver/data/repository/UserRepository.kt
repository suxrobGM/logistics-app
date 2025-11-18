package com.jfleets.driver.data.repository

import com.jfleets.driver.data.api.DriverApi
import com.jfleets.driver.data.api.UserApi
import com.jfleets.driver.data.dto.DeviceTokenDto
import com.jfleets.driver.data.mapper.toDomain
import com.jfleets.driver.data.mapper.toUpdateDto
import com.jfleets.driver.model.User

class UserRepository(
    private val userApi: UserApi,
    private val driverApi: DriverApi
) {
    suspend fun getCurrentUser(): Result<User> {
        return try {
            val dto = userApi.getCurrentUser()
            Result.success(dto.toDomain())
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun updateUser(user: User): Result<Unit> {
        return try {
            userApi.updateUser(user.id, user.toUpdateDto())
            Result.success(Unit)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun getCurrentDriver(): Result<String> {
        return try {
            val dto = driverApi.getCurrentDriver()
            Result.success(dto.id ?: "")
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun sendDeviceToken(token: String): Result<Unit> {
        return try {
            driverApi.sendDeviceToken(DeviceTokenDto(token))
            Result.success(Unit)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}
