package com.jfleets.driver.presentation.auth

import android.app.Activity
import android.content.Intent
import android.os.Bundle
import androidx.activity.ComponentActivity

/**
 * Activity that handles OAuth redirect callback.
 * This activity receives the authorization code from the OAuth provider
 * and returns it to the calling activity.
 */
class OAuthRedirectActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val data = intent.data
        if (data != null) {
            // Return the intent data back to the calling activity
            setResult(Activity.RESULT_OK, Intent().setData(data))
        } else {
            setResult(Activity.RESULT_CANCELED)
        }
        finish()
    }
}
