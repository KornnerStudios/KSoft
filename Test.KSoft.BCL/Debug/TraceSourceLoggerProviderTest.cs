using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Debug.Test;

[TestClass]
public sealed class TraceSourceLoggerProviderTest
{
	[TestMethod]
	public void Logger_ResolvesSourceAfterLoggerCreation()
	{
		var registry = new TraceSourceRegistry();
		using var provider = new TraceSourceLoggerProvider(registry);
		ILogger logger = provider.CreateLogger("Late.Source.Child");

		Assert.ThrowsExactly<InvalidOperationException>(() => logger.IsEnabled(LogLevel.Information));

		var source = CreateSource("Late.Source", out RecordingTraceListener listener);
		try
		{
			registry.Register(source);
			logger.LogInformation("late registration");

			Assert.HasCount(1, listener.Events);
			Assert.AreEqual("late registration", listener.Events[0].Message);
		}
		finally
		{
			source.Close();
		}
	}

	[TestMethod]
	public void Logger_MapsLevelsAndPreservesNumericEventId()
	{
		var registry = new TraceSourceRegistry();
		var source = CreateSource("Mapped.Source", out RecordingTraceListener listener);
		try
		{
			registry.Register(source);
			using var provider = new TraceSourceLoggerProvider(registry);
			ILogger logger = provider.CreateLogger(source.Name);

			logger.Log(LogLevel.Critical, new EventId(1, "critical"), "critical", null, Format);
			logger.Log(LogLevel.Error, new EventId(2, "error"), "error", null, Format);
			logger.Log(LogLevel.Warning, new EventId(3, "warning"), "warning", null, Format);
			logger.Log(LogLevel.Information, new EventId(4, "information"), "information", null, Format);
			logger.Log(LogLevel.Debug, new EventId(5, "debug"), "debug", null, Format);
			logger.Log(LogLevel.Trace, new EventId(6, "trace"), "trace", null, Format);

			CollectionAssert.AreEqual(
				new[]
				{
					TraceEventType.Critical,
					TraceEventType.Error,
					TraceEventType.Warning,
					TraceEventType.Information,
					TraceEventType.Verbose,
					TraceEventType.Verbose,
				},
				listener.Events.ConvertAll(entry => entry.EventType));
			CollectionAssert.AreEqual(
				new[] { 1, 2, 3, 4, 5, 6 },
				listener.Events.ConvertAll(entry => entry.Id));
		}
		finally
		{
			source.Close();
		}
	}

	[TestMethod]
	public void Logger_UsesMessageOverloadWithoutInterpretingBraces()
	{
		var registry = new TraceSourceRegistry();
		var source = CreateSource("Brace.Source", out RecordingTraceListener listener);
		try
		{
			registry.Register(source);
			using var provider = new TraceSourceLoggerProvider(registry);
			ILogger logger = provider.CreateLogger(source.Name);

			logger.Log(LogLevel.Information, new EventId(7), "json { value }", null, Format);

			Assert.HasCount(1, listener.Events);
			Assert.AreEqual("json { value }", listener.Events[0].Message);
			Assert.IsNull(listener.Events[0].Data);
		}
		finally
		{
			source.Close();
		}
	}

	[TestMethod]
	public void Logger_UsesTraceDataForMessageAndException()
	{
		var registry = new TraceSourceRegistry();
		var source = CreateSource("Exception.Source", out RecordingTraceListener listener);
		try
		{
			registry.Register(source);
			using var provider = new TraceSourceLoggerProvider(registry);
			ILogger logger = provider.CreateLogger(source.Name);
			var exception = new InvalidOperationException("failure");

			logger.Log(LogLevel.Error, new EventId(8), "operation failed", exception, Format);

			Assert.HasCount(1, listener.Events);
			Assert.IsNull(listener.Events[0].Message);
			object?[] data = listener.Events[0].Data!;
			Assert.IsNotNull(data);
			Assert.AreEqual("operation failed", data[0]);
			Assert.AreSame(exception, data[1]);
		}
		finally
		{
			source.Close();
		}
	}

	[TestMethod]
	public void Logger_RespectsSwitchAndReturnsIdempotentNoOpScope()
	{
		var registry = new TraceSourceRegistry();
		var source = CreateSource("Filtered.Source", out RecordingTraceListener listener);
		try
		{
			source.Switch.Level = SourceLevels.Warning;
			registry.Register(source);
			using var provider = new TraceSourceLoggerProvider(registry);
			ILogger logger = provider.CreateLogger(source.Name);

			Assert.IsFalse(logger.IsEnabled(LogLevel.Information));
			Assert.IsTrue(logger.IsEnabled(LogLevel.Warning));
			logger.LogInformation("filtered");

			IDisposable scope = logger.BeginScope("scope")!;
			scope.Dispose();
			scope.Dispose();

			Assert.IsEmpty(listener.Events);
		}
		finally
		{
			source.Close();
		}
	}

	[TestMethod]
	public void Logger_RejectsUseAfterProviderDisposal()
	{
		var registry = new TraceSourceRegistry();
		var source = CreateSource("Disposed.Source", out _);
		try
		{
			registry.Register(source);
			var provider = new TraceSourceLoggerProvider(registry);
			ILogger logger = provider.CreateLogger(source.Name);

			provider.Dispose();

			Assert.ThrowsExactly<ObjectDisposedException>(() => logger.IsEnabled(LogLevel.Information));
			Assert.ThrowsExactly<ObjectDisposedException>(() =>
				logger.Log(LogLevel.Information, new EventId(1), "message", null, Format));
			Assert.ThrowsExactly<ObjectDisposedException>(() => provider.CreateLogger(source.Name));
		}
		finally
		{
			source.Close();
		}
	}

	private static string Format(string state, Exception? exception) => state;

	private static TraceSource CreateSource(string name, out RecordingTraceListener listener)
	{
		var source = new TraceSource(name, SourceLevels.All);
		source.Listeners.Clear();
		listener = new RecordingTraceListener();
		source.Listeners.Add(listener);
		return source;
	}

	private sealed class RecordingTraceListener : TraceListener
	{
		public List<RecordedEvent> Events { get; } = [];

		public override void Write(string? message)
		{
		}

		public override void WriteLine(string? message)
		{
		}

		public override void TraceEvent(
			TraceEventCache? eventCache,
			string source,
			TraceEventType eventType,
			int id,
			string? message)
		{
			Events.Add(new RecordedEvent(eventType, id, message, null));
		}

		public override void TraceData(
			TraceEventCache? eventCache,
			string source,
			TraceEventType eventType,
			int id,
			params object?[]? data)
		{
			Events.Add(new RecordedEvent(eventType, id, null, data));
		}
	}

	private sealed record RecordedEvent(
		TraceEventType EventType,
		int Id,
		string? Message,
		object?[]? Data);
}
