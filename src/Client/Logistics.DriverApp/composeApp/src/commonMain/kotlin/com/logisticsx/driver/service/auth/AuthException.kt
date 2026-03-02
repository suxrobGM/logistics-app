package com.logisticsx.driver.service.auth

class AuthException(
    val error: String,
    override val message: String
) : Exception(message)
