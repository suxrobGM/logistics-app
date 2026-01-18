package com.jfleets.driver.model

data class UserInfo(
    val userId: String,
    val fullName: String,
    val tenantId: String,
    val role: String?,
    val permissions: List<String>
)
