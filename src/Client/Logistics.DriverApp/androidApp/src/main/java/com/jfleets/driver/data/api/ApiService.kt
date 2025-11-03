package com.jfleets.driver.data.api

import com.jfleets.driver.data.dto.*
import retrofit2.Response
import retrofit2.http.*

interface LoadApiService {
    @GET("api/loads/{id}")
    suspend fun getLoad(@Path("id") id: Double): Response<LoadDto>

    @POST("api/loads/confirm-status")
    suspend fun confirmLoadStatus(@Body request: ConfirmLoadStatus): Response<Unit>

    @POST("api/loads/update-proximity")
    suspend fun updateLoadProximity(@Body request: UpdateLoadProximityCommand): Response<Unit>

    @GET("api/loads/active")
    suspend fun getActiveLoads(): Response<DriverActiveLoadsDto>

    @GET("api/loads/past")
    suspend fun getPastLoads(
        @Query("startDate") startDate: String,
        @Query("endDate") endDate: String
    ): Response<List<LoadDto>>
}

interface TruckApiService {
    @GET("api/trucks/driver")
    suspend fun getTruckByDriver(@Query("driverId") driverId: String): Response<TruckDto>

    @GET("api/trucks/{id}")
    suspend fun getTruck(@Path("id") id: String): Response<TruckDto>
}

interface UserApiService {
    @GET("api/users/me")
    suspend fun getCurrentUser(): Response<UserDto>

    @PUT("api/users/{id}")
    suspend fun updateUser(@Path("id") id: String, @Body user: UpdateUser): Response<Unit>
}

interface DriverApiService {
    @GET("api/drivers/me")
    suspend fun getCurrentDriver(): Response<EmployeeDto>

    @POST("api/drivers/device-token")
    suspend fun sendDeviceToken(@Body token: DeviceTokenDto): Response<Unit>
}

interface StatsApiService {
    @GET("api/stats/driver")
    suspend fun getDriverStats(): Response<DriverStatsDto>

    @POST("api/stats/daily-grosses")
    suspend fun getDailyGrosses(@Body query: GetDailyGrossesQuery): Response<List<DailyGrossDto>>

    @POST("api/stats/monthly-grosses")
    suspend fun getMonthlyGrosses(@Body query: GetMonthlyGrossesQuery): Response<List<MonthlyGrossDto>>
}

interface AuthApiService {
    @POST("connect/token")
    @FormUrlEncoded
    suspend fun getToken(
        @Field("grant_type") grantType: String,
        @Field("code") code: String,
        @Field("client_id") clientId: String,
        @Field("client_secret") clientSecret: String,
        @Field("redirect_uri") redirectUri: String,
        @Field("code_verifier") codeVerifier: String? = null
    ): Response<TokenResponse>

    @POST("connect/token")
    @FormUrlEncoded
    suspend fun refreshToken(
        @Field("grant_type") grantType: String,
        @Field("refresh_token") refreshToken: String,
        @Field("client_id") clientId: String,
        @Field("client_secret") clientSecret: String
    ): Response<TokenResponse>
}

interface TenantApiService {
    @GET("api/tenants/me")
    suspend fun getCurrentTenant(): Response<String>
}
