package com.jfleets.driver.model

/**
 * Country codes for phone number input.
 */
enum class CountryCode(
    val dialCode: String,
    val countryName: String,
    val flag: String,
    val phoneLength: Int = 10
) {
    US("+1", "United States", "\uD83C\uDDFA\uD83C\uDDF8", 10),
    CA("+1", "Canada", "\uD83C\uDDE8\uD83C\uDDE6", 10),
    MX("+52", "Mexico", "\uD83C\uDDF2\uD83C\uDDFD", 10),
    GB("+44", "United Kingdom", "\uD83C\uDDEC\uD83C\uDDE7", 10),
    DE("+49", "Germany", "\uD83C\uDDE9\uD83C\uDDEA", 11),
    FR("+33", "France", "\uD83C\uDDEB\uD83C\uDDF7", 9),
    RU("+7", "Russia", "\uD83C\uDDF7\uD83C\uDDFA", 10),
    UZ("+998", "Uzbekistan", "\uD83C\uDDFA\uD83C\uDDFF", 9),
    IN("+91", "India", "\uD83C\uDDEE\uD83C\uDDF3", 10),
    CN("+86", "China", "\uD83C\uDDE8\uD83C\uDDF3", 11),
    BR("+55", "Brazil", "\uD83C\uDDE7\uD83C\uDDF7", 11),
    AU("+61", "Australia", "\uD83C\uDDE6\uD83C\uDDFA", 9);

    val displayText: String
        get() = "$flag $dialCode"

    val fullDisplayText: String
        get() = "$flag $countryName ($dialCode)"

    companion object {
        val DEFAULT = US

        fun fromDialCode(dialCode: String): CountryCode? {
            return entries.find { it.dialCode == dialCode }
        }

        /**
         * Parses a full phone number and extracts the country code and local number.
         * @param fullPhoneNumber The full phone number including country code (e.g., "+1234567890")
         * @return Pair of CountryCode and local number, or null if not parseable
         */
        fun parsePhoneNumber(fullPhoneNumber: String?): Pair<CountryCode, String> {
            if (fullPhoneNumber.isNullOrBlank()) {
                return DEFAULT to ""
            }

            val normalized = fullPhoneNumber.replace(Regex("[^+\\d]"), "")

            // Try to match country codes (longer codes first to handle +998 before +9)
            val sortedEntries = entries.sortedByDescending { it.dialCode.length }
            for (country in sortedEntries) {
                if (normalized.startsWith(country.dialCode)) {
                    val localNumber = normalized.removePrefix(country.dialCode)
                    return country to localNumber
                }
            }

            // If no country code found, assume it's a local number with default country
            return DEFAULT to normalized.removePrefix("+")
        }
    }
}
