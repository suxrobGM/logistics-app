import Foundation
import SignalRClient

/// Bridge class to expose SignalR functionality to Kotlin via Objective-C interop.
/// Uses Microsoft's official SignalR Swift client.
@objc public class SignalRBridge: NSObject {
    private var connection: HubConnection?
    private let hubUrl: String
    private var authToken: String?
    private var tenantId: String?

    @objc public var isConnectedValue: Bool {
        return connection?.state == .connected
    }

    @objc public init(hubUrl: String) {
        self.hubUrl = hubUrl
        super.init()
    }

    @objc public func setAuthToken(_ token: String) {
        self.authToken = token
    }

    @objc public func setTenantId(_ tenantId: String) {
        self.tenantId = tenantId
    }

    @objc public func connect(completion: @escaping (Error?) -> Void) {
        guard let url = URL(string: hubUrl) else {
            completion(NSError(domain: "SignalRBridge", code: -1, userInfo: [NSLocalizedDescriptionKey: "Invalid hub URL"]))
            return
        }

        let builder = HubConnectionBuilder(url: url)
            .withAutoReconnect()
            .withLogging(minLogLevel: .warning)

        // Add authentication if token is set
        if let token = authToken {
            builder.withHttpConnectionOptions { options in
                options.accessTokenProvider = { token }
            }
        }

        // Add tenant header if set
        if let tenantId = tenantId {
            builder.withHttpConnectionOptions { options in
                options.headers["X-Tenant"] = tenantId
            }
        }

        connection = builder.build()

        connection?.on(method: "ReceiveNotification") { (message: String) in
            print("Received notification: \(message)")
        }

        connection?.start { [weak self] error in
            if let error = error {
                print("SignalR connection failed: \(error.localizedDescription)")
                completion(error)
            } else {
                print("SignalR connected successfully")
                completion(nil)
            }
        }
    }

    @objc public func disconnect(completion: @escaping (Error?) -> Void) {
        connection?.stop { error in
            if let error = error {
                print("SignalR disconnect failed: \(error.localizedDescription)")
            } else {
                print("SignalR disconnected")
            }
            completion(error)
        }
        connection = nil
    }

    @objc public func sendLocationUpdate(
        truckId: String,
        tenantId: String,
        latitude: Double,
        longitude: Double,
        truckNumber: String?,
        driversName: String?,
        addressLine1: String?,
        city: String?,
        state: String?,
        completion: @escaping (Error?) -> Void
    ) {
        guard let connection = connection, connection.state == .connected else {
            completion(NSError(domain: "SignalRBridge", code: -2, userInfo: [NSLocalizedDescriptionKey: "Not connected"]))
            return
        }

        let locationData: [String: Any] = [
            "truckId": truckId,
            "tenantId": tenantId,
            "currentLocation": [
                "latitude": latitude,
                "longitude": longitude
            ],
            "truckNumber": truckNumber ?? "",
            "driversName": driversName ?? "",
            "currentAddress": [
                "line1": addressLine1 ?? "",
                "city": city ?? "",
                "state": state ?? ""
            ]
        ]

        connection.invoke(method: "SendGeolocationData", arguments: [locationData]) { error in
            if let error = error {
                print("Failed to send location: \(error.localizedDescription)")
            }
            completion(error)
        }
    }
}
