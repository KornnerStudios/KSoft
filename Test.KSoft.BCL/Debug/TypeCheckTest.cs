using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Debug.Test;

[TestClass]
public sealed class TypeCheckTest : BaseTestClass
{
	sealed class BaseReference
	{
	}

	sealed class OtherReference
	{
	}

	[TestMethod]
	public void CastValue_MatchingValue_ReturnsTypedValue()
	{
		object value = 42;

		TypeCheck.CastValue(value, out int result);

		Assert.AreEqual(42, result);
		Assert.AreEqual(42, TypeCheck.CastValue<int>(value));
	}

	[TestMethod]
	public void CastValue_NullValue_ThrowsArgumentNullException()
	{
		var exception = Assert.ThrowsExactly<ArgumentNullException>(() => TypeCheck.CastValue<int>(null!));

		Assert.AreEqual("value", exception.ParamName);

		exception = Assert.ThrowsExactly<ArgumentNullException>(() => TypeCheck.CastValue(null!, out int _));

		Assert.AreEqual("value", exception.ParamName);
	}

	[TestMethod]
	public void CastValue_MismatchedValue_ThrowsArgumentExceptionWithInnerCast()
	{
		var exception = Assert.ThrowsExactly<ArgumentException>(() => TypeCheck.CastValue<int>("42"));

		Assert.IsInstanceOfType<InvalidCastException>(exception.InnerException);
	}

	[TestMethod]
	public void CastReference_MatchingReference_ReturnsTypedReference()
	{
		var value = new BaseReference();

		TypeCheck.CastReference<object, BaseReference>(value, out var result);

		Assert.AreSame(value, result);
		Assert.AreSame(value, TypeCheck.CastReference<object, BaseReference>(value));
		Assert.AreSame(value, TypeCheck.CastReference<BaseReference>(value));
	}

	[TestMethod]
	public void CastReference_NullValue_ThrowsArgumentNullException()
	{
		var exception = Assert.ThrowsExactly<ArgumentNullException>(() =>
			TypeCheck.CastReference<object, BaseReference>(null!));

		Assert.AreEqual("value", exception.ParamName);

		exception = Assert.ThrowsExactly<ArgumentNullException>(() =>
			TypeCheck.CastReference<object, BaseReference>(null!, out _));

		Assert.AreEqual("value", exception.ParamName);
	}

	[TestMethod]
	public void CastReference_MismatchedReference_ThrowsArgumentException()
	{
		Assert.ThrowsExactly<ArgumentException>(() =>
			TypeCheck.CastReference<object, BaseReference>(new OtherReference()));
	}

	[TestMethod]
	public void TryCastReference_NullValue_ReturnsNull()
	{
		TypeCheck.TryCastReference<object, BaseReference>(null, out var result);

		Assert.IsNull(result);
	}

	[TestMethod]
	public void TryCastReference_MatchingReference_ReturnsTypedReference()
	{
		var value = new BaseReference();

		TypeCheck.TryCastReference<object, BaseReference>(value, out var result);

		Assert.AreSame(value, result);
	}

	[TestMethod]
	public void TryCastReference_MismatchedReference_ThrowsArgumentException()
	{
		Assert.ThrowsExactly<ArgumentException>(() =>
			TypeCheck.TryCastReference<object, BaseReference>(new OtherReference(), out _));
	}
}
