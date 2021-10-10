using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;
using FlexDashWebSocket.Models;

// http://localhost:3000/?ws=ws://localhost:5291/ws

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseWebSockets();

WebSocket webSocket = null;
System.Timers.Timer timer = new System.Timers.Timer(1000);

timer.Elapsed += FireEvent;
timer.AutoReset = true;
// kick off timer to send a message every second
timer.Enabled = true;

app.Use(async (context, next) => 
{
if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await ProcessRequest(context);
            //await Echo(context, webSocket);
        }
        else
        {
            context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
        }
    }
    else
    {
        await next();
    }
});

app.Run();

// main websocket method that processes messages received over the socket
async Task ProcessRequest(HttpContext context)
{
    var buffer = new byte[1024 * 4];
    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    while (!result.CloseStatus.HasValue)
    {
        var message = await ReadMessage(result, buffer);
        Console.WriteLine(JsonSerializer.Serialize(message));

        switch (message.Topic)
        {
            case "$ctrl":
                await ProcessControlMessages(message);
                break;
        }

        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    }
    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
}

// this clearly needs to be built out more
// need to process dashboard messages when new widgets, tabs, grids get added
async Task ProcessControlMessages(Message message)
{
    if (message.Payload.Deserialize<string>() == "start")
    {
        await InitializeDashboard();
    }
}

// this method is just fired on a timer job so we can see how to push values to the ui
// this should be replaced with real values coming from the pi
void FireEvent(Object source, ElapsedEventArgs args)
{
    var dateTime = DateTime.Now;
    var message = new Message();

    if (webSocket != null && webSocket.State == WebSocketState.Open)
    {
        message.Topic = "clock/hour";
        message.Payload = JsonSerializer.SerializeToElement(dateTime.Hour);
        SendMessage(message).Wait();

        message.Topic = "clock/minute";
        message.Payload = JsonSerializer.SerializeToElement(dateTime.Minute);
        SendMessage(message).Wait();

        message.Topic = "clock/second";
        message.Payload = JsonSerializer.SerializeToElement(dateTime.Second);
        SendMessage(message).Wait();
    }
}

// this method really should pull the info from a saved config file instead of being hard coded
async Task InitializeDashboard()
{
    var messageToSend = new Message();

    messageToSend.Topic = "$config/dash";
    var dashboard = new Dashboard { Title = "StokesTest", Tabs = new List<string>().ToArray() };
    messageToSend.Payload = JsonSerializer.SerializeToElement(dashboard);
    await SendMessage(messageToSend);

    messageToSend.Topic = "$config/widgets/w0001";
    Widget widget = new Widget() { Kind = "Stat", Id = "w0001",
        Static = new() { Title = "Hour", Unit = ""},
        Dynamic = new() { Value = "clock/hour"},
        Rows = 1, Cols = 1 };
    messageToSend.Payload = JsonSerializer.SerializeToElement(widget);
    await SendMessage(messageToSend);

    messageToSend.Topic = "$config/widgets/w0002";
    widget = new Widget() { Kind = "Stat", Id = "w0002",
        Static = new() { Title = "Minute", Unit = ""},
        Dynamic = new() { Value = "clock/minute"},
        Rows = 1, Cols = 1 };
    messageToSend.Payload = JsonSerializer.SerializeToElement(widget);
    await SendMessage(messageToSend);

    messageToSend.Topic = "$config/widgets/w0003";
    widget = new Widget() { Kind = "Stat", Id = "w0003",
        Static = new() { Title = "Second", Unit = ""},
        Dynamic = new() { Value = "clock/second"},
        Rows = 1, Cols = 1 };
    messageToSend.Payload = JsonSerializer.SerializeToElement(widget);
    await SendMessage(messageToSend);

    messageToSend.Topic = "$config/grids/g0001";
    var grid = new Grid() { Kind = "FixedGrid", Id = "g0001", Title = "Clock", Widgets = new List<string>() {"w0001", "w0002", "w0003"}.ToArray() };
    messageToSend.Payload = JsonSerializer.SerializeToElement(grid);
    await SendMessage(messageToSend);

    messageToSend.Topic = "$config/tabs/t0001";
    var tabs = new Tab() { Id = "t0001", Title = "Clock", Icon = "resistor-nodes", Grids = new List<string>() {"g0001"}.ToArray() };
    messageToSend.Payload = JsonSerializer.SerializeToElement(tabs);
    await SendMessage(messageToSend);

    messageToSend.Topic = "$config/dash/tabs";
    messageToSend.Payload = JsonSerializer.SerializeToElement(new List<string> {"t0001"}.ToArray());
    await SendMessage(messageToSend);
}

async Task<Message> ReadMessage(WebSocketReceiveResult result, byte[] buffer)
{
    StringBuilder builder = new StringBuilder();

    if (result.EndOfMessage)
    {
        // read the payload and call it good, happy day
        var tempString = Encoding.UTF8.GetString(buffer, 0, result.Count);
        builder.Append(tempString);
    }
    else
    {
        // need to read the full payload
        var tempString = Encoding.UTF8.GetString(buffer, 0, result.Count);
        builder.Append(tempString);
        do
        {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            tempString = Encoding.UTF8.GetString(buffer, 0, result.Count);
            builder.Append(tempString);
        } while (!result.EndOfMessage);
    }

    var message = JsonSerializer.Deserialize<Message>(builder.ToString());

    return message;
}
async Task SendMessage(Message message)
{
    var messageToSend = JsonSerializer.Serialize(message);
    var bytesToSend = Encoding.UTF8.GetBytes(messageToSend);
    await webSocket.SendAsync(new ArraySegment<byte>(bytesToSend, 0, bytesToSend.Length), WebSocketMessageType.Text, true, CancellationToken.None);
}

// this is just here for debugging in case we run into issues, hopefully won't be needed
async Task Echo(HttpContext context)
{
    var buffer = new byte[1024 * 4];
    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    while (!result.CloseStatus.HasValue)
    {
        var message = await ReadMessage(result, buffer);
        Console.WriteLine(JsonSerializer.Serialize(message));

        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    }
    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
}