using System;
namespace LazyStackDynamoDBRepo
{
    /// <summary>
    /// Extend HttpStatus code 5xx for database errors
    /// Be aware that some proxys can screw around with status codes.
    /// Have fallback logic that works if any of these codes get
    /// changed to 500. 
    /// </summary>
    /*
    public enum DBTransError
    {
        NewerLastUpdateFound = 550,
        KeyNotFound = 551,
        KeyAlreadyExists = 552,
        BadKey = 553,
        BadConsumerID = 554,
        RemoteServiceDown = 555,
        DBError = 556
    }
    */
}