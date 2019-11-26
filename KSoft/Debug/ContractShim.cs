#if CONTRACTS_FULL_SHIM
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Reflection;

// https://github.com/Microsoft/CodeContracts/issues/409#issuecomment-268913908

/* COPY PASTA:
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
*/

namespace System.Diagnostics.ContractsShim
{
	// alt for System.Diagnostics.Contracts.__ContractsRuntime+ContractException
	internal class ContractShimException : Exception
	{
		public ContractShimException() : base() { }
		public ContractShimException(string message) : base(message) { }
		public ContractShimException(string message, Exception inner) : base(message, inner) { }
	};

	public static class Contract
	{
		//
		// Summary:
		//     Checks for a condition; if the condition is false, follows the escalation policy
		//     set for the analyzer.
		//
		// Parameters:
		//   condition:
		//     The conditional expression to test.
		[Conditional("DEBUG")]
		[Conditional("CONTRACTS_FULL_SHIM")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void Assert(bool condition)
		{
			System.Diagnostics.Debug.Assert(condition);
		}
		//
		// Summary:
		//     Checks for a condition; if the condition is false, follows the escalation policy
		//     set by the analyzer and displays the specified message.
		//
		// Parameters:
		//   condition:
		//     The conditional expression to test.
		//
		//   userMessage:
		//     A message to display if the condition is not met.
		[Conditional("CONTRACTS_FULL_SHIM")]
		[Conditional("DEBUG")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void Assert(bool condition, string userMessage)
		{
			System.Diagnostics.Debug.Assert(condition, userMessage);
		}
		//
		// Summary:
		//     Instructs code analysis tools to assume that the specified condition is true,
		//     even if it cannot be statically proven to always be true.
		//
		// Parameters:
		//   condition:
		//     The conditional expression to assume true.
		[Conditional("DEBUG")]
		[Conditional("CONTRACTS_FULL_SHIM")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void Assume(bool condition)
		{
			System.Diagnostics.Debug.Assert(condition);
		}
		//
		// Summary:
		//     Instructs code analysis tools to assume that a condition is true, even if it
		//     cannot be statically proven to always be true, and displays a message if the
		//     assumption fails.
		//
		// Parameters:
		//   condition:
		//     The conditional expression to assume true.
		//
		//   userMessage:
		//     The message to post if the assumption fails.
		[Conditional("DEBUG")]
		[Conditional("CONTRACTS_FULL_SHIM")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void Assume(bool condition, string userMessage)
		{
			System.Diagnostics.Debug.Assert(condition, userMessage);
		}
		//
		// Summary:
		//     Marks the end of the contract section when a method's contracts contain only
		//     preconditions in the if-then-throw form.
		[Conditional("CONTRACTS_FULL_SHIM")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static void EndContractBlock()
		{
			// do nothing
		}
		//
		// Summary:
		//     Specifies a postcondition contract for the enclosing method or property.
		//
		// Parameters:
		//   condition:
		//     The conditional expression to test. The expression may include System.Diagnostics.Contracts.Contract.OldValue``1(``0),
		//     System.Diagnostics.Contracts.Contract.ValueAtReturn``1(``0@), and System.Diagnostics.Contracts.Contract.Result``1
		//     values.
		[Conditional("CONTRACTS_FULL_SHIM")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void Ensures(bool condition)
		{
			// #TODO something
			//System.Diagnostics.Debug.Assert(condition);
		}
		//
		// Summary:
		//     Specifies a postcondition contract for a provided exit condition and a message
		//     to display if the condition is false.
		//
		// Parameters:
		//   condition:
		//     The conditional expression to test. The expression may include System.Diagnostics.Contracts.Contract.OldValue``1(``0)
		//     and System.Diagnostics.Contracts.Contract.Result``1 values.
		//
		//   userMessage:
		//     The message to display if the expression is not true.
		[Conditional("CONTRACTS_FULL_SHIM")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void Ensures(bool condition, string userMessage)
		{
			// #TODO something
			//System.Diagnostics.Debug.Assert(condition, userMessage);
		}
#if false
		//
		// Summary:
		//     Specifies a postcondition contract for the enclosing method or property, based
		//     on the provided exception and condition.
		//
		// Parameters:
		//   condition:
		//     The conditional expression to test.
		//
		// Type parameters:
		//   TException:
		//     The type of exception that invokes the postcondition check.
		[Conditional("CONTRACTS_FULL_SHIM")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void EnsuresOnThrow<TException>(bool condition) where TException : Exception;
#endif
#if false
		//
		// Summary:
		//     Specifies a postcondition contract and a message to display if the condition
		//     is false for the enclosing method or property, based on the provided exception
		//     and condition.
		//
		// Parameters:
		//   condition:
		//     The conditional expression to test.
		//
		//   userMessage:
		//     The message to display if the expression is false.
		//
		// Type parameters:
		//   TException:
		//     The type of exception that invokes the postcondition check.
		[Conditional("CONTRACTS_FULL_SHIM")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void EnsuresOnThrow<TException>(bool condition, string userMessage) where TException : Exception;
#endif
#if false
		//
		// Summary:
		//     Determines whether a specified test is true for any integer within a range of
		//     integers.
		//
		// Parameters:
		//   fromInclusive:
		//     The first integer to pass to predicate.
		//
		//   toExclusive:
		//     One more than the last integer to pass to predicate.
		//
		//   predicate:
		//     The function to evaluate for any value of the integer in the specified range.
		//
		// Returns:
		//     true if predicate returns true for any integer starting from fromInclusive to
		//     toExclusive - 1.
		//
		// Exceptions:
		//   T:System.ArgumentNullException:
		//     predicate is null.
		//
		//   T:System.ArgumentException:
		//     toExclusive is less than fromInclusive.
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static bool Exists(int fromInclusive, int toExclusive, Predicate<int> predicate);
		//
		// Summary:
		//     Determines whether an element within a collection of elements exists within a
		//     function.
		//
		// Parameters:
		//   collection:
		//     The collection from which elements of type T will be drawn to pass to predicate.
		//
		//   predicate:
		//     The function to evaluate for an element in collection.
		//
		// Type parameters:
		//   T:
		//     The type that is contained in collection.
		//
		// Returns:
		//     true if and only if predicate returns true for any element of type T in collection.
		//
		// Exceptions:
		//   T:System.ArgumentNullException:
		//     collection or predicate is null.
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static bool Exists<T>(IEnumerable<T> collection, Predicate<T> predicate);
		//
		// Summary:
		//     Determines whether a particular condition is valid for all integers in a specified
		//     range.
		//
		// Parameters:
		//   fromInclusive:
		//     The first integer to pass to predicate.
		//
		//   toExclusive:
		//     One more than the last integer to pass to predicate.
		//
		//   predicate:
		//     The function to evaluate for the existence of the integers in the specified range.
		//
		// Returns:
		//     true if predicate returns true for all integers starting from fromInclusive to
		//     toExclusive - 1.
		//
		// Exceptions:
		//   T:System.ArgumentNullException:
		//     predicate is null.
		//
		//   T:System.ArgumentException:
		//     toExclusive is less than fromInclusive.
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static bool ForAll(int fromInclusive, int toExclusive, Predicate<int> predicate);
		//
		// Summary:
		//     Determines whether all the elements in a collection exist within a function.
		//
		// Parameters:
		//   collection:
		//     The collection from which elements of type T will be drawn to pass to predicate.
		//
		//   predicate:
		//     The function to evaluate for the existence of all the elements in collection.
		//
		// Type parameters:
		//   T:
		//     The type that is contained in collection.
		//
		// Returns:
		//     true if and only if predicate returns true for all elements of type T in collection.
		//
		// Exceptions:
		//   T:System.ArgumentNullException:
		//     collection or predicate is null.
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static bool ForAll<T>(IEnumerable<T> collection, Predicate<T> predicate);
#endif
		//
		// Summary:
		//     Specifies an invariant contract for the enclosing method or property.
		//
		// Parameters:
		//   condition:
		//     The conditional expression to test.
		[Conditional("CONTRACTS_FULL_SHIM")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void Invariant(bool condition)
		{
			System.Diagnostics.Debug.Assert(condition);
		}
		//
		// Summary:
		//     Specifies an invariant contract for the enclosing method or property, and displays
		//     a message if the condition for the contract fails.
		//
		// Parameters:
		//   condition:
		//     The conditional expression to test.
		//
		//   userMessage:
		//     The message to display if the condition is false.
		[Conditional("CONTRACTS_FULL_SHIM")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void Invariant(bool condition, string userMessage)
		{
			System.Diagnostics.Debug.Assert(condition, userMessage);
		}
#if true // #HORRIBLE_SHIM
		//
		// Summary:
		//     Represents values as they were at the start of a method or property.
		//
		// Parameters:
		//   value:
		//     The value to represent (field or parameter).
		//
		// Type parameters:
		//   T:
		//     The type of value.
		//
		// Returns:
		//     The value of the parameter or field at the start of a method or property.
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static T OldValue<T>(T value)
		{
			return value;
		}
#endif
		//
		// Summary:
		//     Specifies a precondition contract for the enclosing method or property.
		//
		// Parameters:
		//   condition:
		//     The conditional expression to test.
		[Conditional("CONTRACTS_FULL_SHIM")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void Requires(bool condition)
		{
			if (!condition)
			{
				throw new ContractShimException();
			}
		}
		//
		// Summary:
		//     Specifies a precondition contract for the enclosing method or property, and displays
		//     a message if the condition for the contract fails.
		//
		// Parameters:
		//   condition:
		//     The conditional expression to test.
		//
		//   userMessage:
		//     The message to display if the condition is false.
		[Conditional("CONTRACTS_FULL_SHIM")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void Requires(bool condition, string userMessage)
		{
			if (!condition)
			{
				throw new ContractShimException(userMessage);
			}
		}
		//
		// Summary:
		//     Specifies a precondition contract for the enclosing method or property, and throws
		//     an exception if the condition for the contract fails.
		//
		// Parameters:
		//   condition:
		//     The conditional expression to test.
		//
		// Type parameters:
		//   TException:
		//     The exception to throw if the condition is false.
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void Requires<TException>(bool condition) where TException : Exception
		{
			if (!condition)
			{
				var ex = Activator.CreateInstance<TException>();
				throw ex;
			}
		}
		//
		// Summary:
		//     Specifies a precondition contract for the enclosing method or property, and throws
		//     an exception with the provided message if the condition for the contract fails.
		//
		// Parameters:
		//   condition:
		//     The conditional expression to test.
		//
		//   userMessage:
		//     The message to display if the condition is false.
		//
		// Type parameters:
		//   TException:
		//     The exception to throw if the condition is false.
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void Requires<TException>(bool condition, string userMessage) where TException : Exception
		{
			if (!condition)
			{
				var ex = Activator.CreateInstance<TException>();
				typeof(TException).GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(ex, userMessage);
				throw ex;
			}
		}
#if true // #HORRIBLE_SHIM
		//
		// Summary:
		//     Represents the return value of a method or property.
		//
		// Type parameters:
		//   T:
		//     Type of return value of the enclosing method or property.
		//
		// Returns:
		//     Return value of the enclosing method or property.
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static T Result<T>() { return default(T); }
		//
		// Summary:
		//     Represents the final (output) value of an out parameter when returning from a
		//     method.
		//
		// Parameters:
		//   value:
		//     The out parameter.
		//
		// Type parameters:
		//   T:
		//     The type of the out parameter.
		//
		// Returns:
		//     The output value of the out parameter.
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static T ValueAtReturn<T>(out T value)
		{
			value = default(T);
			return value;
		}
#endif
	};
};

#endif // CONTRACTS_FULL_SHIM

