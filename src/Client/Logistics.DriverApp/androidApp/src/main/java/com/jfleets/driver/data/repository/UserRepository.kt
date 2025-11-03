package com.jfleets.driver.data.repository

import com.jfleets.driver.data.api.DriverApiService
import com.jfleets.driver.data.api.UserApiService
import com.jfleets.driver.data.dto.DeviceTokenDto
import com.jfleets.driver.data.local.PreferencesManager
import com.jfleets.driver.data.mapper.toDomain
import com.jfleets.driver.data.mapper.toUpdateDto
import com.jfleets.driver.data.model.User
import com.jfleets.driver.util.Result
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class UserRepository @Inject constructor(
    private val userApiService: UserApiService,
    private val driverApiService: DriverApiService,
    private val preferencesManager: PreferencesManager
) {
    suspend fun getCurrentUser(): Result<User> = withContext(Dispatchers.IO) {
        try {
            val response = userApiService.getCurrentUser()
            if (response.isSuccessful && response.body() != null) {
                Result.Success(response.body()!!.toDomain())
            } else {
                Result.Error("Failed to load user: ${response.message()}")
            }
        } catch (e: Exception) {
            Result.Error(e.message ?: "Unknown error")
        }
    }

    suspend fun updateUser(user: User): Result<Unit> = withContext(Dispatchers.IO) {
        try {
            val response = userApiService.updateUser(user.id, user.toUpdateDto())
            if (response.isSuccessful) {
                Result.Success(Unit)
            } else {
                Result.Error("Failed to update user: ${response.message()}")
            }
        } catch (e: Exception) {
            Result.Error(e.message ?: "Unknown error")
        }
    }

    suspend fun getCurrentDriver(): Result<String> = withContext(Dispatchers.IO) {
        try {
            val response = driverApiService.getCurrentDriver()
            if (response.isSuccessful && response.body() != null) {
                val driverId = response.body()!!.id ?: ""
                // Save driver ID for later use
                preferencesManager.saveDriverId(driverId)
                Result.Success(driverId)
            } else {
                Result.Error("Failed to load driver: ${response.message()}")
            }
        } catch (e: Exception) {
            Result.Error(e.message ?: "Unknown error")
        }
    }

    suspend fun sendDeviceToken(token: String): Result<Unit> = withContext(Dispatchers.IO) {
        try {
            val response = driverApiService.sendDeviceToken(DeviceTokenDto(token))
            if (response.isSuccessful) {
                Result.Success(Unit)
            } else {
                Result.Error("Failed to send device token: ${response.message()}")
            }
        } catch (e: Exception) {
            Result.Error(e.message ?: "Unknown error")
        }
    }
}
