package com.jfleets.driver.shared.data.repository

import com.jfleets.driver.shared.data.api.LoadApi
import com.jfleets.driver.shared.data.dto.ConfirmLoadStatus
import com.jfleets.driver.shared.data.dto.UpdateLoadProximityCommand
import com.jfleets.driver.shared.data.mapper.toDomain
import com.jfleets.driver.shared.domain.model.Load
import com.jfleets.driver.shared.domain.model.LoadStatus
import kotlinx.datetime.TimeZone
import kotlinx.datetime.toLocalDateTime
import kotlin.time.Clock
import kotlin.time.Duration.Companion.days

class LoadRepository(
    private val loadApi: LoadApi
) {
    suspend fun getLoad(id: Double): Result<Load> {
        return try {
            val dto = loadApi.getLoad(id)
            Result.success(dto.toDomain())
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun getActiveLoads(): Result<List<Load>> {
        return try {
            val response = loadApi.getActiveLoads()
            val loads = response.loads?.map { it.toDomain() } ?: emptyList()
            Result.success(loads)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun getPastLoads(): Result<List<Load>> {
        return try {
            val now = Clock.System.now()
            val ninetyDaysAgo = now.minus(90.days)

            val endDate = now.toLocalDateTime(TimeZone.UTC).date.toString()
            val startDate = ninetyDaysAgo.toLocalDateTime(TimeZone.UTC).date.toString()

            val dtos = loadApi.getPastLoads(startDate, endDate)
            val loads = dtos.map { it.toDomain() }
            Result.success(loads)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun confirmPickup(loadId: Double): Result<Unit> {
        return try {
            val request = ConfirmLoadStatus(
                id = loadId,
                status = LoadStatus.PICKED_UP.toApiString()
            )
            loadApi.confirmLoadStatus(request)
            Result.success(Unit)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun confirmDelivery(loadId: Double): Result<Unit> {
        return try {
            val request = ConfirmLoadStatus(
                id = loadId,
                status = LoadStatus.DELIVERED.toApiString()
            )
            loadApi.confirmLoadStatus(request)
            Result.success(Unit)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun updateLoadProximity(
        loadId: Double,
        isNearOrigin: Boolean,
        isNearDestination: Boolean
    ): Result<Unit> {
        return try {
            val request = UpdateLoadProximityCommand(
                loadId = loadId,
                isNearOrigin = isNearOrigin,
                isNearDestination = isNearDestination
            )
            loadApi.updateLoadProximity(request)
            Result.success(Unit)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}
