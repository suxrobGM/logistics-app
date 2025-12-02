package com.jfleets.driver.ui.components.phone

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.ModalBottomSheet
import androidx.compose.material3.Text
import androidx.compose.material3.rememberModalBottomSheetState
import androidx.compose.runtime.Composable
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.jfleets.driver.model.CountryCode
import com.jfleets.driver.ui.components.settings.SelectableItem
import kotlinx.coroutines.launch

/**
 * A bottom sheet picker for selecting a country code.
 *
 * @param selectedCountry The currently selected country code
 * @param onCountrySelected Callback when a country is selected (sheet will be hidden automatically)
 * @param onDismiss Callback when the sheet is dismissed
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun CountryCodePicker(
    selectedCountry: CountryCode,
    onCountrySelected: (CountryCode) -> Unit,
    onDismiss: () -> Unit
) {
    val sheetState = rememberModalBottomSheetState()
    val scope = rememberCoroutineScope()

    ModalBottomSheet(
        onDismissRequest = onDismiss,
        sheetState = sheetState
    ) {
        Column(
            modifier = Modifier
                .fillMaxWidth()
                .padding(bottom = 32.dp)
        ) {
            Text(
                text = "Select Country",
                style = MaterialTheme.typography.titleLarge,
                modifier = Modifier.padding(horizontal = 24.dp, vertical = 16.dp)
            )

            CountryCode.entries.forEach { country ->
                SelectableItem(
                    text = country.fullDisplayText,
                    isSelected = selectedCountry == country,
                    onClick = {
                        scope.launch {
                            sheetState.hide()
                            onDismiss()
                        }
                        onCountrySelected(country)
                    }
                )
                if (country != CountryCode.entries.last()) {
                    HorizontalDivider(
                        modifier = Modifier.padding(horizontal = 24.dp),
                        color = MaterialTheme.colorScheme.outlineVariant
                    )
                }
            }
        }
    }
}
