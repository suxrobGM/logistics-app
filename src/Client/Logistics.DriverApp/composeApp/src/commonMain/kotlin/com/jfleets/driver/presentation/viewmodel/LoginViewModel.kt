package com.jfleets.driver.presentation.viewmodel

import androidx.lifecycle.ViewModel
import com.jfleets.driver.data.auth.LoginService
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

class LoginViewModel(
    private val loginService: LoginService
) : ViewModel() {

    private val _uiState = MutableStateFlow<LoginUiState>(LoginUiState.Idle)
    val uiState: StateFlow<LoginUiState> = _uiState.asStateFlow()

    fun startLogin() {
        _uiState.value = LoginUiState.Loading
        loginService.startLogin { result ->
            result.onSuccess {
                _uiState.value = LoginUiState.Success
            }.onFailure { error ->
                _uiState.value = LoginUiState.Error(error.message ?: "Authentication failed")
            }
        }
    }

    fun resetState() {
        _uiState.value = LoginUiState.Idle
    }

    override fun onCleared() {
        super.onCleared()
        loginService.cancelLogin()
    }
}

sealed class LoginUiState {
    data object Idle : LoginUiState()
    data object Loading : LoginUiState()
    data object Success : LoginUiState()
    data class Error(val message: String) : LoginUiState()
}
