package com.jfleets.driver.data.dto

import com.google.gson.annotations.SerializedName

data class DeviceTokenDto(
    @SerializedName("token")
    val token: String
)

data class LoginRequest(
    @SerializedName("code")
    val code: String,

    @SerializedName("codeVerifier")
    val codeVerifier: String?
)

data class TokenResponse(
    @SerializedName("access_token")
    val accessToken: String,

    @SerializedName("refresh_token")
    val refreshToken: String?,

    @SerializedName("id_token")
    val idToken: String?,

    @SerializedName("token_type")
    val tokenType: String,

    @SerializedName("expires_in")
    val expiresIn: Int
)

data class RefreshTokenRequest(
    @SerializedName("refresh_token")
    val refreshToken: String
)
