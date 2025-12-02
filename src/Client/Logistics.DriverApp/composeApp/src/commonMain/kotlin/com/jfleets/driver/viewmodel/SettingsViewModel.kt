package com.jfleets.driver.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.model.DistanceUnit
import com.jfleets.driver.model.Language
import com.jfleets.driver.model.UserSettings
import com.jfleets.driver.service.PreferencesManager
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class SettingsViewModel(
    private val preferencesManager: PreferencesManager
) : ViewModel() {

    private val _settings = MutableStateFlow(UserSettings())
    val settings: StateFlow<UserSettings> = _settings.asStateFlow()

    private val _saveState = MutableStateFlow<SettingsSaveState>(SettingsSaveState.Idle)
    val saveState: StateFlow<SettingsSaveState> = _saveState.asStateFlow()

    init {
        loadSettings()
    }

    private fun loadSettings() {
        viewModelScope.launch {
            preferencesManager.getUserSettingsFlow().collect { userSettings ->
                _settings.value = userSettings
            }
        }
    }

    fun updateDistanceUnit(unit: DistanceUnit) {
        viewModelScope.launch {
            try {
                preferencesManager.saveDistanceUnit(unit)
                _settings.value = _settings.value.copy(distanceUnit = unit)
                _saveState.value = SettingsSaveState.Success
            } catch (e: Exception) {
                _saveState.value = SettingsSaveState.Error(e.message ?: "Failed to save setting")
            }
        }
    }

    fun updateLanguage(language: Language) {
        viewModelScope.launch {
            try {
                preferencesManager.saveLanguage(language)
                _settings.value = _settings.value.copy(language = language)
                _saveState.value = SettingsSaveState.Success
            } catch (e: Exception) {
                _saveState.value = SettingsSaveState.Error(e.message ?: "Failed to save setting")
            }
        }
    }

    fun resetSaveState() {
        _saveState.value = SettingsSaveState.Idle
    }
}

sealed class SettingsSaveState {
    object Idle : SettingsSaveState()
    object Success : SettingsSaveState()
    data class Error(val message: String) : SettingsSaveState()
}
