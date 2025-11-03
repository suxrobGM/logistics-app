package com.jfleets.driver.presentation.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.data.model.User
import com.jfleets.driver.data.repository.UserRepository
import com.jfleets.driver.util.Result
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class AccountViewModel @Inject constructor(
    private val userRepository: UserRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow<AccountUiState>(AccountUiState.Loading)
    val uiState: StateFlow<AccountUiState> = _uiState.asStateFlow()

    private val _saveState = MutableStateFlow<SaveState>(SaveState.Idle)
    val saveState: StateFlow<SaveState> = _saveState.asStateFlow()

    init {
        loadUser()
    }

    private fun loadUser() {
        viewModelScope.launch {
            _uiState.value = AccountUiState.Loading
            when (val result = userRepository.getCurrentUser()) {
                is Result.Success -> {
                    _uiState.value = AccountUiState.Success(result.data)
                }
                is Result.Error -> {
                    _uiState.value = AccountUiState.Error(result.message)
                }
                else -> {}
            }
        }
    }

    fun updateUser(user: User) {
        viewModelScope.launch {
            _saveState.value = SaveState.Saving
            when (val result = userRepository.updateUser(user)) {
                is Result.Success -> {
                    _saveState.value = SaveState.Success
                    _uiState.value = AccountUiState.Success(user)
                }
                is Result.Error -> {
                    _saveState.value = SaveState.Error(result.message)
                }
                else -> {}
            }
        }
    }

    fun resetSaveState() {
        _saveState.value = SaveState.Idle
    }
}

sealed class AccountUiState {
    object Loading : AccountUiState()
    data class Success(val user: User) : AccountUiState()
    data class Error(val message: String) : AccountUiState()
}

sealed class SaveState {
    object Idle : SaveState()
    object Saving : SaveState()
    object Success : SaveState()
    data class Error(val message: String) : SaveState()
}
