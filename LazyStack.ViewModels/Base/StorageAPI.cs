namespace LazyStack.ViewModels;

public enum StorageAPI
{
    Default, // Usually Rest API
    Rest, // API calls, generally requires auth
    S3, // bucket access, generally requireds auth
    Http, // limited to gets
    Local, // local device storage
    Content, // _content access
    Internal // class implementation handles persistence (if any). Use for updating data in place.
}
