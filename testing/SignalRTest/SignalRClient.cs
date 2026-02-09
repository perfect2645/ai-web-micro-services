using Microsoft.AspNetCore.SignalR.Client;

namespace SignalRTest
{
    /// <summary>
    /// .NET 10 SignalR客户端封装类
    /// </summary>
    public class SignalRClientWrapper : IAsyncDisposable
    {
        // SignalR连接对象
        private readonly HubConnection _hubConnection;
        // 接收消息事件（供外部订阅）
        public event Action<string, string>? OnMessageReceived;
        // 连接状态变化事件（供外部订阅）
        public event Action<HubConnectionState, HubConnectionState>? OnStateChanged;

        /// <summary>
        /// 构造函数：初始化SignalR连接
        /// </summary>
        /// <param name="hubUrl">SignalR Hub地址（需和服务端MapHub路径一致）</param>
        public SignalRClientWrapper(string hubUrl)
        {
            // 构建Hub连接
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    // 开发环境忽略HTTPS证书错误（生产环境删除）
                    options.HttpMessageHandlerFactory = _ => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                    };
                    // 支持WebSocket+长轮询（兼容更多场景）
                    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets
                                         | Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
                    // 连接超时（可选）
                    options.CloseTimeout = TimeSpan.FromSeconds(10);
                })
                // 自动重连（0秒、2秒、5秒重试）
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5) })
                .Build();

            // 注册接收服务端消息的方法，转发到外部事件
            _hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
                OnMessageReceived?.Invoke(user, message));
        }

        /// <summary>
        /// 连接到SignalR服务端
        /// </summary>
        public async Task ConnectAsync()
        {
            if (_hubConnection.State != HubConnectionState.Connected)
            {
                await _hubConnection.StartAsync();
            }
        }

        /// <summary>
        /// 发送消息到SignalR服务端
        /// </summary>
        /// <param name="user">发送者名称</param>
        /// <param name="message">消息内容</param>
        public async Task SendMessageAsync(string user, string message)
        {
            if (_hubConnection.State != HubConnectionState.Connected)
            {
                throw new InvalidOperationException("SignalR未连接，无法发送消息");
            }
            // 调用服务端Hub的SendMessage方法（需和服务端方法名一致）
            await _hubConnection.SendAsync("SendMessage", user, message);
        }

        /// <summary>
        /// 断开SignalR连接
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.StopAsync();
            }
        }

        /// <summary>
        /// 释放资源（异步释放）
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync();
            await _hubConnection.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}
