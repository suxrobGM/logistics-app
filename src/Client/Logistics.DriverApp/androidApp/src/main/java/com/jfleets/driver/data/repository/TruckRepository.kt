package com.jfleets.driver.data.repository

import com.jfleets.driver.data.api.TruckApiService
import com.jfleets.driver.data.local.PreferencesManager
import com.jfleets.driver.data.mapper.toDomain
import com.jfleets.driver.data.model.Truck
import com.jfleets.driver.util.Result
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class TruckRepository @Inject constructor(
    private val truckApiService: TruckApiService,
    private val preferencesManager: PreferencesManager
) {
    suspend fun getTruckByDriver(driverId: String): Result<Truck> = withContext(Dispatchers.IO) {
        try {
            val response = truckApiService.getTruckByDriver(driverId)
            if (response.isSuccessful && response.body() != null) {
                val truck = response.body()!!.toDomain()
                // Save truck ID for later use
                preferencesManager.saveTruckId(truck.id)
                Result.Success(truck)
            } else {
                Result.Error("Failed to load truck: ${response.message()}")
            }
        } catch (e: Exception) {
            Result.Error(e.message ?: "Unknown error")
        }
    }

    suspend fun getTruck(truckId: String): Result<Truck> = withContext(Dispatchers.IO) {
        try {
            val response = truckApiService.getTruck(truckId)
            if (response.isSuccessful && response.body() != null) {
                Result.Success(response.body()!!.toDomain())
            } else {
                Result.Error("Failed to load truck: ${response.message()}")
            }
        } catch (e: Exception) {
            Result.Error(e.message ?: "Unknown error")
        }
    }
}
