namespace LazyStack.ViewModels;

public interface ILzItemViewModelBaseData<TModel>
{
    public TModel? Data { get; set; }
    public TModel? NotificationData { get; set; }
}
