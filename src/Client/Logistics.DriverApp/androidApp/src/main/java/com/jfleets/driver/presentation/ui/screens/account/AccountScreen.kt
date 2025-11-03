package com.jfleets.driver.presentation.ui.screens.account

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Save
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import com.jfleets.driver.presentation.ui.components.CardContainer
import com.jfleets.driver.presentation.ui.components.ErrorView
import com.jfleets.driver.presentation.ui.components.LoadingIndicator
import com.jfleets.driver.presentation.viewmodel.AccountUiState
import com.jfleets.driver.presentation.viewmodel.AccountViewModel
import com.jfleets.driver.presentation.viewmodel.SaveState
import com.jfleets.driver.shared.model.User
import org.koin.androidx.compose.koinViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AccountScreen(
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
            firstName = user.firstName
            lastName = user.lastName
            phoneNumber = user.phoneNumber ?: ""
            email = user.email
            userId = user.id
        }
    }

    LaunchedEffect(saveState) {
        if (saveState is SaveState.Success) {
            // Show success message and reset after a delay
            kotlinx.coroutines.delay(2000)
            viewModel.resetSaveState()
        }
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Account") },
                actions = {
                    if (uiState is AccountUiState.Success) {
                        IconButton(
                            onClick = {
                                viewModel.updateUser(
                                    User(
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
                            OutlinedTextField(
                                value = phoneNumber,
                                onValueChange = { phoneNumber = it },
                                label = { Text("Phone Number") },
                                keyboardOptions = KeyboardOptions(
                                    keyboardType = KeyboardType.Phone
                                ),
                                modifier = Modifier.fillMaxWidth()
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
                }
            }

            is AccountUiState.Error -> {
                ErrorView(message = (uiState as AccountUiState.Error).message)
            }
        }
    }
}
