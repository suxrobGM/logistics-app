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
import io.ktor.client.statement.HttpResponse
import io.ktor.client.statement.bodyAsText
import io.ktor.http.ContentType
import io.ktor.http.contentType
import io.ktor.http.isSuccess
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

        val response = client.get(endpoint) {
            if (token != null) {
                header("Authorization", "Bearer $token")
            }
            if (tenantId != null) {
                header("X-Tenant", tenantId)
            }
            block()
        }
        return response.requireSuccess(endpoint)
    }

    suspend inline fun <reified T> post(
        endpoint: String,
        body: Any? = null,
        noinline block: HttpRequestBuilder.() -> Unit = {}
    ): T {
        val token = getAccessToken()
        val tenantId = getTenantId()

        val response = client.post(endpoint) {
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
        }
        return response.requireSuccess(endpoint)
    }

    suspend inline fun <reified T> put(
        endpoint: String,
        body: Any? = null,
        noinline block: HttpRequestBuilder.() -> Unit = {}
    ): T {
        val token = getAccessToken()
        val tenantId = getTenantId()

        val response = client.put(endpoint) {
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
        }
        return response.requireSuccess(endpoint)
    }
}

class LoadApi(private val client: ApiClient) {
    suspend fun getLoad(id: Double): LoadDto {
        return client.get("loads/$id")
    }

    suspend fun confirmLoadStatus(request: ConfirmLoadStatus) {
        client.post<Unit>("loads/confirm-status", request)
    }

    suspend fun updateLoadProximity(request: UpdateLoadProximityCommand) {
        client.post<Unit>("loads/update-proximity", request)
    }

    suspend fun getActiveLoads(): DriverActiveLoadsDto {
        return client.get("loads/active")
    }

    suspend fun getPastLoads(startDate: String, endDate: String): List<LoadDto> {
        return client.get("loads/past") {
            parameter("startDate", startDate)
            parameter("endDate", endDate)
        }
    }
}

class TruckApi(private val client: ApiClient) {
    suspend fun getTruckByDriver(driverId: String): TruckDto {
        return client.get("trucks/$driverId")
    }

    suspend fun getTruck(id: String): TruckDto {
        return client.get("trucks/$id")
    }
}

class UserApi(private val client: ApiClient) {
    suspend fun getUser(userId: String): UserDto {
        return client.get("users/$userId")
    }

    suspend fun updateUser(id: String, user: UpdateUser) {
        client.put<Unit>("users/$id", user)
    }
}

class DriverApi(private val client: ApiClient) {
    suspend fun getDriver(userId: String): DriverDto {
        return client.get("drivers/$userId")
    }

    suspend fun sendDeviceToken(token: DeviceTokenDto) {
        client.post<Unit>("drivers/device-token", token)
    }
}

class StatsApi(private val client: ApiClient) {
    suspend fun getDriverStats(userId: String): DriverStatsDto {
        return client.get("stats/driver/$userId")
    }

    suspend fun getDailyGrosses(query: GetDailyGrossesQuery): List<DailyGrossDto> {
        return client.get("stats/daily-grosses") {
            parameter("startDate", query.startDate)
            parameter("endDate", query.endDate)
        }
    }

    suspend fun getMonthlyGrosses(query: GetMonthlyGrossesQuery): List<MonthlyGrossDto> {
        return client.get("stats/monthly-grosses") {
            parameter("startMonth", query.startMonth)
            parameter("startYear", query.startYear)
            parameter("endMonth", query.endMonth)
            parameter("endYear", query.endYear)
        }
    }
}

suspend inline fun <reified T> HttpResponse.requireSuccess(endpoint: String): T {
    if (!status.isSuccess()) {
        val errorText = runCatching { bodyAsText() }.getOrElse { "" }
        throw ApiException(status.value, endpoint, errorText)
    }
    return body()
}
