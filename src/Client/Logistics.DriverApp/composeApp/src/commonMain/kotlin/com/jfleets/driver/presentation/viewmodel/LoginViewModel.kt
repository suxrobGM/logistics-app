package com.jfleets.driver.presentation.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.service.auth.AuthException
import com.jfleets.driver.service.auth.AuthService
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class LoginViewModel(
    private val authService: AuthService
) : ViewModel() {

    private val _uiState = MutableStateFlow<LoginUiState>(LoginUiState.Idle)
    val uiState: StateFlow<LoginUiState> = _uiState.asStateFlow()

    private val _username = MutableStateFlow("")
    val username: StateFlow<String> = _username.asStateFlow()

    private val _password = MutableStateFlow("")
    val password: StateFlow<String> = _password.asStateFlow()

    fun onUsernameChange(value: String) {
        _username.value = value
    }

    fun onPasswordChange(value: String) {
        _password.value = value
    }

    fun login() {
        val currentUsername = _username.value.trim()
        val currentPassword = _password.value

        if (currentUsername.isBlank()) {
            _uiState.value = LoginUiState.Error("Please enter your username")
            return
        }

        if (currentPassword.isBlank()) {
            _uiState.value = LoginUiState.Error("Please enter your password")
            return
        }

        _uiState.value = LoginUiState.Loading

        viewModelScope.launch {
            authService.login(currentUsername, currentPassword)
                .onSuccess {
                    _uiState.value = LoginUiState.Success
                }
                .onFailure { exception ->
                    val message = when (exception) {
                        is AuthException -> when (exception.error) {
                            "invalid_grant" -> "Invalid username or password"
                            "invalid_client" -> "Authentication configuration error"
                            else -> exception.message
                        }

                        else -> exception.message ?: "Authentication failed"
                    }
                    _uiState.value = LoginUiState.Error(message)
                }
        }
    }

    fun resetState() {
        _uiState.value = LoginUiState.Idle
    }

    fun clearForm() {
        _username.value = ""
        _password.value = ""
        _uiState.value = LoginUiState.Idle
    }
}

sealed class LoginUiState {
    data object Idle : LoginUiState()
    data object Loading : LoginUiState()
    data object Success : LoginUiState()
    data class Error(val message: String) : LoginUiState()
}
