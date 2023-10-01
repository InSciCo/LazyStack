using Amazon.Lambda.Core;

namespace LazyStackNotificationsFromStreams;

public class LazyStackNotificationFromStreams
{
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public string FunctionHandler(string input, ILambdaContext context)
    {
        return input.ToUpper();
    }

}