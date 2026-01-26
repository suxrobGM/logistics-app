package com.jfleets.driver.ui.screens

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.KeyboardArrowRight
import androidx.compose.material.icons.filled.BarChart
import androidx.compose.material.icons.filled.Save
import androidx.compose.material.icons.filled.Settings
import androidx.compose.ui.Alignment
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.jfleets.driver.api.models.UserDto
import com.jfleets.driver.ui.components.AppTopBar
import com.jfleets.driver.ui.components.CardContainer
import com.jfleets.driver.ui.components.ErrorView
import com.jfleets.driver.ui.components.LoadingIndicator
import com.jfleets.driver.ui.components.phone.PhoneNumberInput
import com.jfleets.driver.viewmodel.AccountUiState
import com.jfleets.driver.viewmodel.AccountViewModel
import com.jfleets.driver.viewmodel.SaveState
import kotlinx.coroutines.delay
import org.koin.compose.viewmodel.koinViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AccountScreen(
    onNavigateToStats: () -> Unit,
    onNavigateToSettings: () -> Unit,
    viewModel: AccountViewModel = koinViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()
    val saveState by viewModel.saveState.collectAsState()

    var firstName by remember { mutableStateOf("") }
    var lastName by remember { mutableStateOf("") }
    var phoneNumber by remember { mutableStateOf("") }
    var email by remember { mutableStateOf("") }
    var userId by remember { mutableStateOf("") }

    LaunchedEffect(uiState) {
        if (uiState is AccountUiState.Success) {
            val user = (uiState as AccountUiState.Success).user
            firstName = user.firstName ?: ""
            lastName = user.lastName ?: ""
            phoneNumber = user.phoneNumber ?: ""
            email = user.email ?: ""
            userId = user.id ?: ""
        }
    }

    LaunchedEffect(saveState) {
        if (saveState is SaveState.Success) {
            // Show success message and reset after a delay
            delay(2000)
            viewModel.resetSaveState()
        }
    }

    Scaffold(
        topBar = {
            AppTopBar(
                title = "Account",
                actions = {
                    if (uiState is AccountUiState.Success) {
                        IconButton(
                            onClick = {
                                viewModel.updateUser(
                                    UserDto(
                                        id = userId,
                                        email = email,
                                        firstName = firstName,
                                        lastName = lastName,
                                        phoneNumber = phoneNumber.ifBlank { null }
                                    )
                                )
                            },
                            enabled = saveState !is SaveState.Saving
                        ) {
                            Icon(Icons.Default.Save, "Save")
                        }
                    }
                }
            )
        }
    ) { paddingValues ->
        when (uiState) {
            is AccountUiState.Loading -> {
                LoadingIndicator()
            }

            is AccountUiState.Success -> {
                Column(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(paddingValues)
                        .verticalScroll(rememberScrollState())
                        .padding(16.dp),
                    verticalArrangement = Arrangement.spacedBy(16.dp)
                ) {
                    CardContainer {
                        Column(modifier = Modifier.padding(16.dp)) {
                            OutlinedTextField(
                                value = email,
                                onValueChange = { },
                                label = { Text("Email") },
                                enabled = false,
                                modifier = Modifier.fillMaxWidth()
                            )
                            Spacer(modifier = Modifier.height(16.dp))
                            OutlinedTextField(
                                value = firstName,
                                onValueChange = { firstName = it },
                                label = { Text("First Name") },
                                modifier = Modifier.fillMaxWidth()
                            )
                            Spacer(modifier = Modifier.height(16.dp))
                            OutlinedTextField(
                                value = lastName,
                                onValueChange = { lastName = it },
                                label = { Text("Last Name") },
                                modifier = Modifier.fillMaxWidth()
                            )
                            Spacer(modifier = Modifier.height(16.dp))
                            PhoneNumberInput(
                                fullPhoneNumber = phoneNumber,
                                onPhoneNumberChange = { phoneNumber = it },
                                label = "Phone Number"
                            )
                        }
                    }

                    when (saveState) {
                        is SaveState.Saving -> {
                            LinearProgressIndicator(modifier = Modifier.fillMaxWidth())
                        }

                        is SaveState.Success -> {
                            Text(
                                text = "Account updated successfully!",
                                color = MaterialTheme.colorScheme.primary,
                                modifier = Modifier.padding(horizontal = 16.dp)
                            )
                        }

                        is SaveState.Error -> {
                            Text(
                                text = "Error: ${(saveState as SaveState.Error).message}",
                                color = MaterialTheme.colorScheme.error,
                                modifier = Modifier.padding(horizontal = 16.dp)
                            )
                        }

                        else -> {}
                    }

                    // Quick Links Section
                    CardContainer {
                        Column {
                            MenuRow(
                                icon = Icons.Default.BarChart,
                                title = "Statistics",
                                onClick = onNavigateToStats
                            )
                            MenuRow(
                                icon = Icons.Default.Settings,
                                title = "Settings",
                                onClick = onNavigateToSettings
                            )
                        }
                    }
                }
            }

            is AccountUiState.Error -> {
                ErrorView(message = (uiState as AccountUiState.Error).message)
            }
        }
    }
}

@Composable
private fun MenuRow(
    icon: ImageVector,
    title: String,
    onClick: () -> Unit
) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .clickable(onClick = onClick)
            .padding(16.dp),
        horizontalArrangement = Arrangement.SpaceBetween,
        verticalAlignment = Alignment.CenterVertically
    ) {
        Row(
            horizontalArrangement = Arrangement.spacedBy(12.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Icon(
                imageVector = icon,
                contentDescription = null,
                tint = MaterialTheme.colorScheme.primary,
                modifier = Modifier.size(24.dp)
            )
            Text(
                text = title,
                style = MaterialTheme.typography.bodyLarge
            )
        }
        Icon(
            imageVector = Icons.AutoMirrored.Filled.KeyboardArrowRight,
            contentDescription = null,
            tint = MaterialTheme.colorScheme.onSurfaceVariant
        )
    }
}
