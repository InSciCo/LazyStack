namespace LazyStack.ViewModels;

public interface IItemViewModelNotificationsBase : ILzItemViewModelBase
{
    public abstract ILzNotificationSvc NotificationsSvc { get; init; }
    public Task UpdateFromNotification(string payloadData, string payloadAction, long payloadCreatedAt);
   
}
