using System;

namespace Shuttle.Core.Pipelines;

public class RecursiveException : Exception
{
    public RecursiveException(string message, Exception exception) : base(message, exception)
    {
    }
}