namespace LazyStackCognitoValidation
{
    public interface ITokenValidator
    {
        Task<bool> ValidateTokenHttpAsync(string? token);
    }
}