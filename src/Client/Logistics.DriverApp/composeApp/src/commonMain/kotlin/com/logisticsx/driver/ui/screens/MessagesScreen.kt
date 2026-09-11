package com.logisticsx.driver.ui.screens

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
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.logisticsx.driver.ui.components.AppTopBar
import com.logisticsx.driver.ui.components.ConversationListItem
import com.logisticsx.driver.ui.components.EmptyStateView
import com.logisticsx.driver.ui.components.UiStateContent
import com.logisticsx.driver.ui.icons.AppIcons
import com.logisticsx.driver.viewmodel.ConversationListViewModel
import com.logisticsx.driver.viewmodel.DispatcherInfo
import com.logisticsx.driver.viewmodel.base.ActionState
import com.logisticsx.driver.viewmodel.base.UiState
import org.koin.compose.viewmodel.koinViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MessagesScreen(
    onConversationClick: (String) -> Unit = {},
    onNewMessage: () -> Unit = {},
    onBack: () -> Unit = {},
    viewModel: ConversationListViewModel = koinViewModel()
) {
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()
    val dispatcherInfo by viewModel.dispatcherInfo.collectAsStateWithLifecycle()
    val createState by viewModel.createState.collectAsStateWithLifecycle()
    val teamChatState by viewModel.teamChatState.collectAsStateWithLifecycle()
    val isRefreshing = uiState is UiState.Loading
    val isCreating = createState is ActionState.Loading

    LaunchedEffect(createState) {
        val state = createState
        if (state is ActionState.Success) {
            viewModel.resetCreateState()
            onConversationClick(state.data)
        }
    }

    LaunchedEffect(teamChatState) {
        val state = teamChatState
        if (state is ActionState.Success) {
            viewModel.resetTeamChatState()
            onConversationClick(state.data)
        }
    }

    Scaffold(
        topBar = {
            AppTopBar(
                title = "Messages",
                navigationIcon = {
                    IconButton(onClick = onBack) {
                        Icon(AppIcons.ArrowBack, "Back")
                    }
                },
                actions = {
                    IconButton(onClick = { viewModel.refresh() }) {
                        Icon(AppIcons.Refresh, "Refresh")
                    }
                }
            )
        },
        floatingActionButton = {
            val state = uiState
            val dispatcher = dispatcherInfo
            if (dispatcher != null && state is UiState.Success) {
                val conversations = state.data
                if (conversations.isNotEmpty()) {
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
                                AppIcons.Chat,
                                contentDescription = "Message ${dispatcher.name}"
                            )
                        }
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
            UiStateContent(uiState, onRetry = { viewModel.refresh() }) { conversations ->
                val isLoadingTeamChat = teamChatState is ActionState.Loading

                Column(modifier = Modifier.fillMaxSize()) {
                    Row(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(horizontal = 16.dp, vertical = 8.dp),
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        AssistChip(
                            onClick = { viewModel.openTeamChat() },
                            label = { Text("Company Chat") },
                            leadingIcon = {
                                if (isLoadingTeamChat) {
                                    CircularProgressIndicator(
                                        modifier = Modifier.size(18.dp),
                                        strokeWidth = 2.dp
                                    )
                                } else {
                                    Icon(
                                        AppIcons.Groups,
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
                                    AppIcons.PersonAdd,
                                    contentDescription = null,
                                    modifier = Modifier.size(AssistChipDefaults.IconSize)
                                )
                            }
                        )
                    }

                    if (conversations.isEmpty()) {
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
                            items(conversations) { conversation ->
                                ConversationListItem(
                                    conversation = conversation,
                                    onClick = { conversation.id?.let { onConversationClick(it) } }
                                )
                            }
                        }
                    }
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
        "Start a conversation with ${dispatcherInfo.name}, the dispatcher on your active load."
    } else {
        "Use New Message to reach someone at your company, or open Company Chat."
    }

    EmptyStateView(
        icon = AppIcons.Chat,
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
