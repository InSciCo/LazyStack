﻿using System;
using System.Threading.Tasks;

namespace LazyStack
{
    public interface ILogger
    {
        void Info(string message);
        Task InfoAsync(string message);
        void Error(Exception ex, string message);
        Task ErrorAsync(Exception ex, string message);
    }
}
