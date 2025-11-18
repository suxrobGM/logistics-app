package com.jfleets.driver.presentation.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.data.repository.LoadRepository
import com.jfleets.driver.model.Load
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class PastLoadsViewModel(
    private val loadRepository: LoadRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow<PastLoadsUiState>(PastLoadsUiState.Loading)
    val uiState: StateFlow<PastLoadsUiState> = _uiState.asStateFlow()

    init {
        loadPastLoads()
    }

    private fun loadPastLoads() {
        viewModelScope.launch {
            _uiState.value = PastLoadsUiState.Loading
            loadRepository.getPastLoads()
                .onSuccess { loads ->
                    _uiState.value = PastLoadsUiState.Success(loads)
                }
                .onFailure { error ->
                    _uiState.value =
                        PastLoadsUiState.Error(error.message ?: "Failed to load past loads")
                }
        }
    }

    fun refresh() {
        loadPastLoads()
    }
}

sealed class PastLoadsUiState {
    object Loading : PastLoadsUiState()
    data class Success(val loads: List<Load>) : PastLoadsUiState()
    data class Error(val message: String) : PastLoadsUiState()
}
