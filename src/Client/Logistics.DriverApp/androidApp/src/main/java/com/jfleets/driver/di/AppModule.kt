package com.jfleets.driver.di

import android.content.Context
import com.jfleets.driver.data.auth.AuthService
import com.jfleets.driver.data.local.PreferencesManager
import com.jfleets.driver.data.local.TokenManager
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.android.qualifiers.ApplicationContext
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
object AppModule {

    @Provides
    @Singleton
    fun providePreferencesManager(
        @ApplicationContext context: Context
    ): PreferencesManager {
        return PreferencesManager(context)
    }

    @Provides
    @Singleton
    fun provideTokenManager(
        preferencesManager: PreferencesManager
    ): TokenManager {
        return TokenManager(preferencesManager)
    }

    @Provides
    @Singleton
    fun provideAuthService(
        @ApplicationContext context: Context
    ): AuthService {
        return AuthService(context)
    }
}
