package com.jfleets.driver.data.repository

import com.jfleets.driver.data.api.LoadApiService
import com.jfleets.driver.data.dto.ConfirmLoadStatus
import com.jfleets.driver.data.dto.UpdateLoadProximityCommand
import com.jfleets.driver.data.mapper.toDomain
import com.jfleets.driver.data.model.Load
import com.jfleets.driver.data.model.LoadStatus
import com.jfleets.driver.util.Result
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.text.SimpleDateFormat
import java.util.*
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class LoadRepository @Inject constructor(
    private val loadApiService: LoadApiService
) {
    suspend fun getLoad(id: Double): Result<Load> = withContext(Dispatchers.IO) {
        try {
            val response = loadApiService.getLoad(id)
            if (response.isSuccessful && response.body() != null) {
                Result.Success(response.body()!!.toDomain())
            } else {
                Result.Error("Failed to load: ${response.message()}")
            }
        } catch (e: Exception) {
            Result.Error(e.message ?: "Unknown error")
        }
    }

    suspend fun getActiveLoads(): Result<List<Load>> = withContext(Dispatchers.IO) {
        try {
            val response = loadApiService.getActiveLoads()
            if (response.isSuccessful && response.body() != null) {
                val loads = response.body()!!.loads?.map { it.toDomain() } ?: emptyList()
                Result.Success(loads)
            } else {
                Result.Error("Failed to load: ${response.message()}")
            }
        } catch (e: Exception) {
            Result.Error(e.message ?: "Unknown error")
        }
    }

    suspend fun getPastLoads(): Result<List<Load>> = withContext(Dispatchers.IO) {
        try {
            val dateFormat = SimpleDateFormat("yyyy-MM-dd", Locale.US)
            val endDate = Date()
            val calendar = Calendar.getInstance()
            calendar.add(Calendar.DAY_OF_YEAR, -90)
            val startDate = calendar.time

            val response = loadApiService.getPastLoads(
                startDate = dateFormat.format(startDate),
                endDate = dateFormat.format(endDate)
            )

            if (response.isSuccessful && response.body() != null) {
                val loads = response.body()!!.map { it.toDomain() }
                Result.Success(loads)
            } else {
                Result.Error("Failed to load: ${response.message()}")
            }
        } catch (e: Exception) {
            Result.Error(e.message ?: "Unknown error")
        }
    }

    suspend fun confirmPickup(loadId: Double): Result<Unit> = withContext(Dispatchers.IO) {
        try {
            val request = ConfirmLoadStatus(
                id = loadId,
                status = LoadStatus.PICKED_UP.toApiString()
            )
            val response = loadApiService.confirmLoadStatus(request)
            if (response.isSuccessful) {
                Result.Success(Unit)
            } else {
                Result.Error("Failed to confirm pickup: ${response.message()}")
            }
        } catch (e: Exception) {
            Result.Error(e.message ?: "Unknown error")
        }
    }

    suspend fun confirmDelivery(loadId: Double): Result<Unit> = withContext(Dispatchers.IO) {
        try {
            val request = ConfirmLoadStatus(
                id = loadId,
                status = LoadStatus.DELIVERED.toApiString()
            )
            val response = loadApiService.confirmLoadStatus(request)
            if (response.isSuccessful) {
                Result.Success(Unit)
            } else {
                Result.Error("Failed to confirm delivery: ${response.message()}")
            }
        } catch (e: Exception) {
            Result.Error(e.message ?: "Unknown error")
        }
    }

    suspend fun updateLoadProximity(
        loadId: Double,
        isNearOrigin: Boolean,
        isNearDestination: Boolean
    ): Result<Unit> = withContext(Dispatchers.IO) {
        try {
            val request = UpdateLoadProximityCommand(
                loadId = loadId,
                isNearOrigin = isNearOrigin,
                isNearDestination = isNearDestination
            )
            val response = loadApiService.updateLoadProximity(request)
            if (response.isSuccessful) {
                Result.Success(Unit)
            } else {
                Result.Error("Failed to update proximity: ${response.message()}")
            }
        } catch (e: Exception) {
            Result.Error(e.message ?: "Unknown error")
        }
    }
}
