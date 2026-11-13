using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace BackendCarcass.Application.Tests.TestInfrastructure;

//ლოგის ჩანაწერებს იმახსოვრებს, რომ ტესტმა შეამოწმოს, რა ჩაიწერებოდა ლოგში
public sealed class CapturingLogger<T> : ILogger<T>
{
    public List<CapturedLogEntry> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        string message = formatter(state, exception);
        //სტრუქტურული მნიშვნელობებიც ინახება: Serilog მათ ცალკე წერს, ტექსტში რომ არ ჩანდეს, მაშინაც
        string properties = state is IEnumerable<KeyValuePair<string, object?>> pairs
            ? string.Join(Environment.NewLine,
                pairs.Select(p => p.Key + "=" + Convert.ToString(p.Value, CultureInfo.InvariantCulture)))
            : string.Empty;
        Entries.Add(new CapturedLogEntry(logLevel, message,
            string.Join(Environment.NewLine, message, properties, exception?.ToString())));
    }
}

//FullText არის ყველაფერი, რასაც ლოგი მიიღებდა: ტექსტი, სტრუქტურული მნიშვნელობები და გამონაკლისი
public sealed record CapturedLogEntry(LogLevel Level, string Message, string FullText);
