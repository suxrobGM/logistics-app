package com.jfleets.driver.util

import kotlin.io.encoding.ExperimentalEncodingApi

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

private fun parseJsonClaims(json: String): Map<String, String> {
    val result = mutableMapOf<String, String>()
    // Simple JSON parsing for string values
    val cleanJson = json.trim().removeSurrounding("{", "}")
    val pairs = cleanJson.split(",")

    for (pair in pairs) {
        val keyValue = pair.split(":", limit = 2)
        if (keyValue.size == 2) {
            val key = keyValue[0].trim().removeSurrounding("\"")
            val value = keyValue[1].trim().removeSurrounding("\"")
            // Only store string values (not arrays or objects)
            if (!value.startsWith("[") && !value.startsWith("{")) {
                result[key] = value
            }
        }
    }
    return result
}
