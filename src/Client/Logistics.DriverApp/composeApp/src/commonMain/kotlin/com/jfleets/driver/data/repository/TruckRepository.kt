package com.jfleets.driver.data.repository

import com.jfleets.driver.data.api.TruckApi
import com.jfleets.driver.data.mapper.toDomain
import com.jfleets.driver.model.Truck

class TruckRepository(
    private val truckApi: TruckApi
) {
    suspend fun getTruckByDriver(driverId: String): Result<Truck> {
        return try {
            val dto = truckApi.getTruckByDriver(driverId)
            Result.success(dto.toDomain())
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun getTruck(truckId: String): Result<Truck> {
        return try {
            val dto = truckApi.getTruck(truckId)
            Result.success(dto.toDomain())
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}
