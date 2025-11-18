package com.jfleets.driver.presentation.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.data.local.PreferencesManager
import com.jfleets.driver.data.repository.UserRepository
import com.jfleets.driver.model.User
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class AccountViewModel(
    private val userRepository: UserRepository,
    private val preferencesManager: PreferencesManager
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
            val userId = preferencesManager.getUserId()
            if (userId.isNullOrEmpty()) {
                _uiState.value = AccountUiState.Error("User ID not available")
                return@launch
            }

            userRepository.getCurrentUser(userId)
                .onSuccess { user ->
                    _uiState.value = AccountUiState.Success(user)
                }
                .onFailure { error ->
                    _uiState.value = AccountUiState.Error(error.message ?: "Failed to load user")
                }
        }
    }

    fun updateUser(user: User) {
        viewModelScope.launch {
            _saveState.value = SaveState.Saving
            userRepository.updateUser(user)
                .onSuccess {
                    _saveState.value = SaveState.Success
                    _uiState.value = AccountUiState.Success(user)
                }
                .onFailure { error ->
                    _saveState.value = SaveState.Error(error.message ?: "Failed to update user")
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
