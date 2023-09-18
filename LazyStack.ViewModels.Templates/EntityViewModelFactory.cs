namespace $namespace$;


public interface I$entity$ViewModelFactory
{
    $entity$ViewModel Create();
}

public class $entity$ViewModelFactory : I$entity$ViewModelFactory, ILzViewModelFactory
{
    private IAppSvc appSvc;
    
    public $entity$ViewModelFactory(IAppSvc appSvc)
    {
        this.appSvc = appSvc;
    }

    public $entity$ViewModel Create()
    {
        return new $entity$ViewModel()
        {
            AppSvc = appSvc
        };
    }
}