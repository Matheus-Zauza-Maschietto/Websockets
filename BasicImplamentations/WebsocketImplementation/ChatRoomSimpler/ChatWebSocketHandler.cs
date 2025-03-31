using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace ChatRoomSimpler;


public static class ChatWebSocketHandler
{
    private static readonly List<WebSocket> _sockets = new();

    public static async Task HandleClient(HttpContext context, WebSocket socket)
    {
        _sockets.Add(socket);

        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        
        while (!result.CloseStatus.HasValue)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var msgBytes = Encoding.UTF8.GetBytes($"User{socket.GetHashCode()}: {message}");

            foreach (var s in _sockets.Where(s => s.State == WebSocketState.Open))
            {
                await s.SendAsync(new ArraySegment<byte>(msgBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }

            result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        _sockets.Remove(socket);
        await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }

    public static async Task EfficientHandleClient(HttpContext context, WebSocket socket)
    {
        _sockets.Add(socket);

        var buffer = new byte[1024 * 4];

        while (!socket.CloseStatus.HasValue)
        {
            WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (socket.CloseStatus.HasValue) {
                await DisposeSocket(socket);
                return;
            };
            
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            SocketMessage socketMessage = JsonSerializer.Deserialize<SocketMessage>(message);
            switch (socketMessage.Event)
            {
                case "message":
                    SocketMessage sendMessage = new() { Event = "message", Data = $"User{socket.GetHashCode()}: {socketMessage.Data}" };
                    var msgBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sendMessage));
                    foreach (var s in _sockets.Where(s => s.State == WebSocketState.Open))
                    {
                        await s.SendAsync(new ArraySegment<byte>(msgBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    break;

                case "writing":
                    SocketMessage sendWriting = new() { Event = "writing", Data = $"User{socket.GetHashCode()} writing" };
                    var msgBytesWriting = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sendWriting));
                    foreach (var s in _sockets.Where(s => s.State == WebSocketState.Open && s != socket))
                    {
                        await s.SendAsync(new ArraySegment<byte>(msgBytesWriting), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    break;
            }

        }
        _sockets.Remove(socket);
        await socket.CloseAsync(socket.CloseStatus.Value, socket.CloseStatusDescription, CancellationToken.None);
    }

    private static async Task DisposeSocket(WebSocket socket)
    {
        _sockets.Remove(socket);
        await socket.CloseAsync(socket.CloseStatus.Value, socket.CloseStatusDescription, CancellationToken.None);
    }
}
