package com.jfleets.driver.ui.screens

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Language
import androidx.compose.material.icons.filled.Straighten
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.ModalBottomSheet
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.rememberModalBottomSheetState
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.jfleets.driver.model.DistanceUnit
import com.jfleets.driver.model.Language
import com.jfleets.driver.ui.components.AppTopBar
import com.jfleets.driver.ui.components.CardContainer
import com.jfleets.driver.ui.components.settings.SelectableItem
import com.jfleets.driver.ui.components.settings.SettingsItem
import com.jfleets.driver.viewmodel.SettingsViewModel
import kotlinx.coroutines.launch
import org.koin.compose.viewmodel.koinViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun SettingsScreen(
    viewModel: SettingsViewModel = koinViewModel()
) {
    val settings by viewModel.settings.collectAsState()
    var showDistanceUnitSheet by remember { mutableStateOf(false) }
    var showLanguageSheet by remember { mutableStateOf(false) }

    val distanceSheetState = rememberModalBottomSheetState()
    val languageSheetState = rememberModalBottomSheetState()
    val scope = rememberCoroutineScope()

    Scaffold(
        topBar = {
            AppTopBar(title = "Settings")
        }
    ) { paddingValues ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .verticalScroll(rememberScrollState())
                .padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            // Units Section
            Text(
                text = "Preferences",
                style = MaterialTheme.typography.titleMedium,
                color = MaterialTheme.colorScheme.primary
            )

            CardContainer {
                Column {
                    SettingsItem(
                        icon = Icons.Default.Straighten,
                        title = "Distance Unit",
                        value = settings.distanceUnit.displayName,
                        onClick = { showDistanceUnitSheet = true }
                    )

                    HorizontalDivider(
                        modifier = Modifier.padding(horizontal = 16.dp),
                        color = MaterialTheme.colorScheme.outlineVariant
                    )

                    SettingsItem(
                        icon = Icons.Default.Language,
                        title = "Language",
                        value = settings.language.displayName,
                        onClick = { showLanguageSheet = true }
                    )
                }
            }
        }
    }

    // Distance Unit Bottom Sheet
    if (showDistanceUnitSheet) {
        ModalBottomSheet(
            onDismissRequest = { showDistanceUnitSheet = false },
            sheetState = distanceSheetState
        ) {
            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(bottom = 32.dp)
            ) {
                Text(
                    text = "Distance Unit",
                    style = MaterialTheme.typography.titleLarge,
                    modifier = Modifier.padding(horizontal = 24.dp, vertical = 16.dp)
                )

                DistanceUnit.entries.forEach { unit ->
                    SelectableItem(
                        text = unit.displayName,
                        subtitle = "Display distances in ${unit.displayName.lowercase()}",
                        isSelected = settings.distanceUnit == unit,
                        onClick = {
                            viewModel.updateDistanceUnit(unit)
                            scope.launch {
                                distanceSheetState.hide()
                                showDistanceUnitSheet = false
                            }
                        }
                    )
                }
            }
        }
    }

    // Language Bottom Sheet
    if (showLanguageSheet) {
        ModalBottomSheet(
            onDismissRequest = { showLanguageSheet = false },
            sheetState = languageSheetState
        ) {
            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(bottom = 32.dp)
            ) {
                Text(
                    text = "Language",
                    style = MaterialTheme.typography.titleLarge,
                    modifier = Modifier.padding(horizontal = 24.dp, vertical = 16.dp)
                )

                Language.entries.forEach { language ->
                    SelectableItem(
                        text = language.displayName,
                        subtitle = null,
                        isSelected = settings.language == language,
                        onClick = {
                            viewModel.updateLanguage(language)
                            scope.launch {
                                languageSheetState.hide()
                                showLanguageSheet = false
                            }
                        }
                    )
                }
            }
        }
    }
}
