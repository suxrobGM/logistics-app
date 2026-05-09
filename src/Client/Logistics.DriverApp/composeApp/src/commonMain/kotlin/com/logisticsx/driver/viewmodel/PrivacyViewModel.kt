package com.logisticsx.driver.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.logisticsx.driver.api.PrivacyApi
import com.logisticsx.driver.api.models.DataDeletionRequestDto
import com.logisticsx.driver.api.models.DataDeletionStatus
import com.logisticsx.driver.api.models.DataExportRequestDto
import com.logisticsx.driver.api.models.RequestDataDeletionCommand
import com.logisticsx.driver.util.Logger
import com.logisticsx.driver.viewmodel.base.ActionState
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class PrivacyViewModel(private val privacyApi: PrivacyApi) : ViewModel() {

    private val _exports = MutableStateFlow<List<DataExportRequestDto>>(emptyList())
    val exports = _exports.asStateFlow()

    private val _deletions = MutableStateFlow<List<DataDeletionRequestDto>>(emptyList())
    val deletions = _deletions.asStateFlow()

    private val _isLoading = MutableStateFlow(true)
    val isLoading = _isLoading.asStateFlow()

    private val _exportAction = MutableStateFlow<ActionState<Unit>>(ActionState.Idle)
    val exportAction = _exportAction.asStateFlow()

    private val _deleteAction = MutableStateFlow<ActionState<Unit>>(ActionState.Idle)
    val deleteAction = _deleteAction.asStateFlow()

    init {
        refresh()
    }

    fun refresh() {
        viewModelScope.launch {
            _isLoading.value = true
            runCatching { privacyApi.getMyDataExports().body() }
                .onSuccess { _exports.value = it }
                .onFailure { Logger.e("Failed to load data exports: ${it.message}") }

            runCatching { privacyApi.getMyDataDeletions().body() }
                .onSuccess { _deletions.value = it }
                .onFailure { Logger.e("Failed to load deletion requests: ${it.message}") }

            _isLoading.value = false
        }
    }

    fun requestExport() {
        viewModelScope.launch {
            _exportAction.value = ActionState.Loading
            runCatching { privacyApi.requestDataExport() }
                .onSuccess {
                    _exportAction.value = ActionState.Success(Unit)
                    runCatching { privacyApi.getMyDataExports().body() }
                        .onSuccess { _exports.value = it }
                }
                .onFailure { _exportAction.value = ActionState.Error(it.message ?: "Request failed") }
        }
    }

    fun requestDeletion(reason: String?) {
        viewModelScope.launch {
            _deleteAction.value = ActionState.Loading
            runCatching {
                privacyApi.requestDataDeletion(RequestDataDeletionCommand(reason = reason))
            }
                .onSuccess {
                    _deleteAction.value = ActionState.Success(Unit)
                    runCatching { privacyApi.getMyDataDeletions().body() }
                        .onSuccess { _deletions.value = it }
                }
                .onFailure { _deleteAction.value = ActionState.Error(it.message ?: "Request failed") }
        }
    }

    fun cancelDeletion(id: String) {
        viewModelScope.launch {
            runCatching { privacyApi.cancelDataDeletion(id) }
                .onSuccess {
                    runCatching { privacyApi.getMyDataDeletions().body() }
                        .onSuccess { _deletions.value = it }
                }
                .onFailure { Logger.e("Failed to cancel deletion: ${it.message}") }
        }
    }

    fun resetExportAction() { _exportAction.value = ActionState.Idle }
    fun resetDeleteAction() { _deleteAction.value = ActionState.Idle }

    val pendingDeletion: DataDeletionRequestDto?
        get() = _deletions.value.firstOrNull { it.status == DataDeletionStatus.PENDING }
}
