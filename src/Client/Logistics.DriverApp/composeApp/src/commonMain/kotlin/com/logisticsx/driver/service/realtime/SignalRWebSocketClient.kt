package com.logisticsx.driver.service.realtime

import com.logisticsx.driver.util.Logger
import io.ktor.client.HttpClient
import io.ktor.client.plugins.websocket.WebSockets
import io.ktor.client.plugins.websocket.webSocket
import io.ktor.client.request.get
import io.ktor.client.request.header
import io.ktor.client.request.parameter
import io.ktor.client.statement.bodyAsText
import io.ktor.http.URLBuilder
import io.ktor.http.URLProtocol
import io.ktor.websocket.Frame
import io.ktor.websocket.WebSocketSession
import io.ktor.websocket.close
import io.ktor.websocket.readText
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.cancel
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.launch
import kotlinx.coroutines.withTimeoutOrNull
import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonArray
import kotlinx.serialization.json.JsonElement
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.JsonPrimitive
import kotlinx.serialization.json.int
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive

/**
 * Pure Kotlin implementation of the SignalR JSON Hub Protocol over Ktor WebSocket.
 * Used on iOS where the Java SignalR client is unavailable.
 *
 * Implements the SignalR handshake, message framing (record separator \u001E),
 * hub invocations, and server-to-client method handlers.
 */
