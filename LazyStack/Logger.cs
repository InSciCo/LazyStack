using System;

namespace LazyStack
{
    public interface ILogger
    {
        void Info(string message);
        void Error(Exception ex, string message);
    }
}
