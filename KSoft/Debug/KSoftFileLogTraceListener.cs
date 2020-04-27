using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.Logging;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

// Duplicated types from Microsoft.VisualBasic.Logging to compile for .NET Core
namespace Microsoft.VisualBasic.Logging
{
	//
	// Summary:
	//     Determines what to do when the Microsoft.VisualBasic.Logging.FileLogTraceListener
	//     object attempts to write to a log and there is less free disk space available
	//     than specified by the Microsoft.VisualBasic.Logging.FileLogTraceListener.ReserveDiskSpace
	//     property.
	public enum DiskSpaceExhaustedOption
	{
		//
		// Summary:
		//     Throw an exception.
		ThrowException = 0,
		//
		// Summary:
		//     Discard log messages.
		DiscardMessages = 1
	};

	//
	// Summary:
	//     Determines which predefined path the Microsoft.VisualBasic.Logging.FileLogTraceListener
	//     class uses to write its log files.
	public enum LogFileLocation
	{
		//
		// Summary:
		//     Use the path of the current system's temporary folder.
		TempDirectory = 0,
		//
		// Summary:
		//     Use the path for a user's application data.
		LocalUserApplicationDirectory = 1,
		//
		// Summary:
		//     Use the path for the application data that is shared among all users.
		CommonApplicationDirectory = 2,
		//
		// Summary:
		//     Use the path for the executable file that started the application.
		ExecutableDirectory = 3,
		//
		// Summary:
		//     If the string specified by Microsoft.VisualBasic.Logging.FileLogTraceListener.CustomLocation
		//     is not empty, then use it as the path. Otherwise, use the path for a user's application
		//     data.
		Custom = 4
	};

	//
	// Summary:
	//     Determines which date to include in the names of the Microsoft.VisualBasic.Logging.FileLogTraceListener
	//     class log files.
	public enum LogFileCreationScheduleOption
	{
		//
		// Summary:
		//     Do not include the date in the log file name.
		None = 0,
		//
		// Summary:
		//     Include the current date in the log file name.
		Daily = 1,
		//
		// Summary:
		//     Include the first day of the current week in the log file name.
		Weekly = 2
	};
}

namespace KSoft.Debug
{
	// #NOTE: This is based on Microsoft.VisualBasic's FileLogTraceListener and TextWriterTraceListener

