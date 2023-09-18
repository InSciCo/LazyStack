using Microsoft.VisualBasic;
using System.Data;

namespace LazyStack.ViewModels;

/// <summary>
/// Wraps the Notification type defined in the client library SDK and Schema projects
/// so we have a type that conforms to the INotification interface required by 
/// the generic notification handlers in the LazyStack.ViewModels library.
/// Note: You may be wondering why we don't just pass the Notification record into 
/// a constructor. We can't do this because we use the Notification record a type 
/// in the LazyStack.ViewModels.NotificationSvc class which takes NotificationWrapper 
/// as a generics type argument.
/// </summary>
public class LzNotification : ILzNotification
{
    const string msg = "Notification not assigned";
    public object Notification 
    { 
        get => notification!; 
        init
        {
            notification = (ILzNotification?)value;
        }
    }   
    private ILzNotification? notification;


    public string Id => notification?.Id ?? throw new InvalidOperationException(msg);
    public string TopicId => notification?.TopicId ?? throw new InvalidOperationException(msg);
    public string PayloadParentId => notification?.PayloadParentId ?? throw new InvalidOperationException(msg);
    public string PayloadType => notification?.PayloadType ?? throw new InvalidOperationException(msg);
    public string PayloadId => notification?.PayloadId ?? throw new InvalidOperationException(msg);
    public string Payload => notification?.Payload ?? throw new InvalidOperationException(msg);
    public string PayloadAction => notification?.PayloadAction ?? throw new InvalidOperationException(msg);  
    public long CreatedAt => notification?.CreatedAt ?? throw new InvalidOperationException(msg);    

}
