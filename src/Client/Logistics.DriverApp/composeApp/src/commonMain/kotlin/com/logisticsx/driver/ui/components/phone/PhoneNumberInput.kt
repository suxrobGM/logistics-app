package com.logisticsx.driver.ui.components.phone

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.Icon
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import com.logisticsx.driver.model.CountryCode
import com.logisticsx.driver.ui.icons.AppIcons

/**
 * Phone number input with a country-code selector.
 *
 * [fullPhoneNumber] carries the country code, as in "+12025551234", and the callback reports the
 * same combined form.
 */
@Composable
fun PhoneNumberInput(
    fullPhoneNumber: String,
    onPhoneNumberChange: (String) -> Unit,
    modifier: Modifier = Modifier,
    label: String = "Phone Number",
    enabled: Boolean = true
) {
    val (countryCode, localNumber) = remember(fullPhoneNumber) {
        CountryCode.parsePhoneNumber(fullPhoneNumber)
    }

    var selectedCountry by remember(countryCode) { mutableStateOf(countryCode) }
    var phoneDigits by remember(localNumber) { mutableStateOf(localNumber) }
    var showCountryPicker by remember { mutableStateOf(false) }

    fun updateFullNumber(country: CountryCode, digits: String) {
        val cleanDigits = digits.filter { it.isDigit() }
        val fullNumber = if (cleanDigits.isNotEmpty()) {
            "${country.dialCode}$cleanDigits"
        } else {
            ""
        }
        onPhoneNumberChange(fullNumber)
    }

    Row(
        modifier = modifier.fillMaxWidth(),
        verticalAlignment = Alignment.Top
    ) {
        Box(
            modifier = Modifier
                .width(120.dp)
                .padding(end = 8.dp)
        ) {
            OutlinedTextField(
                value = selectedCountry.displayText,
                onValueChange = { },
                label = { Text("Code") },
                readOnly = true,
                enabled = enabled,
                singleLine = true,
                trailingIcon = {
                    Icon(
                        AppIcons.ArrowDropDown,
                        contentDescription = "Select country"
                    )
                },
                modifier = Modifier.fillMaxWidth()
            )

            // Invisible clickable overlay
            if (enabled) {
                Box(
                    modifier = Modifier
                        .matchParentSize()
                        .clickable { showCountryPicker = true }
                )
            }
        }

        OutlinedTextField(
            value = phoneDigits,
            onValueChange = { newValue ->
                val digitsOnly = newValue.filter { it.isDigit() }
                val limited = digitsOnly.take(selectedCountry.phoneLength)
                phoneDigits = limited
                updateFullNumber(selectedCountry, limited)
            },
            label = { Text(label) },
            enabled = enabled,
            singleLine = true,
            keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Phone),
            visualTransformation = PhoneVisualTransformation(selectedCountry),
            modifier = Modifier.weight(1f)
        )
    }

    if (showCountryPicker) {
        CountryCodePicker(
            selectedCountry = selectedCountry,
            onCountrySelected = { country ->
                selectedCountry = country
                updateFullNumber(country, phoneDigits)
            },
            onDismiss = { showCountryPicker = false }
        )
    }
}
