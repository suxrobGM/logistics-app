package com.jfleets.driver.data.model

data class UserInfo(
    val userId: String,
    val fullName: String,
    val tenantId: String,
    val roles: List<String>,
    val permissions: List<String>
)
