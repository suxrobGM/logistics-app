package com.logisticsx.driver.ui.screens

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.Badge
import androidx.compose.material.icons.filled.Refresh
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import com.logisticsx.driver.api.models.DriverLicenseDto
import com.logisticsx.driver.api.models.DriverLicenseStatus
import com.logisticsx.driver.ui.components.AppTopBar
import com.logisticsx.driver.ui.components.CardContainer
import com.logisticsx.driver.ui.components.Chip
import com.logisticsx.driver.ui.components.DetailRow
import com.logisticsx.driver.ui.components.EmptyStateView
import com.logisticsx.driver.ui.components.UiStateContent
import com.logisticsx.driver.util.formatShort
import com.logisticsx.driver.viewmodel.MyLicensesViewModel
import org.koin.compose.viewmodel.koinViewModel
import kotlin.time.ExperimentalTime

@OptIn(ExperimentalMaterial3Api::class, ExperimentalTime::class)
@Composable
fun MyLicensesScreen(
    onNavigateBack: () -> Unit,
    viewModel: MyLicensesViewModel = koinViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()

    Scaffold(
        topBar = {
            AppTopBar(
                title = "My Licenses",
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, "Back")
                    }
                },
                actions = {
                    IconButton(onClick = { viewModel.refresh() }) {
                        Icon(Icons.Default.Refresh, "Refresh")
                    }
                }
            )
        }
    ) { paddingValues ->
        UiStateContent(uiState, viewModel::refresh) { licenses ->
            if (licenses.isEmpty()) {
                EmptyStateView(
                    icon = Icons.Default.Badge,
                    title = "No licenses on file",
                    message = "Ask your dispatcher to add your driver's license.",
                    modifier = Modifier.padding(paddingValues)
                )
            } else {
                LazyColumn(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(paddingValues),
                    contentPadding = PaddingValues(16.dp),
                    verticalArrangement = Arrangement.spacedBy(12.dp)
                ) {
                    items(licenses) { license ->
                        LicenseCard(license)
                    }
                }
            }
        }
    }
}

@OptIn(ExperimentalTime::class)
@Composable
private fun LicenseCard(license: DriverLicenseDto) {
    val (chipText, chipColor) = expiryChip(license)

    CardContainer {
        Column(
            modifier = Modifier
                .fillMaxWidth()
                .padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Text(
                    text = license.licenseClass?.value ?: "—",
                    style = MaterialTheme.typography.titleMedium
                )
                Chip(text = chipText, color = chipColor)
            }

            DetailRow(
                label = "Number",
                value = license.licenseNumber ?: "—"
            )
            DetailRow(
                label = "Country",
                value = listOfNotNull(license.issuingCountry, license.issuingRegion)
                    .joinToString(" / ")
                    .ifEmpty { "—" }
            )
            license.expiresAt?.let {
                DetailRow(label = "Expires", value = it.formatShort())
            }
            license.medicalCertExpiresAt?.let {
                DetailRow(label = "Medical cert expires", value = it.formatShort())
            }
        }
    }
}

@OptIn(ExperimentalTime::class)
@Composable
private fun expiryChip(license: DriverLicenseDto): Pair<String, Color> {
    val status = license.status
    val days = license.daysUntilExpiry ?: 0

    return when {
        status == DriverLicenseStatus.REVOKED -> "Revoked" to MaterialTheme.colorScheme.error
        status == DriverLicenseStatus.SUSPENDED -> "Suspended" to MaterialTheme.colorScheme.error
        status == DriverLicenseStatus.EXPIRED || days < 0 ->
            "Expired" to MaterialTheme.colorScheme.error
        days <= 7 -> "Expires in ${days}d" to MaterialTheme.colorScheme.error
        days <= 60 -> "Expires in ${days}d" to MaterialTheme.colorScheme.tertiary
        else -> "Active" to MaterialTheme.colorScheme.primary
    }
}
