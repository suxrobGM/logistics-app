package com.logisticsx.driver.util

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonPrimitive
import kotlinx.serialization.json.jsonObject
import kotlin.io.encoding.ExperimentalEncodingApi

private val jsonParser = Json { ignoreUnknownKeys = true }

/**
 * Decodes a JWT token and extracts the payload claims as a map.
 * Returns an empty map if the token is invalid.
 *
 * @param jwt The JWT token string.
 * @return A map of claim names to their string values.
 */
fun decodeJwtPayload(jwt: String): Map<String, String> {
    val parts = jwt.split(".")
    if (parts.size != 3) return emptyMap()

    val payload = parts[1]
    val decoded = decodeBase64Url(payload)
    return parseJsonClaims(decoded)
}

@OptIn(ExperimentalEncodingApi::class)
private fun decodeBase64Url(input: String): String {
    // Add padding if necessary
    val padded = when (input.length % 4) {
        2 -> "$input=="
        3 -> "$input="
        else -> input
    }
    // Convert base64url to base64
    val base64 = padded.replace('-', '+').replace('_', '/')
    val decoded = kotlin.io.encoding.Base64.decode(base64)
    return decoded.decodeToString()
}

private fun parseJsonClaims(jsonString: String): Map<String, String> {
    return try {
        val element = jsonParser.parseToJsonElement(jsonString)
        element.jsonObject
            .filterValues { it is JsonPrimitive }
            .mapValues { (_, value) -> (value as JsonPrimitive).content }
    } catch (e: Exception) {
        Logger.e("JwtUtils: Failed to parse JWT payload: ${e.message}")
        emptyMap()
    }
}