	public class KSoftFileLogTraceListener
		: TraceListener
	{
		static KSoftFileLogTraceListener()
		{
			var prop_names = Enum.GetNames(typeof(Property)).ToList();
			prop_names.Remove(Property.kNumberOf.ToString());

			mSupportedAttributes = prop_names.ToArray();
		}

		internal class ReferencedStream
			: IDisposable
		{
			private StreamWriter mStream;
			private readonly object mSyncObject;
			private int mReferenceCount;
			private bool mDisposed;

			internal bool IsInUse { get {
				return this.mStream != null;
			} }

			internal long FileSize { get {
				return this.mStream.BaseStream.Length;
			} }

			internal ReferencedStream(StreamWriter stream)
			{
				this.mReferenceCount = 0;
				this.mSyncObject = new object();
				this.mDisposed = false;
				this.mStream = stream;
			}

			internal void Write(string message)
			{
				object syncObject = this.mSyncObject;
				lock (syncObject)
				{
					this.mStream.Write(message);
				}
			}

			internal void WriteLine(string message)
			{
				object syncObject = this.mSyncObject;
				lock (syncObject)
				{
					this.mStream.WriteLine(message);
				}
			}

			internal void AddReference()
			{
				object syncObject = this.mSyncObject;
				checked
				{
					lock (syncObject)
					{
						this.mReferenceCount++;
					}
				}
			}

			internal void Flush()
			{
				object syncObject = this.mSyncObject;
				lock (syncObject)
				{
					this.mStream.Flush();
				}
			}

			internal void CloseStream()
			{
				object syncObject = this.mSyncObject;
				checked
				{
					lock (syncObject)
					{
						try
						{
							this.mReferenceCount--;
							this.mStream.Flush();
						}
						finally
						{
							if (this.mReferenceCount <= 0)
							{
								this.mStream.Close();
								this.mStream = null;
							}
						}
					}
				}
			}

			private void Dispose(bool disposing)
			{
				if (disposing && !this.mDisposed)
				{
					Util.DisposeAndNull(ref mStream);
					this.mDisposed = true;
				}
			}

			public void Dispose()
			{
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}

			~ReferencedStream()
			{
				this.Dispose(false);
			}
		};
		static Dictionary<string, ReferencedStream> mStreams = new Dictionary<string, ReferencedStream>();

		private enum Property
		{
			Append,
			AutoFlush,
			BaseFileName,
			CustomLocation,
			Delimiter,
			DiskSpaceExhaustedBehavior,
			Encoding,
			IncludeHostName,
			Location,
			LogFileCreationSchedule,
			MaxFileSize,
			ReservedDiskSpace,

			DoNotIncludeSourceName,
			DoNotIncludeEventType,
			DoNotIncludeEventId,
			OnlyWriteExceptionMessage,

			kNumberOf
		};
		static readonly string[] mSupportedAttributes;
		private Collections.BitVector32 mPropertiesSet;

		private bool GetPropertyIfNotSet(Property property, out string value)
		{
			value = null;
			if (mPropertiesSet.Test(property))
				return false;

			string property_name = property.ToString();
			if (!Attributes.ContainsKey(property_name))
				return false;

			value = Attributes[property_name];

			return true;
		}

		private bool mDoNotIncludeSourceName;
		public bool DoNotIncludeSourceName
		{
			get
			{
				if (GetPropertyIfNotSet(Property.DoNotIncludeSourceName, out string property))
				{
					this.DoNotIncludeSourceName = Convert.ToBoolean(property, CultureInfo.InvariantCulture);
				}
				return this.mDoNotIncludeSourceName;
			}
			set
			{
				this.mDoNotIncludeSourceName = value;
				this.mPropertiesSet.Set(Property.DoNotIncludeSourceName);
			}
		}

		private bool mDoNotIncludeEventType;
		public bool DoNotIncludeEventType
		{
			get
			{
				if (GetPropertyIfNotSet(Property.DoNotIncludeEventType, out string property))
				{
					this.DoNotIncludeEventType = Convert.ToBoolean(property, CultureInfo.InvariantCulture);
				}
				return this.mDoNotIncludeEventType;
			}
			set
			{
				this.mDoNotIncludeEventType = value;
				this.mPropertiesSet.Set(Property.DoNotIncludeEventType);
			}
		}

		private bool mDoNotIncludeEventId;
		public bool DoNotIncludeEventId
		{
			get
			{
				if (GetPropertyIfNotSet(Property.DoNotIncludeEventId, out string property))
				{
					this.DoNotIncludeEventId = Convert.ToBoolean(property, CultureInfo.InvariantCulture);
				}
				return this.mDoNotIncludeEventId;
			}
			set
			{
				this.mDoNotIncludeEventId = value;
				this.mPropertiesSet.Set(Property.DoNotIncludeEventId);
			}
		}

		private bool mOnlyWriteExceptionMessage;
		public bool OnlyWriteExceptionMessage
		{
			get
			{
				if (GetPropertyIfNotSet(Property.OnlyWriteExceptionMessage, out string property))
				{
					this.OnlyWriteExceptionMessage = Convert.ToBoolean(property, CultureInfo.InvariantCulture);
				}
				return this.mOnlyWriteExceptionMessage;
			}
			set
			{
				this.mOnlyWriteExceptionMessage = value;
				this.mPropertiesSet.Set(Property.OnlyWriteExceptionMessage);
			}
		}

		#region Original properties
		private bool mAppend;
		public bool Append
		{
			get
			{
				if (GetPropertyIfNotSet(Property.Append, out string property))
				{
					this.Append = Convert.ToBoolean(property, CultureInfo.InvariantCulture);
				}
				return this.mAppend;
			}
			set
			{
				this.DemandWritePermission();
				if (value != this.mAppend)
				{
					this.CloseCurrentStream();
				}
				this.mAppend = value;
				this.mPropertiesSet.Set(Property.Append);
			}
		}

		private bool mAutoFlush;
		public bool AutoFlush
		{
			get
			{
				if (GetPropertyIfNotSet(Property.AutoFlush, out string property))
				{
					this.AutoFlush = Convert.ToBoolean(property, CultureInfo.InvariantCulture);
				}
				return this.mAutoFlush;
			}
			[SecuritySafeCritical]
			set
			{
				this.DemandWritePermission();
				this.mAutoFlush = value;
				this.mPropertiesSet.Set(Property.AutoFlush);
			}
		}

		private string mBaseFileName;
		public string BaseFileName
		{
			get
			{
				if (GetPropertyIfNotSet(Property.BaseFileName, out string property))
				{
					this.BaseFileName = property;
				}
				return this.mBaseFileName;
			}
			set
			{
				Contract.Requires<ArgumentNullException>(value != null);

				Path.GetFullPath(value); // VB code did this...I guess to 'inherit' its exceptions
				if (string.Compare(value, this.mBaseFileName, StringComparison.OrdinalIgnoreCase) != 0)
				{
					this.CloseCurrentStream();
					this.mBaseFileName = value;
				}
				this.mPropertiesSet.Set(Property.BaseFileName);
			}
		}

		private string mCustomLocation;
		public string CustomLocation
		{
			[SecuritySafeCritical]
			get
			{
				if (GetPropertyIfNotSet(Property.CustomLocation, out string property))
				{
					this.CustomLocation = property;
				}
				string fullPath = Path.GetFullPath(this.mCustomLocation);
				new FileIOPermission(FileIOPermissionAccess.PathDiscovery, fullPath).Demand();
				return fullPath;
			}
			set
			{
				string fullPath = Path.GetFullPath(value);
				if (!Directory.Exists(fullPath))
				{
					Directory.CreateDirectory(fullPath);
				}
				if (this.Location == LogFileLocation.Custom & string.Compare(fullPath, this.mCustomLocation, StringComparison.OrdinalIgnoreCase) != 0)
				{
					this.CloseCurrentStream();
				}
				this.Location = LogFileLocation.Custom;
				this.mCustomLocation = fullPath;
				this.mPropertiesSet.Set(Property.CustomLocation);
			}
		}

		private string mDelimiter;
		public string Delimiter
		{
			get
			{
				if (GetPropertyIfNotSet(Property.Delimiter, out string property))
				{
					this.Delimiter = property;
				}
				return this.mDelimiter;
			}
			set
			{
				this.mDelimiter = value;
				this.mPropertiesSet.Set(Property.Delimiter);
			}
		}

		private DiskSpaceExhaustedOption mDiskSpaceExhaustedBehavior;
		public DiskSpaceExhaustedOption DiskSpaceExhaustedBehavior
		{
			get
			{
				if (GetPropertyIfNotSet(Property.DiskSpaceExhaustedBehavior, out string property))
				{
					var converter = TypeDescriptor.GetConverter(typeof(DiskSpaceExhaustedOption));
					this.DiskSpaceExhaustedBehavior = (DiskSpaceExhaustedOption)converter.ConvertFromInvariantString(property);
				}
				return this.mDiskSpaceExhaustedBehavior;
			}
			[SecuritySafeCritical]
			set
			{
				this.DemandWritePermission();
				ValidateDiskSpaceExhaustedOptionEnumValue(value, "value");
				this.mDiskSpaceExhaustedBehavior = value;
				this.mPropertiesSet.Set(Property.DiskSpaceExhaustedBehavior);
			}
		}

		private Encoding mEncoding;
		public Encoding Encoding
		{
			get
			{
				if (GetPropertyIfNotSet(Property.Encoding, out string property))
				{
					this.Encoding = Encoding.GetEncoding(property);
				}
				return this.mEncoding;
			}
			set
			{
				Contract.Requires<ArgumentNullException>(value != null);

				this.mEncoding = value;
				this.mPropertiesSet.Set(Property.Encoding);
			}
		}

		private bool mIncludeHostName;
		public bool IncludeHostName
		{
			get
			{
				if (GetPropertyIfNotSet(Property.IncludeHostName, out string property))
				{
					this.IncludeHostName = Convert.ToBoolean(property, CultureInfo.InvariantCulture);
				}
				return this.mIncludeHostName;
			}
			[SecuritySafeCritical]
			set
			{
				this.DemandWritePermission();
				this.mIncludeHostName = value;
				this.mPropertiesSet.Set(Property.IncludeHostName);
			}
		}

		private LogFileLocation mLocation;
		public LogFileLocation Location
		{
			get
			{
				if (GetPropertyIfNotSet(Property.Location, out string property))
				{
					TypeConverter converter = TypeDescriptor.GetConverter(typeof(LogFileLocation));
					this.Location = (LogFileLocation)converter.ConvertFromInvariantString(property);
				}
				return this.mLocation;
			}
			set
			{
				ValidateLogFileLocationEnumValue(value, nameof(value));
				if (this.mLocation != value)
				{
					this.CloseCurrentStream();
				}
				this.mLocation = value;
				this.mPropertiesSet.Set(Property.Location);
			}
		}

		private LogFileCreationScheduleOption mLogFileDateStamp;
		public LogFileCreationScheduleOption LogFileCreationSchedule
		{
			get
			{
				if (GetPropertyIfNotSet(Property.LogFileCreationSchedule, out string property))
				{
					var converter = TypeDescriptor.GetConverter(typeof(LogFileCreationScheduleOption));
					this.LogFileCreationSchedule = (LogFileCreationScheduleOption)converter.ConvertFromInvariantString(property);
				}
				return this.mLogFileDateStamp;
			}
			set
			{
				ValidateLogFileCreationScheduleOptionEnumValue(value, nameof(value));
				if (value != this.mLogFileDateStamp)
				{
					this.CloseCurrentStream();
					this.mLogFileDateStamp = value;
				}
				this.mPropertiesSet.Set(Property.LogFileCreationSchedule);
			}
		}

		private long mMaxFileSize;
		public long MaxFileSize
		{
			get
			{
				if (GetPropertyIfNotSet(Property.MaxFileSize, out string property))
				{
					this.MaxFileSize = Convert.ToInt64(property, CultureInfo.InvariantCulture);
				}
				return this.mMaxFileSize;
			}
			[SecuritySafeCritical]
			set
			{
				Contract.Requires<ArgumentOutOfRangeException>(value > 1000);

				this.DemandWritePermission();
				this.mMaxFileSize = value;
				this.mPropertiesSet.Set(Property.MaxFileSize);
			}
		}

		private long mReserveDiskSpace;
		public long ReserveDiskSpace
		{
			get
			{
				if (GetPropertyIfNotSet(Property.ReservedDiskSpace, out string property))
				{
					this.ReserveDiskSpace = Convert.ToInt64(property, CultureInfo.InvariantCulture);
				}
				return this.mReserveDiskSpace;
			}
			[SecuritySafeCritical]
			set
			{
				Contract.Requires<ArgumentOutOfRangeException>(value >= 0);

				this.DemandWritePermission();
				this.mReserveDiskSpace = value;
				this.mPropertiesSet.Set(Property.ReservedDiskSpace);
			}
		}
		#endregion

		private ReferencedStream mStream;
		private ReferencedStream ListenerStream
		{
			get
			{
				this.EnsureStreamIsOpen();
				return this.mStream;
			}
		}

		private readonly DateTime mDay;
		private int mDays;
		private DateTime mFirstDayOfWeek;

		[HostProtection(SecurityAction.LinkDemand, Resources = HostProtectionResource.ExternalProcessMgmt)]
		public KSoftFileLogTraceListener(string name) : base(name)
		{
			this.mLocation = LogFileLocation.LocalUserApplicationDirectory;
			this.mAutoFlush = false;
			this.mAppend = true;
			this.mIncludeHostName = false;
			this.mDiskSpaceExhaustedBehavior = DiskSpaceExhaustedOption.DiscardMessages;
			this.mBaseFileName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
			this.mLogFileDateStamp = LogFileCreationScheduleOption.None;
			this.mMaxFileSize = 5000000L;
			this.mReserveDiskSpace = 10000000L;
			this.mDelimiter = "\t";
			this.mEncoding = Encoding.UTF8;
			this.mCustomLocation = Application.UserAppDataPath;
			this.mDay = DateTime.Now.Date;
			this.mDays = 0;
			this.mFirstDayOfWeek = GetFirstDayOfWeek(mDay);
			this.mPropertiesSet = new Collections.BitVector32();
		}

		[HostProtection(SecurityAction.LinkDemand, Resources = HostProtectionResource.ExternalProcessMgmt)]
		public KSoftFileLogTraceListener() : this(nameof(KSoftFileLogTraceListener))
		{
		}

		private string mHostName;
		private string HostName
		{
			get
			{
				if (mHostName == null)
				{
					this.mHostName = Environment.MachineName;
				}
				return this.mHostName;
			}
		}

		private string mFullFileName;
		public string FullLogFileName
		{
			[SecuritySafeCritical]
			get
			{
				this.EnsureStreamIsOpen();
				string fullFileName = this.mFullFileName;
				new FileIOPermission(FileIOPermissionAccess.PathDiscovery, fullFileName).Demand();
				return fullFileName;
			}
		}

		private string LogFileName { get {
			string path;
			switch (this.Location)
			{
			case LogFileLocation.TempDirectory:
				path = Path.GetTempPath();
				break;
			case LogFileLocation.LocalUserApplicationDirectory:
				path = Application.UserAppDataPath;
				break;
			case LogFileLocation.CommonApplicationDirectory:
				path = Application.CommonAppDataPath;
				break;
			case LogFileLocation.ExecutableDirectory:
				path = Path.GetDirectoryName(Application.ExecutablePath);
				break;
			case LogFileLocation.Custom:
				if (this.CustomLocation.IsNullOrEmpty())
				{
					path = Application.UserAppDataPath;
				}
				else
				{
					path = this.CustomLocation;
				}
				break;
			default:
				path = Application.UserAppDataPath;
				break;
			}
			string text = this.BaseFileName;
			switch (this.LogFileCreationSchedule)
			{
			case LogFileCreationScheduleOption.Daily:
				text = text + "-" + DateTime.Now.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
				break;
			case LogFileCreationScheduleOption.Weekly:
				this.mFirstDayOfWeek = DateTime.Now.AddDays(checked(DayOfWeek.Sunday - DateTime.Now.DayOfWeek));
				text = text + "-" + this.mFirstDayOfWeek.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
				break;
			}
			return Path.Combine(path, text);
		} }

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
		protected override string[] GetSupportedAttributes()
		{
			return mSupportedAttributes;
		}

		#region TemporaryBuffer
		private StringBuilder mTemporaryBuffer = new StringBuilder();
		private void TemporaryWriteIndent()
		{
			NeedIndent = false;
			for (int i = 0; i < IndentLevel; i++)
			{
				if (IndentSize == 4)
					TemporaryWrite("    ");
				else
				{
					for (int j = 0; j < IndentSize; j++)
					{
						TemporaryWrite(" ");
					}
				}
			}
		}
		private void TemporaryWriteFlush()
		{
			if (mTemporaryBuffer.Length == 0)
				return;

			Write(mTemporaryBuffer.ToString());
			mTemporaryBuffer.Clear();
		}
		private void TemporaryWrite(string message)
		{
			if (NeedIndent)
				TemporaryWriteIndent();
			mTemporaryBuffer.Append(message);
		}
		private void TemporaryWriteLine(string message)
		{
			if (NeedIndent)
				TemporaryWriteIndent();
			mTemporaryBuffer.AppendLine(message);
			NeedIndent = true;
		}
		#endregion

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
		public override void Write(string message)
		{
			try
			{
				this.HandleDateChange();
				long newEntrySize = this.Encoding.GetByteCount(message);
				if (this.ResourcesAvailable(newEntrySize))
				{
					this.ListenerStream.Write(message);
					if (this.AutoFlush)
					{
						this.ListenerStream.Flush();
					}
				}
			}
			catch (Exception ex)
			{
				ex.UnusedExceptionVar();
				this.CloseCurrentStream();
				throw;
			}
		}

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
		public override void WriteLine(string message)
		{
			try
			{
				this.HandleDateChange();
				long newEntrySize = this.Encoding.GetByteCount(message + Environment.NewLine);
				if (this.ResourcesAvailable(newEntrySize))
				{
					this.ListenerStream.WriteLine(message);
					if (this.AutoFlush)
					{
						this.ListenerStream.Flush();
					}
				}
			}
			catch (Exception ex)
			{
				ex.UnusedExceptionVar();
				this.CloseCurrentStream();
				throw;
			}
		}

		#region Write helpers
		private void WriteHeader(String source, TraceEventType eventType, int id)
		{
			var sb = new StringBuilder();

			if (!DoNotIncludeSourceName)
			{
				sb.Append(source);
				sb.Append(" ");
			}
			if (!DoNotIncludeEventType)
			{
				sb.Append(eventType.ToString());
			}
			if (sb.Length > 0)
			{
				sb.Append(": ");
			}
			if (!DoNotIncludeEventId)
			{
				sb.Append(id.ToString(CultureInfo.InvariantCulture));
				sb.Append(" : ");
			}

			if (sb.Length > 0)
				Write(sb.ToString());
		}

		private void WriteFooter(TraceEventCache eventCache)
		{
			if (eventCache == null)
				return;

			IndentLevel++;
			if (IsEnabled(TraceOptions.ProcessId))
				TemporaryWriteLine("ProcessId=" + eventCache.ProcessId);

			if (IsEnabled(TraceOptions.LogicalOperationStack))
			{
				TemporaryWrite("LogicalOperationStack=");
				var operationStack = eventCache.LogicalOperationStack;
				bool first = true;
				foreach (var obj in operationStack)
				{
					if (!first)
						Write(", ");
					else
						first = false;

					TemporaryWrite(obj.ToString());
				}
				TemporaryWriteLine(string.Empty);
			}

			if (IsEnabled(TraceOptions.ThreadId))
				TemporaryWriteLine("ThreadId=" + eventCache.ThreadId);

			if (IsEnabled(TraceOptions.DateTime))
				TemporaryWriteLine("DateTime=" + eventCache.DateTime.ToString("o", CultureInfo.InvariantCulture));

			if (IsEnabled(TraceOptions.Timestamp))
				TemporaryWriteLine("Timestamp=" + eventCache.Timestamp);

			if (IsEnabled(TraceOptions.Callstack))
				TemporaryWriteLine("Callstack=" + eventCache.Callstack);

			IndentLevel--;

			TemporaryWriteFlush();
		}

		private void WriteData(object data)
		{
			if (data is AggregateException)
			{
				var ex = data as AggregateException;
				//TemporaryWrite("Exceptions: ");
				TemporaryWriteLine(ex.Message);
				IndentLevel++;

				if (!OnlyWriteExceptionMessage)
				{
					var list = ex.GetKSoftStackTraceList();
					foreach (var line in list)
					{
						TemporaryWriteLine(line);
					}
				}

				IndentLevel++;
				foreach (var inner in ex.InnerExceptions)
				{
					WriteData(inner);
				}
				IndentLevel--;

				IndentLevel--;
			}
			else if (data is Exception)
			{
				var ex = data as Exception;
				//TemporaryWrite("Exception: ");
				TemporaryWriteLine(ex.Message);
				IndentLevel++;

				if (!OnlyWriteExceptionMessage)
				{
					var list = ex.GetKSoftStackTraceList();
					foreach (var line in list)
					{
						TemporaryWriteLine(line);
					}
				}

				var inner = ex.InnerException;
				if (inner != null)
				{
					if (inner is AggregateException)
					{
						TemporaryWrite("InnerException: ");
						WriteData(inner);
					}
					else
					{
						TemporaryWriteLine("InnerException:");

						IndentLevel++;
						WriteData(inner);
						IndentLevel--;
					}
				}

				IndentLevel--;
			}
			else
			{
				TemporaryWriteLine(data.ToString());
			}
		}
		#endregion

		bool IsEnabled(TraceOptions opts)
		{
			return (opts & TraceOutputOptions) != 0;
		}

		#region Trace methods
		[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
		{
			if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, message, null, null, null))
				return;

#if false
			var sb = new StringBuilder();
			if (!DoNotIncludeSourceName)
			{
				sb.Append(source);
				sb.Append(Delimiter);
			}
			if (!DoNotIncludeEventType)
			{
				sb.Append(Enum.GetName(typeof(TraceEventType), eventType));
				sb.Append(Delimiter);
			}
			if (!DoNotIncludeEventId)
			{
				sb.Append(id.ToString(CultureInfo.InvariantCulture));
				sb.Append(Delimiter);
			}

			sb.Append(message);

			if (IsEnabled(TraceOptions.DateTime))
			{
				sb.Append(Delimiter);
				sb.Append(eventCache.DateTime.ToString("u", CultureInfo.InvariantCulture));
			}
			if (IsEnabled(TraceOptions.ProcessId))
			{
				sb.Append(Delimiter);
				sb.Append(eventCache.ProcessId.ToString(CultureInfo.InvariantCulture));
			}
			if (IsEnabled(TraceOptions.ThreadId))
			{
				sb.Append(Delimiter);
				sb.Append(eventCache.ThreadId);
			}
			if (IsEnabled(TraceOptions.Timestamp))
			{
				sb.Append(Delimiter);
				sb.Append(eventCache.Timestamp.ToString(CultureInfo.InvariantCulture));
			}
			if (IsEnabled(TraceOptions.Callstack))
			{
				sb.Append(Delimiter);
				sb.Append(eventCache.Callstack);
			}
			if (IsEnabled(TraceOptions.LogicalOperationStack))
			{
				sb.Append(Delimiter);
				sb.Append(StackToString(eventCache.LogicalOperationStack));
			}
			if (IncludeHostName)
			{
				sb.Append(Delimiter);
				sb.Append(this.HostName);
			}
			this.WriteLine(sb.ToString());
#else
			WriteHeader(source, eventType, id);
			WriteLine(message);
			WriteFooter(eventCache);
#endif
		}

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
		{
			if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, format, args, null, null))
				return;

