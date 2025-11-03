package com.jfleets.driver.shared.data.api

import com.jfleets.driver.shared.data.dto.*
import io.ktor.client.*
import io.ktor.client.call.*
import io.ktor.client.plugins.*
import io.ktor.client.plugins.auth.*
import io.ktor.client.plugins.auth.providers.*
import io.ktor.client.plugins.contentnegotiation.*
import io.ktor.client.plugins.logging.*
import io.ktor.client.request.*
import io.ktor.http.*
import io.ktor.serialization.kotlinx.json.*
import kotlinx.serialization.json.Json

class ApiClient(
    private val baseUrl: String,
    private val getAccessToken: suspend () -> String?,
    private val getTenantId: suspend () -> String?
) {
    val client = HttpClient {
        install(ContentNegotiation) {
            json(Json {
                prettyPrint = true
                isLenient = true
                ignoreUnknownKeys = true
            })
        }

        install(Logging) {
            logger = Logger.DEFAULT
            level = LogLevel.INFO
        }

        install(HttpTimeout) {
            requestTimeoutMillis = 30000
            connectTimeoutMillis = 30000
        }

        defaultRequest {
            url(baseUrl)
            contentType(ContentType.Application.Json)
        }
    }

    suspend fun <T> get(
        endpoint: String,
        block: HttpRequestBuilder.() -> Unit = {}
    ): T {
        val token = getAccessToken()
        val tenantId = getTenantId()

        return client.get(endpoint) {
            if (token != null) {
                header("Authorization", "Bearer $token")
            }
            if (tenantId != null) {
                header("X-Tenant", tenantId)
            }
            block()
        }.body()
    }

    suspend fun <T> post(
        endpoint: String,
        body: Any? = null,
        block: HttpRequestBuilder.() -> Unit = {}
    ): T {
        val token = getAccessToken()
        val tenantId = getTenantId()

        return client.post(endpoint) {
            if (token != null) {
                header("Authorization", "Bearer $token")
            }
            if (tenantId != null) {
                header("X-Tenant", tenantId)
            }
            if (body != null) {
                setBody(body)
            }
            block()
        }.body()
    }

    suspend fun <T> put(
        endpoint: String,
        body: Any? = null,
        block: HttpRequestBuilder.() -> Unit = {}
    ): T {
        val token = getAccessToken()
        val tenantId = getTenantId()

        return client.put(endpoint) {
            if (token != null) {
                header("Authorization", "Bearer $token")
            }
            if (tenantId != null) {
                header("X-Tenant", tenantId)
            }
            if (body != null) {
                setBody(body)
            }
            block()
        }.body()
    }
}

class LoadApi(private val client: ApiClient) {
    suspend fun getLoad(id: Double): LoadDto {
        return client.get("api/loads/$id")
    }

    suspend fun confirmLoadStatus(request: ConfirmLoadStatus) {
        client.post<Unit>("api/loads/confirm-status", request)
    }

    suspend fun updateLoadProximity(request: UpdateLoadProximityCommand) {
        client.post<Unit>("api/loads/update-proximity", request)
    }

    suspend fun getActiveLoads(): DriverActiveLoadsDto {
        return client.get("api/loads/active")
    }

    suspend fun getPastLoads(startDate: String, endDate: String): List<LoadDto> {
        return client.get("api/loads/past") {
            parameter("startDate", startDate)
            parameter("endDate", endDate)
        }
    }
}

class TruckApi(private val client: ApiClient) {
    suspend fun getTruckByDriver(driverId: String): TruckDto {
        return client.get("api/trucks/driver") {
            parameter("driverId", driverId)
        }
    }

    suspend fun getTruck(id: String): TruckDto {
        return client.get("api/trucks/$id")
    }
}

class UserApi(private val client: ApiClient) {
    suspend fun getCurrentUser(): UserDto {
        return client.get("api/users/me")
    }

    suspend fun updateUser(id: String, user: UpdateUser) {
        client.put<Unit>("api/users/$id", user)
    }
}

class DriverApi(private val client: ApiClient) {
    suspend fun getCurrentDriver(): DriverDto {
        return client.get("api/drivers/me")
    }

    suspend fun sendDeviceToken(token: DeviceTokenDto) {
        client.post<Unit>("api/drivers/device-token", token)
    }
}

class StatsApi(private val client: ApiClient) {
    suspend fun getDriverStats(): DriverStatsDto {
        return client.get("api/stats/driver")
    }

    suspend fun getDailyGrosses(query: GetDailyGrossesQuery): List<DailyGrossDto> {
        return client.post("api/stats/daily-grosses", query)
    }

    suspend fun getMonthlyGrosses(query: GetMonthlyGrossesQuery): List<MonthlyGrossDto> {
        return client.post("api/stats/monthly-grosses", query)
    }
}
