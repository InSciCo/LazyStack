using System.Collections.ObjectModel;

namespace LazyStackNotificationsClient;

public interface INotificationSvc<T>
{
    int DueTime { get; set; }
    T? Notification { get; set; }
    int Period { get; set; }
    bool PollingActive { get; set; }
    ObservableCollection<string> Topics { get; set; }

    void StartPollingAt(long dateTimeTicks);
    Task ConnectAsync();
    Task DisconnectAsync();
}