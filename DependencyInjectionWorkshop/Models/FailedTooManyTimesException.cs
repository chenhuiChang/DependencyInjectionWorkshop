using System;

namespace DependencyInjectionWorkshop.Models
{
    public class FailedTooManyTimesException : Exception
    {
        public FailedTooManyTimesException()
        {
        }

        public FailedTooManyTimesException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public string account { get; set; }
    }
}