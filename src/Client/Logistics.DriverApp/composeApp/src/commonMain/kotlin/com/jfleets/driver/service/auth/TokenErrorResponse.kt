package com.jfleets.driver.service.auth

import kotlinx.serialization.SerialName
import kotlinx.serialization.Serializable

@Serializable
data class TokenErrorResponse(
    val error: String,
    @SerialName("error_description") val errorDescription: String? = null
)