#if false
			string message;
			if (args != null)
			{
				message = string.Format(CultureInfo.InvariantCulture, format, args);
			}
			else
			{
				message = format;
			}
			this.TraceEvent(eventCache, source, eventType, id, message);
#else
			WriteHeader(source, eventType, id);

			if (args != null)
				WriteLine(string.Format(CultureInfo.InvariantCulture, format, args));
			else
				WriteLine(format);

			WriteFooter(eventCache);
#endif
		}

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
		{
			if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null))
				return;

#if false
			string message = "";
			var actual_data = GetDataEntryForTrace(data);
			if (actual_data != null)
			{
				message = actual_data.ToString();
			}
			this.TraceEvent(eventCache, source, eventType, id, message);
#else
			WriteHeader(source, eventType, id);
			string datastring = "";
			var actual_data = GetDataEntryForTrace(data);
			if (actual_data != null)
				datastring = actual_data.ToString();

			WriteLine(datastring);
			WriteFooter(eventCache);
#endif
		}

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
		{
			if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data))
				return;

#if false
			var sb = new StringBuilder();

			var actual_data = GetDataForTrace(data);
			if (actual_data != null)
			{
				int num = data.Length - 1;
				int num2 = num;
				for (int i = 0; i <= num2; i++)
				{
					sb.Append(data[i].ToString());
					if (i != num)
					{
						sb.Append(this.Delimiter);
					}
				}
			}

			this.TraceEvent(eventCache, source, eventType, id, sb.ToString());
