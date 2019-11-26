using System.Collections.Generic;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Debug
{
	public static class ValueCheck
	{
		public static void AreEqual<T>(string description, T expected, T actual,
			string expectedDisplayValue = null, string actualDisplayValue = null)
		{
			Contract.Requires(!string.IsNullOrEmpty(description));

			if (!EqualityComparer<T>.Default.Equals(expected, actual))
			{
				if (expectedDisplayValue == null)
					expectedDisplayValue = expected.ToString();
				if (actualDisplayValue == null)
					actualDisplayValue = actual.ToString();

				throw new InvalidDataException(string.Format("{0}. Expected '{1}' but got '{2}'",
					description, expectedDisplayValue, actualDisplayValue));
			}
		}

		public static void IsLessThanEqualTo(string description, int expectedMax, int actualMax)
		{
			Contract.Requires(!string.IsNullOrEmpty(description));

			if (actualMax > expectedMax)
				throw new InvalidDataException(string.Format("{0}. Expected at most {1}, got {2}",
					description, expectedMax, actualMax));
		}

		public static void IsGreaterThanEqualTo(string description, int expectedMin, int actualMin)
		{
			Contract.Requires(!string.IsNullOrEmpty(description));

			if (actualMin < expectedMin)
				throw new InvalidDataException(string.Format("{0}. Expected at least {1}, got {2}",
					description, expectedMin, actualMin));
		}

		public static void IsDistinct<T>(string description, string valueName, IEnumerable<T> seq)
		{
			Contract.Requires(!string.IsNullOrEmpty(description));
			Contract.Requires(!string.IsNullOrEmpty(valueName));
			Contract.Requires(seq != null);

			if (seq.ContainsDuplicates())
				throw new InvalidDataException(string.Format("{0}. Expected all '{1}' values to be distinct",
					description, valueName));
		}
	};
}