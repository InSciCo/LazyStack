namespace LazyStack.ViewModels;

/// <summary>
/// ItemViewModelBase<T,TEdit>
/// Remember to call base() constructor to get subscriptions and state set properly.
/// </summary>
/// <typeparam name="TDTO">DTO Type</typeparam>
/// <typeparam name="TModel">Model Type (extended model off of TDTO)</typeparam>
public abstract class LzItemViewModelNotificationsBase<TDTO, TModel> : LzItemViewModelBase<TDTO,TModel>, IItemViewModelNotificationsBase
    where TDTO : class, new()
    where TModel : class, TDTO, IRegisterObservables, new()
{
    public LzItemViewModelNotificationsBase(TDTO item, bool? isLoaded = null) : base(item, isLoaded)
    {
        if(NotificationsSvc != null)
        {
            this.WhenAnyValue(x => x.NotificationsSvc!.Notification!)
                .WhereNotNull()
                .Where(x => x.PayloadId.Equals(Id))
                .Subscribe(async (x) => await UpdateFromNotification(x.Payload, x.PayloadAction, x.CreatedAt));
        }
    }
    public abstract ILzNotificationSvc NotificationsSvc { get; init; }

    public virtual async Task UpdateFromNotification(string payloadData, string payloadAction, long payloadCreatedAt)
    {
        /*
            LastNotificationTick holds the datetime of the last read or notification processed 
            payloadCreatedAt holds the datetime of the update time of the payload contained in the notificiation 
            dataUpdatedAt is the updated datetime of the current data 
        */

        if (State != LzItemViewModelBaseState.Edit && State != LzItemViewModelBaseState.Current)
            return;

        if (payloadCreatedAt <= LastNotificationTick)
        {
            Console.WriteLine("skipping: payLoadCreatedAt <= LastNotificationsTick");
            return;
        }

        LastNotificationTick = payloadCreatedAt;

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

}
