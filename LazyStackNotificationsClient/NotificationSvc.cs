using System.Reflection;

namespace LazyStackNotificationsClient;
/// <summary>
/// Notifications using Polling or WebSocket. 
/// WebSocket is preferred. Polling is a fallback.
/// Your constructor must assign SvcReadNotifications.
/// Optionally, you can assign:
///     createdAtFieldName, default is 'CreatedAt'
/// </summary>
public partial class NotificationSvc<T> : LzViewModelBase, INotificationSvc<T>
    where T : class, new()
{
    public NotificationSvc(
        ILzClientConfig clientConfig,
        ILzHttpClient client,
        IAuthProcess authProces)
    {
        this.clientConfig = clientConfig;
        this.authProcess = authProces;
        _httpClient = client;

        Period = 3000;

        this.WhenAnyValue(x => x.PollingActive, x => x.Period, x => x.Topics.Count, x => x.authProcess.IsSignedIn)
            .Throttle(TimeSpan.FromMilliseconds(100)) // avoid multiple create/deletes 
            .Subscribe(x =>
            {
                timer?.Dispose();

                if (!PollingActive || Topics.Count == 0 || authProces.IsNotSignedIn)
                    return;

                timer = new System.Threading.Timer(callback: Poll, state: null, dueTime: DueTime, period: Period);
            });
    }

    private ILzHttpClient _httpClient;

    private ILzClientConfig clientConfig;
    private IAuthProcess authProcess;
    private string? wsBaseUri;
    private Timer? timer;
    private long lastDateTimeTicks = 0;
    public ObservableCollection<string> Topics { get; set; } = new();
    public int DueTime { get; set; } = 0; // time before first callback
    [Reactive] public int Period { get; set; } // periods between callbacks
    [Reactive] public bool PollingActive { get; set; }
    private T? _notification;
    public T? Notification {
        get { return _notification; } 
        set { 
            this.RaiseAndSetIfChanged(ref _notification, value); 
        }
    }
    private ClientWebSocket? clientWebSocket;

    private bool isBusy = false;
    public void StartPollingAt(long dateTimeTicks = 0)
    {
        if(dateTimeTicks == 0)
            dateTimeTicks = DateTime.UtcNow.Ticks;
        lastDateTimeTicks = dateTimeTicks;
        PollingActive = true;
    }

    private PropertyInfo? createdAtPropertyInfo;
    protected Func<string, long, Task<ICollection<T>>>? SvcReadNotifications { get; set; }
    protected string createdAtFieldName = "CreatedAt";

    // Poll is an event handler so "async void" is used instead of async Task.
    private async void Poll(object? state)
    {
        createdAtPropertyInfo ??= typeof(T).GetProperty(createdAtFieldName);
        if (createdAtPropertyInfo is null)
            throw new Exception($"Could not find '{createdAtFieldName}' property in Notifications type.");

        if (SvcReadNotifications is null)
            throw new Exception("SvcReadNotificaitons not assigned");

        if (authProcess.IsNotSignedIn || PollingActive == false || Topics.Count == 0 || isBusy)
            return;
        string msg = "Polling Error.";  
        try
        {
            isBusy = true;

            // We use reflection to get CreatedAt value from Notificaitons record. Since we are 
            // only grabbing a single field from the record, this is not especially slow. 
            var notificationsInOrder = new Dictionary<long,T>();
            foreach (var topic in Topics)
            {
                msg = $"";
                var notifications = await SvcReadNotifications(topic, lastDateTimeTicks);
                if (notifications is not null && notifications.Count > 0)
                    foreach (var instance in notifications)
                        try
                        {
                            notificationsInOrder.Add((long)createdAtPropertyInfo!.GetValue(instance)!, instance);
                        } 
                        catch (ArgumentException) 
                        { 
                            // Note: I don't believe this is possible for expected use case where all Notifications 
                            // are in the same table and have the same PK.
                            msg = $"Multiple Notifications with same CreatedAt time encountered for topic: {topic}"; 
                            throw new Exception(msg);
                        }
            }

            if (notificationsInOrder.Count == 0)
                return;
            
            foreach (var notification in notificationsInOrder)
            {
                lastDateTimeTicks = ((notification.Key) + 1);
                Notification = notification.Value; // Assignment generates event
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + msg);
        }
        finally { isBusy = false; }
    }

    public async Task ConnectAsync()
    {
        await EnsureConnectedAsync();
    }

    private async Task EnsureConnectedAsync()
    {

        var runConfigService = clientConfig?.RunConfig?.Service;
        if (runConfigService != null)
        {
            var service = clientConfig?.Services[runConfigService];
            if (service!.Resources.ContainsKey("Notifications"))
            {
                JObject resource = service.Resources["Notifications"];
                wsBaseUri = (string)resource["Url"]!;
                clientWebSocket = new ClientWebSocket();
            }
        }
        else
        {
            if (!PollingActive)
                StartPollingAt();
            return;
        }

        if (clientWebSocket?.State == WebSocketState.Open) 
        {
            return;
        }

        if (clientWebSocket?.State != WebSocketState.None && clientWebSocket?.State != WebSocketState.Closed)
        {
            // Ideally, you should close it gracefully
            await clientWebSocket!.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }

        try
        {
            clientWebSocket = new ClientWebSocket();
            var connectUri = new Uri(wsBaseUri!);
            await clientWebSocket.ConnectAsync(connectUri, CancellationToken.None);
        } catch (Exception ex) 
        { 
            Console.WriteLine(ex.Message);
        }

    }

    public async Task DisconnectAsync()
    {
        if (PollingActive || clientWebSocket is null)
            return;

        if (clientWebSocket.State == WebSocketState.Open)
        {

            await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
            
    }
}


