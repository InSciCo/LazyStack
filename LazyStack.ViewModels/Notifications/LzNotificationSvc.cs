
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;

namespace LazyStack.ViewModels;
/// <summary>
/// Derive this class and 
/// - Implement ReadNotifications() // even if you are using WebSockets
/// 
/// Notifications using WebSocket. 
///     
/// ClientWebSocket limitations:
/// When used in a browser, .NET ClientWebSocket does not allow us to 
/// attach an authorization header when communicating to the WebSocket 
/// Service. For this reason, we do unauthenticated websocket connections.
/// Note that subscriptions use the REST API and usually subject to authentication.
/// Since we use subscriptions to send messages to websocket connections, no 
/// messages will be sent to websocket connections that have not had subsequent 
/// subscriptions. So, in a nutshell, websocket connections are not authenticated
/// but subscription channels usually are. Also, we are using wss:// so the websocket
/// connection transport is secure.
/// 
/// Clientside Usage Notes
/// After signing in, use the ClientSDK.SubscribeAsync(Subscrpition subscription) to subscribe to 
/// one or more topics.
/// 
/// Flow:
/// - Singleton injection, should happen before authentication
/// - Set UsePolling = true before authentication if you want to avoid attempt to establish web socket connection
/// - 
/// 
/// 
/// </summary>
public abstract class LzNotificationSvc : LzViewModelBase, ILzNotificationSvc, IDisposable
{
    public LzNotificationSvc(
        ILzClientConfig clientConfig,
        IAuthProcess authProces,
        IInternetConnectivitySvc internetConnectivity)
    {
        this.clientConfig = clientConfig;
        this.authProcess = authProces;
        this.internetConnectivity = internetConnectivity;

        this.WhenAnyValue(x => x.internetConnectivity.IsOnline, x => x.authProcess.IsSignedIn, (x, y) => x && y )
            .Throttle(TimeSpan.FromMilliseconds(100))
            .DistinctUntilChanged()
            .Subscribe(async x =>
            {
                _ = EnsureConnectedAsync();


            });

    }

    protected ILzClientConfig clientConfig;
    protected IAuthProcess authProcess;
    protected IInternetConnectivitySvc internetConnectivity;    
    protected string? wsBaseUri; 
    protected Timer? timer;
    protected ClientWebSocket? ws;
    protected string connectionId = string.Empty;
    
    protected long lastDateTimeTicks = 0;
    public ObservableCollection<string> Topics { get; set; } = new();
    [ObservableAsProperty] public bool IsActive { get; }
    // This class implements INotifiyPropertyChanged so events are produced 
    // when Notification is assigned. 
    private LzNotification? _notification;
    public LzNotification? Notification 
    {
        get { return _notification; }
        set { this.RaiseAndSetIfChanged(ref _notification, value); }
    }
    protected bool isBusy = false;

    /// <summary>
    /// You must implement this method in the derived class.
    /// Typically, you will have extended your REST API to 
    /// have endpoints supporting Notificaitons using the 
    /// Service solution's NotificationsSvc.yaml and have
    /// appropriate Notification methods in the ClientSDK.
    /// Note: The method should have the 'async' modifier.
    /// </summary>
    /// <param name="lastDateTimeTick"></param>
    /// <returns></returns>
    /// Example Implementation:
    public abstract Task<List<LzNotification>> ReadNotificationsAsync(string connectionId, long lastDateTimeTick);
    public abstract Task<(bool success, string msg)> SubscribeAsync(List<string> topicIds);
    public abstract Task<(bool success, string msg)> UnsubscribeAsync(List<string> topicIds);
    public abstract Task<(bool success, string msg)> UnsubscribeAllAsync();
    
    private PropertyInfo? createdAtPropertyInfo;
    protected string createdAtFieldName = "CreatedAt";

    public async Task ConnectAsync()
    {
        Console.WriteLine("NotificationSvc.ConnetcAsync()");
        await EnsureConnectedAsync();
    }

    private async Task EnsureConnectedAsync()
    {
        ws ??= new ClientWebSocket();

        Console.WriteLine($"EnsureConnectedAsync. WebSocketState={ws.State}");
        var runConfigService = clientConfig?.RunConfig?.Service;
        var service = clientConfig?.Services[runConfigService!];
        const string notificationsSvc = "NotificationsWebSocketAPI";

        if (ws.State == WebSocketState.Open)
            return;

        if(ws.State != WebSocketState.None)
            Console.WriteLine($"ConnectAsync failed. State={ws.State}");

        if (service != null && service.Resources.ContainsKey(notificationsSvc))
        {
            JObject resource = service.Resources[notificationsSvc];
            wsBaseUri = (string)resource["Url"]!;
            var uri = new Uri(wsBaseUri);   
            try
            {
                Console.WriteLine("Calling ws.ConnectAsync");
                await ws.ConnectAsync(uri, CancellationToken.None);
                await ListenForMessages();
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());   
            }
        }
    }

    private async Task ListenForMessages()
    {
        Console.WriteLine("Listening for web socket messages");
        var buffer = new byte[1024];
        while (ws.State == WebSocketState.Open)
        {
            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            Console.WriteLine($"Received message. type:{result.MessageType}");
            switch(result.MessageType)
            {
                case WebSocketMessageType.Close:
                    Console.WriteLine("WebSocket Close");
                    break;
                case WebSocketMessageType.Text:
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"WebSocket Text message. {message}");
                    break;
                case WebSocketMessageType.Binary:
                    Console.WriteLine($"WebSocket Binary message.");
                    break;
            }
        }
    }

    public async Task SendAsync(string message)
    {
        if(ws is null)
        {
            Console.WriteLine($"WebScoket SendAsync failed. WebSocketClient is null");
            return;
        }

        if (ws.State != WebSocketState.Open)
        {
            Console.WriteLine($"WebScoket SendAsync failed. State={ws.State}");
            return;
        }    

        message = "{\"action\": \"message\", \"content\": \"{message}\"}";

        var bytes = Encoding.UTF8.GetBytes(message);

        await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

        Console.WriteLine($"WebScoket SendAsync done. State={ws.State}");

    }

    public async Task DisconnectAsync()
    {
        if (ws is null) return;

        if (ws.State == WebSocketState.Open)
        {
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            Console.WriteLine($"DisconnectAsync called. ws.State={ws.State.ToString()}");
        }
        ws = null;  
    }

    public override void Dispose()
    {
        base.Dispose(); 
    }
}


