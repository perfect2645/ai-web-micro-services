// See https://aka.ms/new-console-template for more information
using SignalRTest;

Console.WriteLine("Hello, World!");

// ===================== 配置项（替换成你的实际地址）=====================
var hubUrl = "https://localhost:7094/signalRHub"; // 服务端Hub地址
var clientName = "测试客户端（.NET 10）";

// 1. 实例化封装的SignalR客户端
await using var signalRClient = new SignalRClientWrapper(hubUrl);

// 2. 订阅消息接收事件（打印收到的消息）
signalRClient.OnMessageReceived += (user, message) =>
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 收到消息 | {user}：{message}");

// 3. 订阅连接状态变化事件（打印状态）
signalRClient.OnStateChanged += (oldState, newState) =>
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 连接状态 | {oldState} → {newState}");

try
{
    // 4. 连接到SignalR服务端
    Console.WriteLine($"正在连接到SignalR Hub：{hubUrl}");
    await signalRClient.ConnectAsync();
    Console.WriteLine("✅ SignalR连接成功！");

    // 5. 循环接收用户输入，发送消息
    Console.WriteLine("\n=====================");
    Console.WriteLine("输入消息内容（输入exit退出）：");
    Console.WriteLine("=====================\n");

    string? input;
    while ((input = Console.ReadLine()) != null)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            break;
        }

        // 发送消息
        await signalRClient.SendMessageAsync(clientName, input);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 消息已发送：{input}");
    }
}
catch (Exception ex)
{
    // 异常捕获（定位连接/发送失败原因）
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"❌ 操作失败：{ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"   内部原因：{ex.InnerException.Message}");
    }
    Console.ResetColor();
}
finally
{
    // 6. 断开连接
    await signalRClient.DisconnectAsync();
    Console.WriteLine("\n🔌 SignalR连接已断开");
}

Console.WriteLine("\n按任意键退出...");
Console.ReadKey();