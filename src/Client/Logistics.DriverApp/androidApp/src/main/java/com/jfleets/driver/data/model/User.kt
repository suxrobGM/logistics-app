package com.jfleets.driver.data.model

data class User(
    val id: String,
    val email: String,
    val firstName: String,
    val lastName: String,
    val phoneNumber: String?
) {
    val fullName: String
        get() = "$firstName $lastName"
}

data class UserInfo(
    val userId: String,
    val fullName: String,
    val tenantId: String,
    val roles: List<String>,
    val permissions: List<String>
)
