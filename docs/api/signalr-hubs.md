# SignalR Hubs

Real-time communication via SignalR WebSocket connections.

## Available Hubs

| Hub | URL | Purpose |
|-----|-----|---------|
| Live Tracking | `/hubs/live-tracking` | GPS location updates |
| Notifications | `/hubs/notification` | Push notifications |

## Connecting to Hubs

### JavaScript/TypeScript

```typescript
import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder()
  .withUrl('https://api.yourdomain.com/hubs/notification', {
    accessTokenFactory: () => getAccessToken()
  })
  .withAutomaticReconnect()
  .build();

await connection.start();
```

### Authentication

Hubs require JWT authentication. Pass the token via query string or header:

```typescript
const connection = new signalR.HubConnectionBuilder()
  .withUrl('/hubs/notification', {
    accessTokenFactory: () => localStorage.getItem('access_token')
  })
  .build();
```

## Live Tracking Hub

Real-time GPS location tracking for drivers.

### Client → Server Methods

**SendLocation**: Driver sends their GPS location

```typescript
await connection.invoke('SendLocation', {
  latitude: 41.8781,
  longitude: -87.6298,
  heading: 90,
  speed: 65
});
```

### Server → Client Events

**ReceiveLocation**: Receive driver location updates

```typescript
connection.on('ReceiveLocation', (driverId: string, location: Location) => {
  console.log(`Driver ${driverId} at ${location.latitude}, ${location.longitude}`);
  updateMapMarker(driverId, location);
});
```

**DriverOffline**: Driver disconnected

```typescript
connection.on('DriverOffline', (driverId: string) => {
  removeMapMarker(driverId);
});
```

### Groups

Dispatchers automatically join groups for their tenant's drivers:

```typescript
// Server-side (automatic on connection)
await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant-{tenantId}");
```

## Notification Hub

Real-time notifications for all users.

### Server → Client Events

**ReceiveNotification**: New notification received

```typescript
connection.on('ReceiveNotification', (notification: Notification) => {
  showToast(notification.title, notification.message);
});
```

**LoadStatusChanged**: Load status updated

```typescript
connection.on('LoadStatusChanged', (loadId: string, status: string) => {
  updateLoadInList(loadId, status);
});
```

**NewLoadAssigned**: Driver assigned to a new load

```typescript
connection.on('NewLoadAssigned', (load: LoadDto) => {
  showAlert(`New load assigned: ${load.origin} → ${load.destination}`);
});
```

## Connection Management

### Automatic Reconnection

```typescript
const connection = new signalR.HubConnectionBuilder()
  .withUrl('/hubs/notification')
  .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
  .build();

connection.onreconnecting(error => {
  console.log('Reconnecting...', error);
});

connection.onreconnected(connectionId => {
  console.log('Reconnected:', connectionId);
});

connection.onclose(error => {
  console.log('Connection closed', error);
});
```

### Manual Reconnection

```typescript
async function startConnection() {
  try {
    await connection.start();
  } catch (err) {
    console.error('Connection failed, retrying...', err);
    setTimeout(startConnection, 5000);
  }
}
```

## Nginx Configuration

For SignalR through Nginx reverse proxy:

```nginx
location /hubs/ {
    proxy_pass http://127.0.0.1:7000;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection "upgrade";
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;

    # Important for long-lived connections
    proxy_read_timeout 86400;
    proxy_send_timeout 86400;
}
```

## Mobile Integration

### Kotlin (Android)

```kotlin
val hubConnection = HubConnectionBuilder
    .create("https://api.yourdomain.com/hubs/notification")
    .withAccessTokenProvider { getAccessToken() }
    .build()

hubConnection.on("ReceiveNotification", { notification: Notification ->
    showNotification(notification)
}, Notification::class.java)

hubConnection.start().blockingAwait()
```

## Troubleshooting

### Connection Fails

1. Check WebSocket support in browser/client
2. Verify Nginx proxy configuration
3. Check authentication token is valid

### Messages Not Received

1. Verify hub method names match exactly (case-sensitive)
2. Check group membership
3. Enable SignalR logging:

```typescript
const connection = new signalR.HubConnectionBuilder()
  .withUrl('/hubs/notification')
  .configureLogging(signalR.LogLevel.Debug)
  .build();
```

### Falls Back to Long Polling

1. Check WebSocket is allowed through firewall
2. Verify Nginx WebSocket headers
3. Check `proxy_read_timeout` is sufficient

## Next Steps

- [API Authentication](authentication.md) - Token handling
- [Deployment](../deployment/nginx-reverse-proxy.md) - Nginx configuration
