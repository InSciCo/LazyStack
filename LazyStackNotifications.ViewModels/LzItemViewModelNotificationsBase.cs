using LazyStackNotificationsSchema;

namespace LazyStackNotifications.ViewModels;

/// <summary>
/// ItemViewModelNotificationsBase<T,TEdit>
/// This class is the base class for all ItemViewModels that will use Notifications.
/// </summary>
/// <typeparam name="TDTO">DTO Type</typeparam>
/// <typeparam name="TModel">Model Type (extended model off of TDTO)</typeparam>
public abstract class LzItemViewModelNotificationsBase<TDTO, TModel> : LzItemViewModelBase<TDTO,TModel>, ILzItemViewModelNotificationsBase<TModel>
    where TDTO : class, new()
    where TModel : class, TDTO, IRegisterObservables, new()
{
    public LzItemViewModelNotificationsBase(TDTO item, bool? isLoaded = null) : base(item, isLoaded)
    {
        this.WhenAnyValue(x => x.NotificationsSvc!.Notification!)
            .WhereNotNull()
            .Where(x => x.PayloadId.Equals(Id))
            .Subscribe(async (x) => await UpdateFromNotification(x.Payload, x.PayloadAction, x.CreateUtcTick));
    }
    public ILzNotificationSvc? NotificationsSvc { get; set; }
    public INotificationEditOption NotificationEditOption { get; set; }
    [Reactive] public TModel? NotificationData { get; set; }
    [Reactive] public bool NotificationReceived { get; set; }
    [Reactive] public long LastNotificationTick { get; set; }
    [Reactive] public bool IsMerge { get; set; }

    public virtual async Task UpdateFromNotification(string payloadData, string payloadAction, long createdUtcTick)
    {
        /*
            LastNotificationTick holds the datetime of the last read or notification processed 
            payloadCreatedAt holds the datetime of the update time of the payload contained in the notificiation 
            dataUpdatedAt is the updated datetime of the current data 
        */

        if (State != LzItemViewModelBaseState.Edit && State != LzItemViewModelBaseState.Current)
            return;

        if (createdUtcTick <= LastNotificationTick)
        {
            Console.WriteLine("skipping: createdUtcTick <= LastNotificationsTick");
            return;
        }

        LastNotificationTick = createdUtcTick;

        try
        {
            var dataObj = JsonConvert.DeserializeObject<TDTO>(payloadData);
            if (dataObj == null)
            {
                Console.WriteLine("dataObj is null");
                return;
            }

            if(State == LzItemViewModelBaseState.Current)
            {
                if (payloadAction.Equals("Delete"))
                {
                    Console.WriteLine("State == Current && Action == Delete - not handled");
                    return; // this action is handled at the ItemsViewModel level
                }
                UpdateData(dataObj);
                LastNotificationTick = UpdatedAt;
                NotificationReceived = true; // Fires off event in case we want to inform the user an update occurred
                Console.WriteLine("Data object updated from dataObj");
                return;
            }

            if(State == LzItemViewModelBaseState.Edit)
            {
                if(payloadAction.Equals("Delete"))
                {
                    Console.WriteLine("State == Edit && Action == Delete");
                    await CancelEditAsync();
                    return; // The actual delete is handled at the ItemsViewModel level
                }

                switch(NotificationEditOption)
                {
                    case INotificationEditOption.Cancel:
                        Console.WriteLine("State == Edit && Action == Cancel");
                        await CancelEditAsync();
                        UpdateData(dataObj);
                        LastNotificationTick = UpdatedAt;
                        await OpenEditAsync();
                        break;

                    case INotificationEditOption.Merge:
                        Console.WriteLine("State == Edit && Action == Merge");
                        UpdateData(dataObj);
                        LastNotificationTick = UpdatedAt;
                        IsMerge = true;
                        break;
                    default:
                        return;

                }
            }
        } catch 
        { 
            // Todo: What, if anything, do we want to do here? Maybe just log?
        }
        finally
        {
            UpdateCount++;
        }
    }
    
    public override async Task<(bool, string)> ReadAsync(string id, StorageAPI storageAPI = StorageAPI.Default)
    {
        var (success, msg) = await base.ReadAsync(id, storageAPI);
        if(success)
            LastNotificationTick = UpdatedAt;
        return (success, msg);
    }
    public override async Task<(bool, string)> UpdateAsync(string? id, StorageAPI storageAPI = StorageAPI.Default)
    {
        var (success, msg) = await base.UpdateAsync(id, storageAPI);
        if (success)
            LastNotificationTick = UpdatedAt;
        return (success, msg);
    }
    public override async Task<(bool, string)> SaveEditAsync(string? id, StorageAPI storageAPI = StorageAPI.Default)
    {
        var (success, msg) = await base.SaveEditAsync(id, storageAPI);
        if (success)
            IsMerge = false;
        return (success, msg);
    }
    public override (bool, string) CancelEdit()
    {

        if (State != LzItemViewModelBaseState.Edit && State != LzItemViewModelBaseState.New)
            return (false, Log(MethodBase.GetCurrentMethod()!, "No Active Edit"));

        State = (IsLoaded) ? LzItemViewModelBaseState.Current : LzItemViewModelBaseState.New;

        if (IsMerge)
            UpdateData(NotificationData!);
        else
            RestoreFromDataCopy();
        IsMerge = false;
        return (true, String.Empty);
    }
}
