package com.jfleets.driver.data.api

import com.jfleets.driver.data.dto.ConfirmLoadStatus
import com.jfleets.driver.data.dto.DailyGrossDto
import com.jfleets.driver.data.dto.DeviceTokenDto
import com.jfleets.driver.data.dto.DriverActiveLoadsDto
import com.jfleets.driver.data.dto.DriverDto
import com.jfleets.driver.data.dto.DriverStatsDto
import com.jfleets.driver.data.dto.GetDailyGrossesQuery
import com.jfleets.driver.data.dto.GetMonthlyGrossesQuery
import com.jfleets.driver.data.dto.LoadDto
import com.jfleets.driver.data.dto.MonthlyGrossDto
import com.jfleets.driver.data.dto.TruckDto
import com.jfleets.driver.data.dto.UpdateLoadProximityCommand
import com.jfleets.driver.data.dto.UpdateUser
import com.jfleets.driver.data.dto.UserDto
import io.ktor.client.HttpClient
import io.ktor.client.call.body
import io.ktor.client.plugins.HttpTimeout
import io.ktor.client.plugins.contentnegotiation.ContentNegotiation
import io.ktor.client.plugins.defaultRequest
import io.ktor.client.plugins.logging.DEFAULT
import io.ktor.client.plugins.logging.LogLevel
import io.ktor.client.plugins.logging.Logger
import io.ktor.client.plugins.logging.Logging
import io.ktor.client.request.HttpRequestBuilder
import io.ktor.client.request.get
import io.ktor.client.request.header
import io.ktor.client.request.parameter
import io.ktor.client.request.post
import io.ktor.client.request.put
import io.ktor.client.request.setBody
import io.ktor.http.ContentType
import io.ktor.http.contentType
import io.ktor.serialization.kotlinx.json.json
import kotlinx.serialization.json.Json

class ApiClient(
    private val baseUrl: String,
    @PublishedApi
    internal val getAccessToken: suspend () -> String?,
    @PublishedApi
    internal val getTenantId: suspend () -> String?
) {
    private val allowSelfSigned = baseUrl.contains("10.0.2.2")

    val client = createPlatformHttpClient(allowSelfSigned) {
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

    suspend inline fun <reified T> get(
        endpoint: String,
        noinline block: HttpRequestBuilder.() -> Unit = {}
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

    suspend inline fun <reified T> post(
        endpoint: String,
        body: Any? = null,
        noinline block: HttpRequestBuilder.() -> Unit = {}
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

    suspend inline fun <reified T> put(
        endpoint: String,
        body: Any? = null,
        noinline block: HttpRequestBuilder.() -> Unit = {}
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
