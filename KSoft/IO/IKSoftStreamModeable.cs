﻿using System;
using System.IO;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	/// <summary>Exposes data streaming state information and control</summary>
	[Contracts.ContractClass(typeof(IKSoftStreamModeableContract))]
	public interface IKSoftStreamModeable
	{
		/// <summary>Supported access permissions for the stream</summary>
		FileAccess StreamPermissions { get; }

		/// <summary>Current data streaming state</summary>
		/// <remarks>Read or Write, not both</remarks>
		FileAccess StreamMode { get; set; }
	};

	[Contracts.ContractClassFor(typeof(IKSoftStreamModeable))]
	abstract class IKSoftStreamModeableContract : IKSoftStreamModeable
	{
		public abstract FileAccess StreamPermissions { get; }

		public FileAccess StreamMode {
			get {
				Contract.Ensures(Contract.Result<FileAccess>() < FileAccess.ReadWrite, 
					"StreamMode was unset before use!");

				throw new NotImplementedException();
			}
			set {
				Contract.Requires<ArgumentOutOfRangeException>(value < FileAccess.ReadWrite);
				Contract.Requires<InvalidOperationException>((StreamPermissions & value) == value,
					"Stream doesn't support the requested access mode");

				throw new NotImplementedException();
			}
		}
	};
}