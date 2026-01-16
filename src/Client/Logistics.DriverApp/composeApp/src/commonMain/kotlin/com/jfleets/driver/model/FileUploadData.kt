package com.jfleets.driver.model

import io.ktor.client.request.forms.FormPart
import io.ktor.client.request.forms.InputProvider
import io.ktor.http.HttpHeaders
import io.ktor.http.headersOf
import io.ktor.utils.io.core.ByteReadPacket

/**
 * Data class representing file upload information for multipart form requests.
 */
data class FileUploadData(
    val bytes: ByteArray,
    val fileName: String,
    val contentType: String
) {
    /**
     * Converts this FileUploadData to a Ktor FormPart for multipart form requests.
     */
    fun toFormPart(fieldName: String = "Photos"): FormPart<InputProvider> {
        return FormPart(
            key = fieldName,
            value = InputProvider { ByteReadPacket(bytes) },
            headers = headersOf(
                HttpHeaders.ContentDisposition to listOf("form-data; name=\"$fieldName\"; filename=\"$fileName\""),
                HttpHeaders.ContentType to listOf(contentType)
            )
        )
    }

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other == null || this::class != other::class) return false

        other as FileUploadData

        if (!bytes.contentEquals(other.bytes)) return false
        if (fileName != other.fileName) return false
        if (contentType != other.contentType) return false

        return true
    }

    override fun hashCode(): Int {
        var result = bytes.contentHashCode()
        result = 31 * result + fileName.hashCode()
        result = 31 * result + contentType.hashCode()
        return result
    }
}

/**
 * Extension function to convert a list of FileUploadData to FormParts.
 */
fun List<FileUploadData>.toFormParts(fieldName: String = "Photos"): List<FormPart<InputProvider>> {
    return this.map { it.toFormPart(fieldName) }
}
