using System;
using System.Collections.Generic;
using System.IO;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

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

				throw new InvalidDataException(string.Format("{0}. Expected '{0}' but got '{1}'",
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