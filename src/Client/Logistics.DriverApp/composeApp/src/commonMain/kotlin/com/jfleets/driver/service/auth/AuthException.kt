package com.jfleets.driver.service.auth

class AuthException(
    val error: String,
    override val message: String
) : Exception(message)