#else
			WriteHeader(source, eventType, id);

			var actual_data = GetDataForTrace(data);
			if (actual_data != null)
			{
				IndentLevel++;
				for (int i = 0; i < actual_data.Length; i++)
				{
					if (actual_data[i] != null)
						WriteData(actual_data[i]);
				}
				IndentLevel--;

				TemporaryWriteFlush();
			}

			WriteFooter(eventCache);
#endif
		}
		#endregion

		private object[] GetDataForTrace(params object[] args)
		{
			if (args.IsNullOrEmpty())
				return args;

			var result = new object[args.Length];

			for (int x = 0; x < result.Length; x++)
			{
				var arg = args[x];
				result[x] = GetDataEntryForTrace(arg);
			}

			return result;
		}

		private object GetDataEntryForTrace(object data)
		{
			if (data == null)
				return data;

			var result = data;
			if (result is Exception)
			{
			}
			else
			{
				result = result.ToString();
			}
			return result;
		}

		#region Stream stuff
		[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
		public override void Flush()
		{
			if (this.mStream != null)
			{
				this.mStream.Flush();
			}
		}

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
		public override void Close()
		{
			this.Dispose(true);
		}
		[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.CloseCurrentStream();
			}
		}

		private void EnsureStreamIsOpen()
		{
			if (this.mStream == null)
			{
				this.mStream = this.GetStream();
			}
		}

		[SecuritySafeCritical]
		private ReferencedStream GetStream()
		{
			int num = 0;
			ReferencedStream referencedStream = null;
			string fullPath = Path.GetFullPath(this.LogFileName + ".log");
			checked
			{
				while (referencedStream == null && num < 2147483647)
				{
					string fullPath2;
					if (num == 0)
					{
						fullPath2 = Path.GetFullPath(this.LogFileName + ".log");
					}
					else
					{
						fullPath2 = Path.GetFullPath(this.LogFileName + "-" + num.ToString(CultureInfo.InvariantCulture) + ".log");
					}
					string key = fullPath2.ToUpper(CultureInfo.InvariantCulture);
					object streams = mStreams;
					lock (streams)
					{
						if (mStreams.ContainsKey(key))
						{
							referencedStream = mStreams[key];
							if (!referencedStream.IsInUse)
							{
								mStreams.Remove(key);
								referencedStream = null;
							}
							else
							{
								if (this.Append)
								{
									new FileIOPermission(FileIOPermissionAccess.Write, fullPath2).Demand();
									referencedStream.AddReference();
									this.mFullFileName = fullPath2;
									ReferencedStream result = referencedStream;
									return result;
								}
								num++;
								referencedStream = null;
								continue;
							}
						}
						Encoding encoding = this.Encoding;
						try
						{
							if (this.Append)
							{
								encoding = GetFileEncoding(fullPath2);
								if (encoding == null)
								{
									encoding = this.Encoding;
								}
							}
							var stream = new StreamWriter(fullPath2, this.Append, encoding);
							referencedStream = new ReferencedStream(stream);
							referencedStream.AddReference();
							mStreams.Add(key, referencedStream);
							this.mFullFileName = fullPath2;
							ReferencedStream result = referencedStream;
							return result;
						}
						catch (IOException var_10_148)
						{
							var_10_148.UnusedExceptionVar();
						}
						num++;
					}
				}

				throw new Exception("ExhaustedPossibleStreamNames: " + fullPath);
			}
		}

		private void CloseCurrentStream()
		{
			if (this.mStream != null)
			{
				lock (mStreams)
				{
					if (this.mStream != null)
					{
						this.mStream.CloseStream();
						if (!this.mStream.IsInUse)
						{
							mStreams.Remove(this.mFullFileName.ToUpper(CultureInfo.InvariantCulture));
						}
						this.mStream = null;
					}
				}
			}
		}

		private bool ResourcesAvailable(long newEntrySize)
		{
			checked
			{
				bool result;
				if (this.ListenerStream.FileSize + newEntrySize > this.MaxFileSize)
				{
					if (this.DiskSpaceExhaustedBehavior == DiskSpaceExhaustedOption.ThrowException)
					{
						throw new InvalidOperationException("FileExceedsMaximumSize");
					}
					result = false;
				}
				else if (this.GetFreeDiskSpace() - newEntrySize < this.ReserveDiskSpace)
				{
					if (this.DiskSpaceExhaustedBehavior == DiskSpaceExhaustedOption.ThrowException)
					{
						throw new InvalidOperationException("ReservedSpaceEncroached");
					}
					result = false;
				}
				else
				{
					result = true;
				}
				return result;
			}
		}

		[SecuritySafeCritical]
		private long GetFreeDiskSpace()
		{
			string pathRoot = Path.GetPathRoot(Path.GetFullPath(this.FullLogFileName));
			new FileIOPermission(FileIOPermissionAccess.PathDiscovery, pathRoot).Demand();
			if (GetDiskFreeSpaceEx(pathRoot, out long num, out long num2, out long num3) && num > -1L)
			{
				return num;
			}
			throw new Exception("ApplicationLog_FreeSpaceError");
		}
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
			out long lpFreeBytesAvailable,
			out long lpTotalNumberOfBytes,
			out long lpTotalNumberOfFreeBytes);

		[SecurityCritical]
		private void DemandWritePermission()
		{
			string directoryName = Path.GetDirectoryName(this.LogFileName);
			new FileIOPermission(FileIOPermissionAccess.Write, directoryName).Demand();
		}

		private Encoding GetFileEncoding(string fileName)
		{
			Encoding result;
			if (File.Exists(fileName))
			{
				StreamReader streamReader = null;
				try
				{
					streamReader = new StreamReader(fileName, this.Encoding, true);
					if (streamReader.BaseStream.Length > 0L)
					{
						streamReader.ReadLine();
						result = streamReader.CurrentEncoding;
						return result;
					}
				}
				finally
				{
					if (streamReader != null)
					{
						streamReader.Close();
					}
				}
			}
			result = null;
			return result;
		}
		#endregion

		#region Date stuff
		private bool DayChanged()
		{
			return DateTime.Compare(this.mDay.AddDays((double)this.mDays), DateTime.Now.Date) != 0;
		}

		private bool WeekChanged()
		{
			return DateTime.Compare(this.mFirstDayOfWeek.Date, GetFirstDayOfWeek(DateTime.Now.Date)) != 0;
		}

		private static DateTime GetFirstDayOfWeek(DateTime checkDate)
		{
			return checkDate.AddDays(checked(DayOfWeek.Sunday - checkDate.DayOfWeek)).Date;
		}

		private void HandleDateChange()
		{
			if (this.LogFileCreationSchedule == LogFileCreationScheduleOption.Daily)
			{
				if (this.DayChanged())
				{
					this.mDays = DateTime.Now.Date.Subtract(this.mDay).Days;
					this.CloseCurrentStream();
					return;
				}
			}
			else if (this.LogFileCreationSchedule == LogFileCreationScheduleOption.Weekly && this.WeekChanged())
			{
				this.CloseCurrentStream();
			}
		}
		#endregion

		static void ValidateLogFileLocationEnumValue(LogFileLocation value, string paramName)
		{
			if (value < LogFileLocation.TempDirectory || value > LogFileLocation.Custom)
			{
				throw new InvalidEnumArgumentException(paramName, (int)value, typeof(LogFileLocation));
			}
		}

		static void ValidateDiskSpaceExhaustedOptionEnumValue(DiskSpaceExhaustedOption value, string paramName)
		{
			if (value < DiskSpaceExhaustedOption.ThrowException || value > DiskSpaceExhaustedOption.DiscardMessages)
			{
				throw new InvalidEnumArgumentException(paramName, (int)value, typeof(DiskSpaceExhaustedOption));
			}
		}

		static void ValidateLogFileCreationScheduleOptionEnumValue(LogFileCreationScheduleOption value, string paramName)
		{
			if (value < LogFileCreationScheduleOption.None || value > LogFileCreationScheduleOption.Weekly)
			{
				throw new InvalidEnumArgumentException(paramName, (int)value, typeof(LogFileCreationScheduleOption));
			}
		}

		static string StackToString(System.Collections.Stack stack)
		{
			const string STACK_DELIMITER = ", ";

			int length = STACK_DELIMITER.Length;
			var stringBuilder = new StringBuilder();
			foreach (object obj in stack)
			{
				stringBuilder.Append(obj);
				stringBuilder.Append(STACK_DELIMITER);
			}

			stringBuilder.Replace("\"", "\"\"");
			if (stringBuilder.Length >= length)
			{
				stringBuilder.Remove(checked(stringBuilder.Length - length), length);
			}
			return "\"" + stringBuilder + "\"";
		}
	};
}
