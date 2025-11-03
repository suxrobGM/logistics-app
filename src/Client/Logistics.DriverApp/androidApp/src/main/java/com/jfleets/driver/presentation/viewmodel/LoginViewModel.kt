package com.jfleets.driver.presentation.viewmodel

import android.content.Intent
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.data.repository.AuthRepository
import com.jfleets.driver.util.Result
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class LoginViewModel @Inject constructor(
    private val authRepository: AuthRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow<LoginUiState>(LoginUiState.Idle)
    val uiState: StateFlow<LoginUiState> = _uiState.asStateFlow()

    init {
        checkAutoLogin()
    }

    private fun checkAutoLogin() {
        viewModelScope.launch {
            val isLoggedIn = authRepository.isLoggedIn()
            if (isLoggedIn) {
                _uiState.value = LoginUiState.Success
            }
        }
    }

    fun getLoginIntent(): Intent {
        return authRepository.getLoginIntent()
    }

    fun handleAuthorizationResponse(intent: Intent) {
        viewModelScope.launch {
            _uiState.value = LoginUiState.Loading
            when (val result = authRepository.handleAuthorizationResponse(intent)) {
                is Result.Success -> {
                    _uiState.value = LoginUiState.Success
                }
                is Result.Error -> {
                    _uiState.value = LoginUiState.Error(result.message)
                }
                else -> {}
            }
        }
    }

    fun resetState() {
        _uiState.value = LoginUiState.Idle
    }
}

sealed class LoginUiState {
    object Idle : LoginUiState()
    object Loading : LoginUiState()
    object Success : LoginUiState()
    data class Error(val message: String) : LoginUiState()
}
