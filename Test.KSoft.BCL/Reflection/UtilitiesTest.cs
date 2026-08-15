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

		class GenericTypeDefinition<T>
		{
			public T Value { get; set; }
		};

		static void AssertThrowsArgumentNull(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);
		}

		static ArgumentException AssertThrowsArgument(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);

			return exception;
		}

		[TestMethod]
		public void Reflection_GetEnumFieldsGuards_ThrowExpectedExceptions()
		{
			AssertThrowsArgumentNull("enumType", () => _ = Util.GetEnumFields(null!));
			AssertThrowsArgument("enumType", () => _ = Util.GetEnumFields(typeof(string)));
		}

		[TestMethod]
		public void Reflection_GetDelegateForFunctionPointerGuards_ThrowExpectedExceptions()
		{
			const System.Runtime.InteropServices.CallingConvention kWinapi =
				System.Runtime.InteropServices.CallingConvention.Winapi;
			const System.Runtime.InteropServices.CallingConvention kThisCall =
				System.Runtime.InteropServices.CallingConvention.ThisCall;

			AssertThrowsArgument("T", () =>
				_ = Util.GetDelegateForFunctionPointer<string>(new IntPtr(1), kWinapi));
			AssertThrowsArgumentNull("nativePtr", () =>
				_ = Util.GetDelegateForFunctionPointer<MessageBoxDelegate>(IntPtr.Zero, kWinapi));
			var exception = AssertThrowsArgument("callConv", () =>
				_ = Util.GetDelegateForFunctionPointer<MessageBoxDelegate>(new IntPtr(1), kThisCall));

			StringAssert.StartsWith(exception.Message, "TODO: ThisCall's require a different implementation");
		}

		[TestMethod]
		public void Reflection_MemberGetterGuards_ThrowExpectedExceptions()
		{
			AssertThrowsArgument("memberName", () =>
				_ = Util.GenerateMemberGetter<PropertySetPrivateClass, string>(null!));
			AssertThrowsArgument("memberName", () =>
				_ = Util.GenerateStaticPropertyGetter<MemberSetterTestClass, string>(string.Empty));
			AssertThrowsArgument("memberName", () =>
				_ = Util.GenerateStaticFieldGetter<ClassContainingDefaultFileStreamBufferSizeLiteral, int>(null!));

			AssertThrowsArgumentNull("type", () =>
				_ = Util.GenerateMemberGetter<string>(null!, nameof(PropertySetPrivateClass.Value)));
			AssertThrowsArgument("type", () =>
				_ = Util.GenerateMemberGetter<string>(
					typeof(GenericTypeDefinition<>),
					nameof(GenericTypeDefinition<int>.Value)));
			AssertThrowsArgument("memberName", () =>
				_ = Util.GenerateMemberGetter<string>(typeof(PropertySetPrivateClass), string.Empty));
		}

		[TestMethod]
		public void Reflection_MemberSetterGuards_ThrowExpectedExceptions()
		{
			AssertThrowsArgument("memberName", () =>
				_ = Util.GenerateValueTypeMemberSetter<MemberSetterTestStruct, string>(null!));
			AssertThrowsArgument("memberName", () =>
				_ = Util.GenerateReferenceTypeMemberSetter<MemberSetterTestClass, string>(string.Empty));
			AssertThrowsArgumentNull("type", () =>
				_ = Util.GenerateReferenceTypeMemberSetter<string>(null!, nameof(PropertySetPrivateClass.Value)));
			AssertThrowsArgument("type", () =>
				_ = Util.GenerateReferenceTypeMemberSetter<string>(
					typeof(GenericTypeDefinition<>),
					nameof(GenericTypeDefinition<int>.Value)));
			var exception = AssertThrowsArgument("type", () =>
				_ = Util.GenerateReferenceTypeMemberSetter<int>(
					typeof(int),
					nameof(GenericTypeDefinition<int>.Value)));

			StringAssert.StartsWith(exception.Message, "Type must be a reference type");
			AssertThrowsArgument("memberName", () =>
				_ = Util.GenerateStaticPropertySetter<MemberSetterTestClass, string>(null!));
			AssertThrowsArgument("memberName", () =>
				_ = Util.GenerateStaticFieldSetter<MemberSetterTestClass, string>(string.Empty));
		}

		[TestMethod]
		public void Reflection_PropertyNameFromExprGuards_ThrowExpectedExceptions()
		{
			AssertThrowsArgumentNull("expr", () =>
				_ = Util.PropertyNameFromExpr<int>(null!));
			AssertThrowsArgument("expr", () =>
				_ = Util.PropertyNameFromExpr(() => 1 + 1));
			AssertThrowsArgumentNull("expr", () =>
				_ = Util.PropertyNameFromExpr<TestPropertyNameFromExprClass, int>(null!));
			AssertThrowsArgument("expr", () =>
				_ = Util.PropertyNameFromExpr<TestPropertyNameFromExprClass, int>(_ => 1 + 1));
		}

		[TestMethod]
		public void Reflection_DynamicDelegateTypeGuards_ThrowExpectedExceptions()
		{
			AssertThrowsArgumentNull("parameters", () =>
				_ = Util.GenerateDynamicDelegateType(typeof(void), null!));
			AssertThrowsArgument("parameters", () =>
				_ = Util.GenerateDynamicDelegateType(
					typeof(void),
					new Type[Util.kGenerateDynamicDelegateMaximumParameters + 1]));
		}

		[TestMethod]
		public void Reflection_ObjectMethodProxyGuards_ThrowExpectedExceptions()
		{
			AssertThrowsArgument("methodName", () =>
				_ = Util.GenerateObjectMethodProxy<
					TestGenerateObjectMethodProxyClass,
					TestGenerateObjectMethodProxyClassPrivateFunc,
					TestGenerateObjectMethodProxyClassPrivateFuncSig>(
						null!));
			AssertThrowsArgument("TSig", () =>
				_ = Util.GenerateObjectMethodProxy<
					TestGenerateObjectMethodProxyClass,
					TestGenerateObjectMethodProxyClassPrivateFunc,
					string>(
						"PrivateFunc"));
			AssertThrowsArgument("TFunc", () =>
				_ = Util.GenerateObjectMethodProxy<
					TestGenerateObjectMethodProxyClass,
					string,
					TestGenerateObjectMethodProxyClassPrivateFuncSig>(
						"PrivateFunc"));
		}

		[TestMethod]
		public void Reflection_ConstructorFuncGuards_ThrowExpectedExceptions()
		{
			const Reflect.BindingFlags kNonPublicCtorBindingFlags =
				Reflect.BindingFlags.Instance | Reflect.BindingFlags.NonPublic;

			AssertThrowsArgumentNull("type", () =>
				_ = Util.GenerateConstructorFunc<TestGenerateConstructorFuncClass,
					TestGenerateConstructorFuncClassPrivateCtor>(
						null!,
						kNonPublicCtorBindingFlags));
			AssertThrowsArgument("type", () =>
				_ = Util.GenerateConstructorFunc<TestGenerateConstructorFuncClass,
					TestGenerateConstructorFuncClassPrivateCtor>(
						typeof(TestGenerateConstructorFuncClass),
						kNonPublicCtorBindingFlags));
			AssertThrowsArgument("TFunc", () =>
				_ = Util.GenerateConstructorFunc<TestGenerateConstructorFuncClass,
					string>(
						typeof(TestGenerateConstructorFuncSubClass),
						kNonPublicCtorBindingFlags));
			AssertThrowsArgument("TFunc", () =>
				_ = Util.GenerateConstructorFunc<TestGenerateConstructorFuncClass,
					string>());
		}
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
			Assert.AreEqual(PropertySetPrivateClass.kModifiedValue, c.Value);
		}
		[TestMethod]
		public void Reflection_PropertySetPrivateViaReflectionTest()
		{
			var c = new PropertySetPrivateClass();
			var value_prop = Util.PropertyFromExpr(() => c.Value);

			value_prop.SetValue(c, PropertySetPrivateClass.kModifiedValue, null);
			Assert.AreEqual(PropertySetPrivateClass.kModifiedValue, c.Value);
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

		#region GenerateLiteralMemberGetterTest
		internal class ClassContainingDefaultFileStreamBufferSizeLiteral
		{
			protected const int DefaultFileStreamBufferSize = 4096;
		};
		// .net9 update: apparently after .netframework, the IL for *private* const fields changed
		// to no longer generate FieldInfos.
		[TestMethod]
		public void Reflection_GenerateLiteralMemberGetterTest()
		{
			// DefaultBufferSize is a property, at least in .NET 4.5+
			// .net9 update: above is now a const, just like the below value.
			const string kLiteralName = "DefaultFileStreamBufferSize";

			// .net9 update: apparently after .netframework, the IL for *private* const fields changed
			// to no longer generate FieldInfos.
			Type typeContainingLiteral = typeof(/*System.IO.StreamReader*/ClassContainingDefaultFileStreamBufferSizeLiteral);
			var literalFieldInfo = typeContainingLiteral.GetField(
				kLiteralName,
				Reflect.BindingFlags.Static |
				Reflect.BindingFlags.NonPublic |
				Reflect.BindingFlags.IgnoreCase |
				Reflect.BindingFlags.FlattenHierarchy);
			Assert.IsNotNull(literalFieldInfo, "Literal not found. Renamed?");
			Assert.IsTrue(literalFieldInfo.IsLiteral);

			// internal const int DefaultBufferSize
			var kDefaultBufferSize = Util.GenerateStaticFieldGetter</*System.IO.StreamReader*/ClassContainingDefaultFileStreamBufferSizeLiteral, int>(
				kLiteralName);

			Assert.AreEqual(4096, kDefaultBufferSize());
		}
		#endregion

		#region Generate MemberSetter fail tests
		readonly struct MemberSetterTestStruct
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
		public void Reflection_GenerateValueTypeMemberSetterFailTest1()
		{
			Assert.Throws<MemberAccessException>(() =>
				Util.GenerateValueTypeMemberSetter<MemberSetterTestStruct, string>("mValueReadonly")
			);
		}
		[TestMethod]
		[Description("Validate GenerateValueTypeMemberSetter fails on a get-only property")]
		public void Reflection_GenerateValueTypeMemberSetterFailTest2()
		{
			Assert.Throws<MemberAccessException>(() =>
				Util.GenerateValueTypeMemberSetter<MemberSetterTestStruct, string>("ValueNoSetter")
			);
		}

		[TestMethod]
		[Description("Validate GenerateReferenceTypeMemberSetter fails on readonly field")]
		public void Reflection_GenerateReferenceTypeMemberSetterFailTest1()
		{
			Assert.Throws<MemberAccessException>(() =>
				Util.GenerateReferenceTypeMemberSetter<MemberSetterTestClass, string>("mValueReadonly")
			);
		}
		[TestMethod]
		[Description("Validate GenerateReferenceTypeMemberSetter fails on a get-only property")]
		public void Reflection_GenerateReferenceTypeMemberSetterFailTest2()
		{
			Assert.Throws<MemberAccessException>(() =>
				Util.GenerateReferenceTypeMemberSetter<MemberSetterTestClass, string>("ValueNoSetter")
			);
		}

		[TestMethod]
		[Description("Validate GenerateStaticFieldSetter fails on readonly field")]
		public void Reflection_GenerateStaticFieldSetterFailTest()
		{
			Assert.Throws<MemberAccessException>(() =>
				Util.GenerateStaticFieldSetter<MemberSetterTestClass, string>("mStaticValueReadonly")
			);
		}
		[TestMethod]
		[Description("Validate GenerateStaticPropertySetter fails on a get-only property")]
		public void Reflection_GenerateStaticPropertySetterFailTest()
		{
			Assert.Throws<MemberAccessException>(() =>
				Util.GenerateStaticPropertySetter<MemberSetterTestClass, string>("StaticValueNoSetter")
			);
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
			internal TestGenerateConstructorFuncClass(int _)
			{
			}
			public TestGenerateConstructorFuncClass(object _, double _1)
			{

			}
		};
		internal class TestGenerateConstructorFuncSubClass : TestGenerateConstructorFuncClass
		{
			public TestGenerateConstructorFuncSubClass()
				: base(null, 0.0)
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
