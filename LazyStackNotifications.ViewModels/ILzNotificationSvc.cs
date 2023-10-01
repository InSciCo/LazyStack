
namespace LazyStackNotifications.ViewModels;

public interface ILzNotificationSvc
{
    LzNotification? Notification { get; set; }
    ObservableCollection<string> Topics { get; }
    bool IsActive { get; } 
    Task ConnectAsync();
    Task DisconnectAsync();
    Task SendAsync(string message);
    Task<List<LzNotification>> ReadNotificationsAsync(string connectionId, long lastDateTimeTick);
    Task<(bool success, string msg)> SubscribeAsync(List<String> topicIds);
    Task<(bool success, string msg)> UnsubscribeAsync(List<String> topicIds);
    Task<(bool success, string msg)> UnsubscribeAllAsync();
}