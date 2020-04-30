using System;
using System.Diagnostics.CodeAnalysis;
using Reflect = System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Reflection.Test
{
	using MessageBoxDelegateGeneric = Func<IntPtr, string, string, uint, int>;

	#region GenerateObjectMethodProxy aliases
	using TestGenerateObjectMethodProxyClassPrivateFuncSig = Func<int,
		bool>;
	using TestGenerateObjectMethodProxyClassPrivateFunc = Func<UtilitiesTest.TestGenerateObjectMethodProxyClass, int,
		bool>;
	#endregion

	#region GenerateConstructorFunc aliases
	using TestGenerateConstructorFuncClassPrivateCtor =		Func<
		UtilitiesTest.TestGenerateConstructorFuncClass>;
	using TestGenerateConstructorFuncClassInternalCtor =	Func<int,
		UtilitiesTest.TestGenerateConstructorFuncClass>;
	using TestGenerateConstructorFuncClassPublicCtor =		Func<object, double,
		UtilitiesTest.TestGenerateConstructorFuncClass>;
	#endregion

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

		#region PropertyNameFromExpr
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
		#endregion

		[TestMethod]
		public void Reflection_GenerateLiteralMemberGetterTest()
		{
			// DefaultBufferSize is a property, at least in .NET 4.5+
			const string kLiteralName = "DefaultFileStreamBufferSize";

			var literalFieldInfo = typeof(System.IO.StreamReader).GetField(
				kLiteralName,
				Reflect.BindingFlags.Static |
				Reflect.BindingFlags.Instance |
				Reflect.BindingFlags.NonPublic |
				Reflect.BindingFlags.IgnoreCase |
				Reflect.BindingFlags.FlattenHierarchy);
			Assert.IsNotNull(literalFieldInfo, "Literal not found. Renamed?");
			Assert.IsTrue(literalFieldInfo.IsLiteral);

			// internal const int DefaultBufferSize
			var kDefaultBufferSize = Util.GenerateStaticFieldGetter<System.IO.StreamReader, int>(kLiteralName);

			Assert.AreEqual(4096, kDefaultBufferSize());
		}

		#region Generate MemberSetter fail tests
		struct MemberSetterTestStruct
		{
#pragma warning disable 649
			private readonly string mValueReadonly;
#pragma warning restore 649

			private string ValueNoSetter { get { return mValueReadonly; } }
		};
		// ReSharper disable once ClassNeverInstantiated.Local
		[SuppressMessage("Microsoft.Design", "CA1812:AvoidUninstantiatedInternalClasses")]
		class MemberSetterTestClass
		{
#pragma warning disable 649
			private readonly string mValueReadonly;
			private static readonly string mStaticValueReadonly;
#pragma warning restore 649

			private string ValueNoSetter { get { return mValueReadonly; } }

			private static string StaticValueNoSetter { get { return mStaticValueReadonly; } }
		};

		[TestMethod]
		[Description("Validate GenerateValueTypeMemberSetter fails on readonly field")]
		[ExpectedException(typeof(MemberAccessException))]
		public void Reflection_GenerateValueTypeMemberSetterFailTest1()
		{
			Util.GenerateValueTypeMemberSetter<MemberSetterTestStruct, string>("mValueReadonly");
		}
		[TestMethod]
		[Description("Validate GenerateValueTypeMemberSetter fails on a get-only property")]
		[ExpectedException(typeof(MemberAccessException))]
		public void Reflection_GenerateValueTypeMemberSetterFailTest2()
		{
			Util.GenerateValueTypeMemberSetter<MemberSetterTestStruct, string>("ValueNoSetter");
		}

		[TestMethod]
		[Description("Validate GenerateReferenceTypeMemberSetter fails on readonly field")]
		[ExpectedException(typeof(MemberAccessException))]
		public void Reflection_GenerateReferenceTypeMemberSetterFailTest1()
		{
			Util.GenerateReferenceTypeMemberSetter<MemberSetterTestClass, string>("mValueReadonly");
		}
		[TestMethod]
		[Description("Validate GenerateReferenceTypeMemberSetter fails on a get-only property")]
		[ExpectedException(typeof(MemberAccessException))]
		public void Reflection_GenerateReferenceTypeMemberSetterFailTest2()
		{
			Util.GenerateReferenceTypeMemberSetter<MemberSetterTestClass, string>("ValueNoSetter");
		}

		[TestMethod]
		[Description("Validate GenerateStaticFieldSetter fails on readonly field")]
		[ExpectedException(typeof(MemberAccessException))]
		public void Reflection_GenerateStaticFieldSetterFailTest()
		{
			Util.GenerateStaticFieldSetter<MemberSetterTestClass, string>("mStaticValueReadonly");
		}
		[TestMethod]
		[Description("Validate GenerateStaticPropertySetter fails on a get-only property")]
		[ExpectedException(typeof(MemberAccessException))]
		public void Reflection_GenerateStaticPropertySetterFailTest()
		{
			Util.GenerateStaticPropertySetter<MemberSetterTestClass, string>("StaticValueNoSetter");
		}
		#endregion

		#region GenerateObjectMethodProxy
		internal class TestGenerateObjectMethodProxyClass
		{
			private bool PrivateFunc(int value)
			{
				KSoft.Util.MarkUnusedVariable(ref value);
				return true;
			}
		};
		[TestMethod]
		public void Reflection_GenerateObjectMethodProxyTest()
		{
			var proxy_func =
				Util.GenerateObjectMethodProxy<
					TestGenerateObjectMethodProxyClass,
					TestGenerateObjectMethodProxyClassPrivateFunc,
					TestGenerateObjectMethodProxyClassPrivateFuncSig>(
						"PrivateFunc");

			Assert.IsNotNull(proxy_func,
				"PrivateFunc-proxy method generation failed");
			Assert.AreEqual(true, proxy_func(new TestGenerateObjectMethodProxyClass(), 0),
				"PrivateFunc-proxy didn't return true, something is very wrong");
		}
		#endregion

		#region GenerateConstructorFunc
		[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
		[SuppressMessage("Microsoft.Design", "CA1812:AvoidUninstantiatedInternalClasses")]
		internal class TestGenerateConstructorFuncClass
		{
			private TestGenerateConstructorFuncClass()
			{
			}
			internal TestGenerateConstructorFuncClass(int i)
			{
			}
			public TestGenerateConstructorFuncClass(object o, double d)
			{

			}
		};
		[TestMethod]
		public void Reflection_GenerateConstructorFuncTest()
		{
			const Reflect.BindingFlags k_non_public_ctor_binding_flags =
				Reflect.BindingFlags.Instance | Reflect.BindingFlags.NonPublic;

			var ctor_priv = Util.GenerateConstructorFunc<TestGenerateConstructorFuncClass,
				TestGenerateConstructorFuncClassPrivateCtor>(k_non_public_ctor_binding_flags);
			Assert.IsNotNull(ctor_priv);

			var ctor_internal = Util.GenerateConstructorFunc<TestGenerateConstructorFuncClass,
				TestGenerateConstructorFuncClassInternalCtor>(k_non_public_ctor_binding_flags);
			Assert.IsNotNull(ctor_internal);

			var ctor_public = Util.GenerateConstructorFunc<TestGenerateConstructorFuncClass,
				TestGenerateConstructorFuncClassPublicCtor>();
			Assert.IsNotNull(ctor_public);

			Assert.IsNotNull(ctor_priv());
			Assert.IsNotNull(ctor_internal(1234));
			Assert.IsNotNull(ctor_public(null, 1234.0));
		}
		#endregion
	};
}
