package com.jfleets.driver.service

import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import kotlinx.cinterop.ExperimentalForeignApi
import platform.Foundation.NSDocumentDirectory
import platform.Foundation.NSFileManager
import platform.Foundation.NSUserDomainMask

/**
 * Creates a DataStore instance for iOS using the documents directory.
 */
@OptIn(ExperimentalForeignApi::class)
fun createIosDataStore(): DataStore<Preferences> = createDataStore(
    producePath = {
        val documentDirectory = NSFileManager.defaultManager.URLForDirectory(
            directory = NSDocumentDirectory,
            inDomain = NSUserDomainMask,
            appropriateForURL = null,
            create = false,
            error = null
        )
        requireNotNull(documentDirectory).path + "/$DATA_STORE_FILE_NAME"
    }
)
