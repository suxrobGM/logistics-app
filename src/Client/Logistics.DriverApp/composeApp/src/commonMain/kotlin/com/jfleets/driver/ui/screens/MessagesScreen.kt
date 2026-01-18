package com.jfleets.driver.ui.screens

import androidx.compose.foundation.layout.Arrangement
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
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.automirrored.filled.Chat
import androidx.compose.material.icons.filled.Groups
import androidx.compose.material.icons.filled.PersonAdd
import androidx.compose.material.icons.filled.Refresh
import androidx.compose.material3.AssistChip
import androidx.compose.material3.AssistChipDefaults
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.FloatingActionButton
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.pulltorefresh.PullToRefreshBox
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.jfleets.driver.ui.components.AppTopBar
import com.jfleets.driver.ui.components.ConversationListItem
import com.jfleets.driver.ui.components.EmptyStateView
import com.jfleets.driver.ui.components.ErrorView
import com.jfleets.driver.ui.components.LoadingIndicator
import com.jfleets.driver.viewmodel.ConversationListUiState
import com.jfleets.driver.viewmodel.ConversationListViewModel
import com.jfleets.driver.viewmodel.CreateConversationState
import com.jfleets.driver.viewmodel.DispatcherInfo
import com.jfleets.driver.viewmodel.TeamChatState
import org.koin.compose.viewmodel.koinViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MessagesScreen(
    onConversationClick: (String) -> Unit = {},
    onNewMessage: () -> Unit = {},
    onBack: () -> Unit = {},
    viewModel: ConversationListViewModel = koinViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()
    val dispatcherInfo by viewModel.dispatcherInfo.collectAsState()
    val createState by viewModel.createState.collectAsState()
    val teamChatState by viewModel.teamChatState.collectAsState()
    val isRefreshing = uiState is ConversationListUiState.Loading
    val isCreating = createState is CreateConversationState.Creating

    // Handle successful conversation creation - navigate to the new conversation
    LaunchedEffect(createState) {
        if (createState is CreateConversationState.Success) {
            val conversationId = (createState as CreateConversationState.Success).conversationId
            viewModel.resetCreateState()
            onConversationClick(conversationId)
        }
    }

    // Handle successful team chat open - navigate to the team chat conversation
    LaunchedEffect(teamChatState) {
        if (teamChatState is TeamChatState.Success) {
            val conversationId = (teamChatState as TeamChatState.Success).conversationId
            viewModel.resetTeamChatState()
            onConversationClick(conversationId)
        }
    }

    Scaffold(
        topBar = {
            AppTopBar(
                title = "Messages",
                navigationIcon = {
                    IconButton(onClick = onBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, "Back")
                    }
                },
                actions = {
                    IconButton(onClick = { viewModel.refresh() }) {
                        Icon(Icons.Default.Refresh, "Refresh")
                    }
                }
            )
        },
        floatingActionButton = {
            // Show FAB to message dispatcher if dispatcher info is available and has conversations
            if (dispatcherInfo != null && uiState is ConversationListUiState.Success &&
                (uiState as ConversationListUiState.Success).conversations.isNotEmpty()
            ) {
                FloatingActionButton(
                    onClick = { viewModel.startConversationWithDispatcher() },
                    containerColor = MaterialTheme.colorScheme.primary
                ) {
                    if (isCreating) {
                        CircularProgressIndicator(
                            modifier = Modifier.size(24.dp),
                            color = MaterialTheme.colorScheme.onPrimary,
                            strokeWidth = 2.dp
                        )
                    } else {
                        Icon(
                            Icons.AutoMirrored.Filled.Chat,
                            contentDescription = "Message Dispatcher"
                        )
                    }
                }
            }
        }
    ) { paddingValues ->
        PullToRefreshBox(
            isRefreshing = isRefreshing,
            onRefresh = { viewModel.refresh() },
            modifier = Modifier.padding(paddingValues)
        ) {
            when (val state = uiState) {
                is ConversationListUiState.Loading -> {
                    LoadingIndicator()
                }

                is ConversationListUiState.Success -> {
                    val isLoadingTeamChat = teamChatState is TeamChatState.Loading

                    Column(modifier = Modifier.fillMaxSize()) {
                        // Action buttons row
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(horizontal = 16.dp, vertical = 8.dp),
                            horizontalArrangement = Arrangement.spacedBy(8.dp)
                        ) {
                            AssistChip(
                                onClick = { viewModel.openTeamChat() },
                                label = { Text("Team Chat") },
                                leadingIcon = {
                                    if (isLoadingTeamChat) {
                                        CircularProgressIndicator(
                                            modifier = Modifier.size(18.dp),
                                            strokeWidth = 2.dp
                                        )
                                    } else {
                                        Icon(
                                            Icons.Default.Groups,
                                            contentDescription = null,
                                            modifier = Modifier.size(AssistChipDefaults.IconSize)
                                        )
                                    }
                                },
                                enabled = !isLoadingTeamChat
                            )

                            AssistChip(
                                onClick = onNewMessage,
                                label = { Text("New Message") },
                                leadingIcon = {
                                    Icon(
                                        Icons.Default.PersonAdd,
                                        contentDescription = null,
                                        modifier = Modifier.size(AssistChipDefaults.IconSize)
                                    )
                                }
                            )
                        }

                        if (state.conversations.isEmpty()) {
                            EmptyMessagesView(
                                dispatcherInfo = dispatcherInfo,
                                isCreating = isCreating,
                                onStartConversation = { viewModel.startConversationWithDispatcher() }
                            )
                        } else {
                            LazyColumn(
                                modifier = Modifier.fillMaxSize(),
                                contentPadding = PaddingValues(horizontal = 16.dp, vertical = 8.dp),
                                verticalArrangement = Arrangement.spacedBy(8.dp)
                            ) {
                                items(state.conversations) { conversation ->
                                    ConversationListItem(
                                        conversation = conversation,
                                        onClick = { conversation.id?.let { onConversationClick(it) } }
                                    )
                                }
                            }
                        }
                    }
                }

                is ConversationListUiState.Error -> {
                    ErrorView(
                        message = state.message,
                        onRetry = { viewModel.refresh() }
                    )
                }
            }
        }
    }
}

@Composable
private fun EmptyMessagesView(
    dispatcherInfo: DispatcherInfo?,
    isCreating: Boolean,
    onStartConversation: () -> Unit
) {
    val message = if (dispatcherInfo != null) {
        "Start a conversation with your dispatcher"
    } else {
        "You'll be able to message your dispatcher once you have an active load"
    }

    EmptyStateView(
        icon = Icons.AutoMirrored.Filled.Chat,
        title = "No conversations yet",
        message = message,
        action = if (dispatcherInfo != null) {
            {
                Button(
                    onClick = onStartConversation,
                    enabled = !isCreating
                ) {
                    if (isCreating) {
                        CircularProgressIndicator(
                            modifier = Modifier.size(20.dp),
                            color = MaterialTheme.colorScheme.onPrimary,
                            strokeWidth = 2.dp
                        )
                        Spacer(modifier = Modifier.width(8.dp))
                    }
                    Text("Message ${dispatcherInfo.name}")
                }
            }
        } else null
    )
}
