package com.logisticsx.driver.ui.screens

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.Clear
import androidx.compose.material.icons.filled.Person
import androidx.compose.material.icons.filled.Search
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.logisticsx.driver.api.models.EmployeeDto
import com.logisticsx.driver.model.fullName
import com.logisticsx.driver.ui.components.AppTopBar
import com.logisticsx.driver.ui.components.EmptyStateView
import com.logisticsx.driver.ui.components.ErrorView
import com.logisticsx.driver.util.getInitials
import com.logisticsx.driver.viewmodel.EmployeeSelectViewModel
import com.logisticsx.driver.viewmodel.base.ActionState
import com.logisticsx.driver.viewmodel.base.UiState
import org.koin.compose.viewmodel.koinViewModel

@Composable
fun EmployeeSelectScreen(
    onConversationCreated: (String) -> Unit = {},
    onBack: () -> Unit = {},
    viewModel: EmployeeSelectViewModel = koinViewModel()
) {
    val searchQuery by viewModel.searchQuery.collectAsState()
    val searchState by viewModel.searchState.collectAsState()
    val createState by viewModel.createState.collectAsState()

    // Handle successful conversation creation
    LaunchedEffect(createState) {
        if (createState is ActionState.Success<*>) {
            @Suppress("UNCHECKED_CAST")
            val conversationId = (createState as ActionState.Success<String>).data
            viewModel.resetCreateState()
            onConversationCreated(conversationId)
        }
    }

    Scaffold(
        topBar = {
            AppTopBar(
                title = "New Message",
                navigationIcon = {
                    IconButton(onClick = onBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, "Back")
                    }
                }
            )
        }
    ) { paddingValues ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
        ) {
            // Search field
            OutlinedTextField(
                value = searchQuery,
                onValueChange = { viewModel.setSearchQuery(it) },
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(16.dp),
                placeholder = { Text("Search employees...") },
                leadingIcon = {
                    Icon(Icons.Default.Search, contentDescription = null)
                },
                trailingIcon = {
                    if (searchQuery.isNotEmpty()) {
                        IconButton(onClick = {
                            viewModel.setSearchQuery("")
                        }) {
                            Icon(Icons.Default.Clear, contentDescription = "Clear")
                        }
                    }
                },
                singleLine = true,
                shape = RoundedCornerShape(12.dp)
            )

            // Search results
            when (val state = searchState) {
                null -> {
                    if (searchQuery.isEmpty()) {
                        EmptyStateView(
                            icon = Icons.Default.Search,
                            title = "Search for employees",
                            message = "Enter at least 2 characters to search"
                        )
                    }
                }

                is UiState.Loading -> {
                    Box(
                        modifier = Modifier.fillMaxSize(),
                        contentAlignment = Alignment.Center
                    ) {
                        CircularProgressIndicator()
                    }
                }

                is UiState.Success<*> -> {
                    @Suppress("UNCHECKED_CAST")
                    val employees = (state as UiState.Success<List<EmployeeDto>>).data
                    if (employees.isEmpty()) {
                        EmptyStateView(
                            icon = Icons.Default.Person,
                            title = "No employees found",
                            message = "Try a different search term"
                        )
                    } else {
                        LazyColumn(
                            modifier = Modifier.fillMaxSize(),
                            contentPadding = PaddingValues(horizontal = 16.dp),
                            verticalArrangement = Arrangement.spacedBy(4.dp)
                        ) {
                            items(employees) { employee ->
                                EmployeeListItem(
                                    employee = employee,
                                    isCreating = createState is ActionState.Loading,
                                    onClick = {
                                        viewModel.startConversationWithEmployee(employee)
                                    }
                                )
                            }
                        }
                    }
                }

                is UiState.Error -> {
                    ErrorView(
                        message = state.message,
                        onRetry = { viewModel.retrySearch() }
                    )
                }
            }
        }
    }
}

@Composable
private fun EmployeeListItem(
    employee: EmployeeDto,
    isCreating: Boolean,
    onClick: () -> Unit
) {
    Surface(
        modifier = Modifier
            .fillMaxWidth()
            .clickable(enabled = !isCreating, onClick = onClick),
        shape = RoundedCornerShape(8.dp),
        color = MaterialTheme.colorScheme.surface
    ) {
        Row(
            modifier = Modifier.padding(12.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            // Avatar
            Surface(
                modifier = Modifier.size(40.dp),
                shape = CircleShape,
                color = MaterialTheme.colorScheme.primaryContainer
            ) {
                Box(contentAlignment = Alignment.Center) {
                    Text(
                        text = employee.fullName().getInitials(),
                        style = MaterialTheme.typography.titleSmall,
                        color = MaterialTheme.colorScheme.onPrimaryContainer
                    )
                }
            }

            Spacer(modifier = Modifier.width(12.dp))

            // Employee info
            Column(modifier = Modifier.weight(1f)) {
                Text(
                    text = employee.fullName(),
                    style = MaterialTheme.typography.bodyLarge,
                    fontWeight = FontWeight.Medium
                )

                employee.role?.displayName?.let { roleName ->
                    Text(
                        text = roleName,
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
            }

            // Loading indicator when creating conversation
            if (isCreating) {
                CircularProgressIndicator(
                    modifier = Modifier.size(24.dp),
                    strokeWidth = 2.dp
                )
            }
        }
    }
}
