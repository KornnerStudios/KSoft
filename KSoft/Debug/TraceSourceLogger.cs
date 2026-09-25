using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace KSoft.Debug;

/// <remarks>
/// Existing listeners emit formatted text and exceptions; structured state and <see cref="EventId.Name"/>
/// are not preserved. Scopes are accepted but are not rendered by the current trace configuration.
/// </remarks>
internal class TraceSourceLogger : ILogger
{
	private readonly TraceSourceLoggerProvider mProvider;
	private readonly string mCategoryName;

	public TraceSourceLogger(TraceSourceLoggerProvider provider, string categoryName)
	{
		ArgumentNullException.ThrowIfNull(provider);
		ArgumentException.ThrowIfNullOrWhiteSpace(categoryName);

		mProvider = provider;
		mCategoryName = categoryName;
	}

	public IDisposable BeginScope<TState>(TState state) where TState : notnull
		=> NullTraceScope.Instance;

	public bool IsEnabled(LogLevel logLevel)
	{
		if (logLevel == LogLevel.None)
			return false;

		TraceEventType eventType = ToTraceEventType(logLevel);
		return mProvider.Resolve(mCategoryName).Switch.ShouldTrace(eventType);
	}

	public void Log<TState>(
		LogLevel logLevel,
		EventId eventId,
		TState state,
		Exception? exception,
		Func<TState, Exception?, string> formatter)
	{
		ArgumentNullException.ThrowIfNull(formatter);

		if (logLevel == LogLevel.None)
			return;

		TraceEventType eventType = ToTraceEventType(logLevel);
		TraceSource source = mProvider.Resolve(mCategoryName);
		if (!source.Switch.ShouldTrace(eventType))
			return;

		string message = formatter(state, exception) ?? string.Empty;
		if (exception != null)
			source.TraceData(eventType, eventId.Id, message, exception);
		else if (message.Length > 0)
			source.TraceEvent(eventType, eventId.Id, message);
	}

	private static TraceEventType ToTraceEventType(LogLevel logLevel)
	{
		return logLevel switch
		{
			LogLevel.Critical => TraceEventType.Critical,
			LogLevel.Error => TraceEventType.Error,
			LogLevel.Warning => TraceEventType.Warning,
			LogLevel.Information => TraceEventType.Information,
			LogLevel.Debug => TraceEventType.Verbose,
			LogLevel.Trace => TraceEventType.Verbose,
			_ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, "Unsupported log level."),
		};
	}
}

internal sealed class TraceSourceLogger<T> : TraceSourceLogger, ILogger<T>
{
	public TraceSourceLogger(TraceSourceLoggerProvider provider)
		: base(provider, typeof(T).FullName ?? typeof(T).Name)
	{
	}
}
