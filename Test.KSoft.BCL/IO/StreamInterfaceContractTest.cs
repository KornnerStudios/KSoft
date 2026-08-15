using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.IO.Test
{
	[TestClass]
	public sealed class StreamInterfaceContractTest : BaseTestClass
	{
		sealed class VirtualBufferContract : KSoft.IO.IKSoftStreamWithVirtualBufferContract
		{
			public override Stream BaseStream => Stream.Null;
		}

		sealed class ModeableContract : KSoft.IO.IKSoftStreamModeableContract
		{
			readonly FileAccess mStreamPermissions;

			public ModeableContract(FileAccess streamPermissions)
			{
				mStreamPermissions = streamPermissions;
			}

			public override FileAccess StreamPermissions => mStreamPermissions;
		}

		static void AssertThrowsArgumentOutOfRange(Action action, string paramName)
		{
			var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);

			Assert.AreEqual(paramName, exception.ParamName);
		}

		[TestMethod]
		public void IO_VirtualBufferContractSettersRejectNegativeValuesTest()
		{
			var contract = new VirtualBufferContract();

			AssertThrowsArgumentOutOfRange(() => contract.VirtualBufferStart = -1, "value");
			AssertThrowsArgumentOutOfRange(() => contract.VirtualBufferLength = -1, "value");
		}

		[TestMethod]
		public void IO_StreamModeableContractSetterRejectsReadWriteModeTest()
		{
			var contract = new ModeableContract(FileAccess.ReadWrite);

			AssertThrowsArgumentOutOfRange(() => contract.StreamMode = FileAccess.ReadWrite, "value");
			AssertThrowsArgumentOutOfRange(() => contract.StreamMode = (FileAccess)4, "value");
		}

		[TestMethod]
		public void IO_StreamModeableContractSetterRejectsUnsupportedPermissionsTest()
		{
			var contract = new ModeableContract(FileAccess.Read);
			var exception = Assert.ThrowsExactly<InvalidOperationException>(() => contract.StreamMode = FileAccess.Write);

			Assert.AreEqual("Stream doesn't support the requested access mode", exception.Message);
		}
	};
}
