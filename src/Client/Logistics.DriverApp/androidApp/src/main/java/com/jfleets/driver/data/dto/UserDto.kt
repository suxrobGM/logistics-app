package com.jfleets.driver.data.dto

import com.google.gson.annotations.SerializedName

data class UserDto(
    @SerializedName("id")
    val id: String?,

    @SerializedName("email")
    val email: String?,

    @SerializedName("firstName")
    val firstName: String?,

    @SerializedName("lastName")
    val lastName: String?,

    @SerializedName("phoneNumber")
    val phoneNumber: String?
)

data class UpdateUser(
    @SerializedName("id")
    val id: String,

    @SerializedName("firstName")
    val firstName: String?,

    @SerializedName("lastName")
    val lastName: String?,

    @SerializedName("phoneNumber")
    val phoneNumber: String?
)

data class EmployeeDto(
    @SerializedName("id")
    val id: String?,

    @SerializedName("firstName")
    val firstName: String?,

    @SerializedName("lastName")
    val lastName: String?,

    @SerializedName("email")
    val email: String?,

    @SerializedName("role")
    val role: String?,

    @SerializedName("userId")
    val userId: String?
)