class SignalRWebSocketClient(
    private val hubUrl: String,
    private val accessToken: String,
    private val tenantId: String
) {
    companion object {
        private const val RECORD_SEPARATOR = '\u001E'
        private const val HANDSHAKE_REQUEST = """{"protocol":"json","version":1}"""
        private const val CONNECTION_TIMEOUT_MS = 5000L
    }

    private val json = Json { ignoreUnknownKeys = true; isLenient = true }
    private val handlers = mutableMapOf<String, (JsonObject) -> Unit>()
    private var scope: CoroutineScope? = null
    private var pendingSends = mutableListOf<String>()
    private var webSocketSession: WebSocketSession? = null
    private var invocationId = 0

    private val _connectionState = MutableStateFlow(false)
    val connectionState: StateFlow<Boolean> = _connectionState.asStateFlow()
    val isConnected: Boolean get() = _connectionState.value

    private val client = HttpClient {
        install(WebSockets)
    }

    /**
     * Register a handler for a server-to-client method invocation.
     */
    fun on(method: String, handler: (JsonObject) -> Unit) {
        handlers[method] = handler
    }

    /**
     * Connect to the SignalR hub. Performs negotiate then WebSocket handshake.
     */
    suspend fun connect() {
        val connectionToken = negotiateConnection()

        val wsScope = CoroutineScope(Dispatchers.Default + SupervisorJob())
        scope = wsScope

        wsScope.launch {
            try {
                val wsUrl = buildWebSocketUrl(connectionToken)
                client.webSocket(urlString = wsUrl, request = {
                    header("Authorization", "Bearer $accessToken")
                    header("X-Tenant", tenantId)
                }) {
                    webSocketSession = this
                    performHandshake()
                    _connectionState.value = true
                    Logger.d("SignalR WS: Connected and handshake complete")
                    flushPendingSends()
                    startMessageLoop()
                    // Connection closed normally
                    _connectionState.value = false
                    Logger.d("SignalR WS: Connection closed")
                }
            } catch (e: Exception) {
                _connectionState.value = false
                Logger.e("SignalR WS: Error: ${e.message}")
            }
        }

        // Wait for connection using StateFlow instead of polling
        val connected = withTimeoutOrNull(CONNECTION_TIMEOUT_MS) {
            _connectionState.first { it }
        }
        if (connected != true) {
            throw IllegalStateException("SignalR WS: Connection timeout")
        }
    }

    /**
     * Invoke a hub method with arguments (expects a response).
     */
    suspend fun invoke(method: String, vararg args: JsonElement) {
        val message = JsonObject(buildMap {
            put("type", JsonPrimitive(1)) // Invocation
            put("invocationId", JsonPrimitive((++invocationId).toString()))
            put("target", JsonPrimitive(method))
            put("arguments", JsonArray(args.toList()))
        })
        sendOrQueue(message)
    }

    /**
     * Send a hub method invocation (fire-and-forget, no invocationId).
     */
    suspend fun send(method: String, vararg args: JsonElement) {
        val message = JsonObject(buildMap {
            put("type", JsonPrimitive(1)) // Invocation
            put("target", JsonPrimitive(method))
            put("arguments", JsonArray(args.toList()))
        })
        sendOrQueue(message)
    }

    /**
     * Disconnect from the SignalR hub.
     */
    suspend fun disconnect() {
        _connectionState.value = false
        try {
            webSocketSession?.close()
        } catch (_: Exception) { }
        webSocketSession = null
        scope?.cancel()
        scope = null
        Logger.d("SignalR WS: Disconnected")
    }

    // --- Private helpers ---

    private suspend fun negotiateConnection(): String {
        val negotiateUrl = hubUrl.trimEnd('/') + "/negotiate"
        val result = try {
            val response = client.get(negotiateUrl) {
                parameter("negotiateVersion", 1)
                header("Authorization", "Bearer $accessToken")
                header("X-Tenant", tenantId)
            }
            val body = response.bodyAsText()
            json.parseToJsonElement(body).jsonObject
        } catch (e: Exception) {
            Logger.e("SignalR WS: Negotiate failed: ${e.message}")
            throw IllegalStateException("SignalR negotiate failed: ${e.message}", e)
        }

        val connectionToken = result["connectionToken"]?.jsonPrimitive?.content
            ?: throw IllegalStateException("SignalR negotiate failed: no connectionToken")
        val connectionId = result["connectionId"]?.jsonPrimitive?.content
        Logger.d("SignalR WS: Negotiated connectionId=$connectionId")
        return connectionToken
    }

    private suspend fun WebSocketSession.performHandshake() {
        send(Frame.Text("$HANDSHAKE_REQUEST$RECORD_SEPARATOR"))
        val handshakeFrame = incoming.receive() as? Frame.Text
            ?: throw IllegalStateException("SignalR WS: Invalid handshake response")
        val handshakeResponse = handshakeFrame.readText().trimEnd(RECORD_SEPARATOR)
        val handshakeJson = json.parseToJsonElement(handshakeResponse).jsonObject
        if (handshakeJson.containsKey("error")) {
            throw IllegalStateException(
                "SignalR handshake error: ${handshakeJson["error"]?.jsonPrimitive?.content}"
            )
        }
    }

    private suspend fun WebSocketSession.flushPendingSends() {
        pendingSends.forEach { msg ->
            send(Frame.Text("$msg$RECORD_SEPARATOR"))
        }
        pendingSends.clear()
    }

    private suspend fun WebSocketSession.startMessageLoop() {
        for (frame in incoming) {
            if (frame is Frame.Text) {
                val text = frame.readText()
                text.split(RECORD_SEPARATOR)
                    .filter { it.isNotBlank() }
                    .forEach { handleMessage(it) }
            }
        }
    }

    private suspend fun sendOrQueue(message: JsonObject) {
        val text = json.encodeToString(JsonObject.serializer(), message)
        val session = webSocketSession
        if (session != null && isConnected) {
            session.send(Frame.Text("$text$RECORD_SEPARATOR"))
        } else {
            pendingSends.add(text)
        }
    }

    private fun buildWebSocketUrl(connectionToken: String): String {
        val builder = URLBuilder(hubUrl)
        builder.protocol = if (hubUrl.startsWith("https")) URLProtocol.WSS else URLProtocol.WS
        builder.parameters.append("id", connectionToken)
        return builder.buildString()
    }

    private fun handleMessage(text: String) {
        try {
            val msg = json.parseToJsonElement(text).jsonObject
            val type = msg["type"]?.jsonPrimitive?.int ?: return

            when (type) {
                1 -> { // Invocation
                    val target = msg["target"]?.jsonPrimitive?.content ?: return
                    val handler = handlers[target]
                    if (handler != null) {
                        handler(msg)
                    } else {
                        Logger.d("SignalR WS: No handler for method '$target'")
                    }
                }
                6 -> { // Ping - respond with pong
                    scope?.launch {
                        val pong = """{"type":6}"""
                        webSocketSession?.send(Frame.Text("$pong$RECORD_SEPARATOR"))
                    }
                }
                7 -> { // Close
                    val error = msg["error"]?.jsonPrimitive?.content
                    if (error != null) {
                        Logger.e("SignalR WS: Server closed with error: $error")
                    }
                    _connectionState.value = false
                }
                3 -> { // Completion
                    Logger.d("SignalR WS: Invocation completed")
                }
            }
        } catch (e: Exception) {
            Logger.e("SignalR WS: Error handling message: ${e.message}")
        }
    }
}
