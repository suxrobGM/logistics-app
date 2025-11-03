package com.jfleets.driver.presentation.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.data.model.Load
import com.jfleets.driver.data.repository.LoadRepository
import com.jfleets.driver.util.Result
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class PastLoadsViewModel @Inject constructor(
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
            when (val result = loadRepository.getPastLoads()) {
                is Result.Success -> {
                    _uiState.value = PastLoadsUiState.Success(result.data)
                }
                is Result.Error -> {
                    _uiState.value = PastLoadsUiState.Error(result.message)
                }
                else -> {}
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
