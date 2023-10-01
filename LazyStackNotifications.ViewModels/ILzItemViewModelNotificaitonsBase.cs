using LazyStackNotificationsSchema;

namespace LazyStackNotifications.ViewModels;

public interface IItemViewModelNotificationsBase : ILzItemViewModelBase
{
    public abstract ILzNotificationSvc NotificationsSvc { get; init; }
    public INotificationEditOption NotificationEditOption { get; set; }
    public Task UpdateFromNotification(string payloadData, string payloadAction, long payloadCreatedAt);
   
}
