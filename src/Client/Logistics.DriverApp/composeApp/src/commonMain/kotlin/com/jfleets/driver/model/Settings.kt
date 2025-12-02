package com.jfleets.driver.model

import androidx.compose.runtime.compositionLocalOf

/**
 * CompositionLocal for accessing user settings throughout the app.
 */
val LocalUserSettings = compositionLocalOf { UserSettings() }

/**
 * Distance unit for displaying distances in the app.
 */
enum class DistanceUnit(val code: String, val displayName: String, val abbreviation: String) {
    MILES("mi", "Miles", "mi"),
    KILOMETERS("km", "Kilometers", "km");

    companion object {
        fun fromCode(code: String): DistanceUnit {
            return entries.find { it.code == code } ?: MILES
        }
    }
}

/**
 * Supported languages for the app.
 */
enum class Language(val code: String, val displayName: String) {
    ENGLISH("en", "English"),
    RUSSIAN("ru", "Russian"),
    UZBEK("uz", "Uzbek");

    companion object {
        fun fromCode(code: String): Language {
            return entries.find { it.code == code } ?: ENGLISH
        }
    }
}

/**
 * User settings data class.
 */
data class UserSettings(
    val distanceUnit: DistanceUnit = DistanceUnit.MILES,
    val language: Language = Language.ENGLISH
)
