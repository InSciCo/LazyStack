
namespace LazyStack.ViewModels;

public interface ILzNotification
{
    public string Id { get; }  
    public string TopicId { get; }
    public string PayloadParentId { get; } 
    public string PayloadId { get;  }
    public string PayloadType { get;  } 
    public string Payload { get; }
    public string PayloadAction { get;  }   
    public long CreatedAt { get; } 

}
