using System;
using Reflect = System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Reflection.Test
{
	using MessageBoxDelegateGeneric = Func<IntPtr, string, string, uint, int>;

	[TestClass]
	public partial class UtilitiesTest : BaseTestClass
	{
		delegate int MessageBoxDelegate(IntPtr hWnd, string lpText, string lpCaption, uint uType);
#if false
		[TestMethod]
		public void ReflectUtil_GetDelegateForFunctionPointerTest()
		{
			var module = LowLevel.Windows.LoadLibrary("user32.dll");
			// Can't use MessageBoxW, seems to implicitly marshal to UTF8? Could have sworn .NET strings were internally UTF16
			// Then again, my system is setup for en-us...
			var proc_ptr = LowLevel.Windows.GetProcAddress(module, "MessageBoxA");
			const System.Runtime.InteropServices.CallingConvention call_conv = 
				System.Runtime.InteropServices.CallingConvention.Winapi;

			var msg_box = Util.GetDelegateForFunctionPointer<MessageBoxDelegate>(proc_ptr, call_conv);
			msg_box(IntPtr.Zero, "Hello World", "Test1", 0);

			var msg_box_gen = Util.GetDelegateForFunctionPointer<MessageBoxDelegateGeneric>(proc_ptr, call_conv);
			msg_box_gen(IntPtr.Zero, "Goodbye World", "Test2", 0);

			LowLevel.Windows.FreeLibrary(module);
		}
#endif

		#region Set private property
		class PropertySetPrivateClass
		{
			public const string kInitialValue = "Can't touch this!";
			public const string kModifiedValue = "Rape!";

			public string Value { get; private set; }

			public PropertySetPrivateClass()
			{
				Value = kInitialValue;
			}
		};
		[TestMethod]
		public void Reflection_PropertySetPrivateViaLinqTest()
		{
			var c = new PropertySetPrivateClass();
			var value_setter = Util.GenerateReferenceTypeMemberSetter<PropertySetPrivateClass, string>("Value");

			value_setter(c, PropertySetPrivateClass.kModifiedValue);
			Assert.AreEqual(c.Value, PropertySetPrivateClass.kModifiedValue);
		}
		[TestMethod]
		public void Reflection_PropertySetPrivateViaReflectionTest()
		{
			var c = new PropertySetPrivateClass();
			var value_prop = Util.PropertyFromExpr(() => c.Value);

			value_prop.SetValue(c, PropertySetPrivateClass.kModifiedValue, null);
			Assert.AreEqual(c.Value, PropertySetPrivateClass.kModifiedValue);
		}
		#endregion


		class TestPropertyNameFromExprClass
		{
			public int Property { get; set; }
		};
		[TestMethod]
		public void Reflection_PropertyNameFromExprTest()
		{
			var value = new TestPropertyNameFromExprClass();
			string name;

			name = Reflection.Util.PropertyNameFromExpr(() => value.Property);
			Assert.AreEqual("Property", name);

			name = Reflection.Util.PropertyNameFromExpr((TestPropertyNameFromExprClass v) => v.Property);
			Assert.AreEqual("Property", name);
		}


		[TestMethod]
		public void Reflection_GenerateLiteralMemberGetterTest()
		{
			// internal const int DefaultBufferSize
			var kDefaultBufferSize = Util.GenerateStaticFieldGetter<System.IO.StreamReader, int>("DefaultBufferSize");

			Assert.AreEqual(1024, kDefaultBufferSize());
		}
	};
}