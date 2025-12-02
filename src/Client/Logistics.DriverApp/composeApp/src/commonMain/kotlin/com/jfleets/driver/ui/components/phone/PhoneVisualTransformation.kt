package com.jfleets.driver.ui.components.phone

import androidx.compose.ui.text.AnnotatedString
import androidx.compose.ui.text.input.OffsetMapping
import androidx.compose.ui.text.input.TransformedText
import androidx.compose.ui.text.input.VisualTransformation
import com.jfleets.driver.model.CountryCode

/**
 * Visual transformation for formatting phone numbers.
 * Formats US/CA numbers as (XXX) XXX-XXXX
 */
internal class PhoneVisualTransformation(
    private val countryCode: CountryCode
) : VisualTransformation {
    override fun filter(text: AnnotatedString): TransformedText {
        val digits = text.text.filter { it.isDigit() }

        val formatted = when (countryCode) {
            CountryCode.US, CountryCode.CA -> formatNorthAmerican(digits)
            else -> formatGeneric(digits)
        }

        return TransformedText(
            AnnotatedString(formatted),
            PhoneOffsetMapping(digits, formatted, countryCode)
        )
    }

    private fun formatNorthAmerican(digits: String): String {
        return buildString {
            digits.forEachIndexed { index, char ->
                when (index) {
                    0 -> append("($char")
                    2 -> append("$char) ")
                    5 -> append("$char-")
                    else -> append(char)
                }
            }
        }
    }

    private fun formatGeneric(digits: String): String {
        return buildString {
            digits.forEachIndexed { index, char ->
                if (index > 0 && index % 3 == 0) {
                    append(' ')
                }
                append(char)
            }
        }
    }
}

/**
 * Maps cursor offsets between original and transformed phone number strings.
 */
private class PhoneOffsetMapping(
    private val original: String,
    private val formatted: String,
    private val countryCode: CountryCode
) : OffsetMapping {
    override fun originalToTransformed(offset: Int): Int {
        if (original.isEmpty()) return 0

        return when (countryCode) {
            CountryCode.US, CountryCode.CA -> {
                when {
                    offset <= 0 -> 0
                    offset <= 1 -> offset + 1  // After (
                    offset <= 3 -> offset + 1  // Inside area code
                    offset <= 6 -> offset + 3  // After ") "
                    offset <= 10 -> offset + 4 // After "-"
                    else -> formatted.length
                }
            }

            else -> {
                val spaces = (offset - 1) / 3
                minOf(offset + spaces, formatted.length)
            }
        }
    }

    override fun transformedToOriginal(offset: Int): Int {
        if (formatted.isEmpty()) return 0

        return when (countryCode) {
            CountryCode.US, CountryCode.CA -> {
                when {
                    offset <= 1 -> 0           // Before or at (
                    offset <= 4 -> offset - 1  // Inside area code
                    offset <= 6 -> 3           // At ") "
                    offset <= 9 -> offset - 3  // After ") "
                    offset <= 10 -> 6          // At "-"
                    offset <= 14 -> offset - 4 // After "-"
                    else -> original.length
                }
            }

            else -> {
                var digitCount = 0
                for (i in 0 until minOf(offset, formatted.length)) {
                    if (formatted[i].isDigit()) digitCount++
                }
                digitCount
            }
        }
    }
}
