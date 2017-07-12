using System;
using System.Collections.Generic;
using System.Linq;

namespace KSoft.IO
{
	using BitVector32 = Collections.BitVector32;
	using BitVector64 = Collections.BitVector64;

	static class JsonNodeGlobals
	{
		[ThreadStatic]
		public static List<string> gErrorsOutputList = new List<string>();
	};

	public struct JsonNode
	{
		private IDictionary<string, object> mData;

		public bool IsNull { get { return mData == null; } }
		public bool IsNotNull { get { return mData != null; } }
		public bool IsEmpty { get { return IsNull || mData.Count == 0; } }
		public bool IsNotEmpty { get { return IsNotNull && mData.Count > 0; } }

		public IDictionary<string, object> DictionaryData { get { return mData; } }

		public JsonNode(IDictionary<string, object> parsedData)
		{
			mData = parsedData;
		}

		public static JsonNode New { get {
			return new JsonNode
			{
				mData = new Dictionary<string, object>(),
			};
		} }

		public static JsonNode Null { get { return new JsonNode(); } }

		public IEnumerable<KeyValuePair<string, JsonNode>> ChildNodes { get {
			if (IsNull)
				yield break;

			foreach (var kvp in mData)
			{
				var nodeValues = kvp.Value as IDictionary<string, object>;
				if (nodeValues == null)
					continue;

				var childNode = new JsonNode(nodeValues);
				yield return new KeyValuePair<string, JsonNode>(kvp.Key, childNode);
			}
		} }

		public IEnumerable<KeyValuePair<string, object>> RawData { get {
			if (IsNull)
				return Enumerable.Empty<KeyValuePair<string, object>>();

			var enumerable = (IEnumerable<KeyValuePair<string, object>>)mData;
			return new EnumeratorWrapper<KeyValuePair<string, object>>(enumerable);
		} }

		public ICollection<string> ChildNames { get {
			if (IsNull)
				return new string[0];

			return mData.Keys;
		} }

		public string ToJson(bool prettyPrint = false)
		{
			return MiniJSON.Json.Serialize(mData, prettyPrint);
		}

		public bool ContainsChild(string valueName)
		{
			if (IsNull)
				return false;

			return mData.ContainsKey(valueName);
		}

		/// <summary>
		/// Adds a child JsonNode with the given name, or gets an existing child JsonNode
		/// </summary>
		public JsonNode AddChild(string valueName)
		{
			if (IsNull)
				return Null;

			IDictionary<string, object> childData;

			object existingValue;
			if (mData.TryGetValue(valueName, out existingValue))
			{
				childData = existingValue as IDictionary<string, object>;

				if (childData == null)
				{
					Debug.Trace.IO.TraceDataSansId(System.Diagnostics.TraceEventType.Error, string.Format(
						"Trying to add a child node named {0} when a value already exists but is an unexpected type: {1}",
						valueName, existingValue.GetType()));
					return Null;
				}
			}
			else
			{
				childData = new Dictionary<string, object>();
				mData.Add(valueName, childData);
			}

			return new JsonNode(childData);
		}

		public JsonNode GetChild(string valueName)
		{
			if (IsNull)
				return Null;

			IDictionary<string, object> childData;

			object existingValue;
			if (mData.TryGetValue(valueName, out existingValue))
			{
				childData = existingValue as IDictionary<string, object>;

				if (childData == null)
				{
					Debug.Trace.IO.TraceDataSansId(System.Diagnostics.TraceEventType.Error, string.Format(
						"Trying to get a child node named {0} with an unexpected type: {1}",
						valueName, existingValue.GetType()));
					return Null;
				}
			}
			else
				return Null;

			return new JsonNode(childData);
		}

		public bool AddArrayAsObjects(string valueName, IList<object> objects)
		{
			if (IsNull)
				return false;

			IList<object> existingArray = null;

			object existingValue;
			if (mData.TryGetValue(valueName, out existingValue))
			{
				existingArray = existingValue as IList<object>;

				if (existingArray == null)
				{
					Debug.Trace.IO.TraceDataSansId(System.Diagnostics.TraceEventType.Error, string.Format(
						"Trying to add a child node named {0} when a value already exists but is an unexpected type: {1}",
						valueName, existingValue.GetType()));
					return false;
				}
			}
			else
			{
				existingArray = new List<object>();
				mData.Add(valueName, existingArray);
			}

			foreach (var obj in objects)
				existingArray.Add(obj);

			return true;
		}

		public bool AddArrayAsObjects(string valueName, IEnumerable<JsonNode> objects)
		{
			if (IsNull)
				return false;

			IList<object> existingArray = null;

			object existingValue;
			if (mData.TryGetValue(valueName, out existingValue))
			{
				existingArray = existingValue as IList<object>;

				if (existingArray == null)
				{
					Debug.Trace.IO.TraceDataSansId(System.Diagnostics.TraceEventType.Error, string.Format(
						"Trying to add a child node named {0} when a value already exists but is an unexpected type: {1}",
						valueName, existingValue.GetType()));
					return false;
				}
			}
			else
			{
				existingArray = new List<object>();
				mData.Add(valueName, existingArray);
			}

			foreach (var obj in objects)
				existingArray.Add(obj.mData);

			return true;
		}

		public IEnumerable<JsonNode> GetArrayAsObjects(string valueName)
		{
			if (IsNull)
				yield break;

			IList<object> array;

			object existingValue;
			if (mData.TryGetValue(valueName, out existingValue))
			{
				array = existingValue as IList<object>;

				if (array == null)
				{
					Debug.Trace.IO.TraceDataSansId(System.Diagnostics.TraceEventType.Error, string.Format(
						"Trying to get a child node named {0} with an unexpected type: {1}",
						valueName, existingValue.GetType()));
					yield break;
				}
			}
			else
				yield break;

			foreach (var o in array)
			{
				var arrayElement = o as IDictionary<string, object>;
				if (arrayElement == null)
					continue;

				yield return new JsonNode(arrayElement);
			}
		}

		#region SetValue
		public bool SetValueForName(string valueName, string value)
		{
			if (value == null)
				return false;

			MiniJSON.Json.SetValue(mData, valueName, value);
			return true;
		}
		public bool SetValuesForName(string valueName, IEnumerable<string> values)
		{
			if (values == null)
				return false;

			var list = values as List<string>;
			if (list == null)
				list = new List<string>(values);

			MiniJSON.Json.SetValue(mData, valueName, list);
			return true;
		}

		public bool SetValue(string valueName, bool value)
		{
			MiniJSON.Json.SetValue(mData, valueName, value);
			return true;
		}

		public bool SetValue(string valueName, string value)
		{
			return SetValueForName(valueName, value);
		}

		public bool SetValue(string valueName, int value)
		{
			MiniJSON.Json.SetValue(mData, valueName, value);
			return true;
		}

		public bool SetValue(string valueName, long value)
		{
			MiniJSON.Json.SetValue(mData, valueName, value);
			return true;
		}

		public bool SetValue(string valueName, float value)
		{
			MiniJSON.Json.SetValue(mData, valueName, value);
			return true;
		}

		public bool SetValue(string valueName, double value)
		{
			MiniJSON.Json.SetValue(mData, valueName, value);
			return true;
		}

		public bool SetEnumValue<TEnum>(string valueName, TEnum value)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			MiniJSON.Json.SetValue(mData, valueName, value.ToString());
			return true;
		}
		public bool SetEnumValue<TEnum>(string valueName, TEnum value, TEnum skipWritingIfThisValue)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (value.Equals(skipWritingIfThisValue))
				return false;

			MiniJSON.Json.SetValue(mData, valueName, value.ToString());
			return true;
		}

		/// <typeparam name="TEnum">Members should be bit indices, not literal flag values</typeparam>
		/// <param name="useArray">A JSON array is used instead of a single seperated string</param>
		public bool SetFlagsValue<TEnum>(string valueName, BitVector32 value
			, TEnum maxValue
			, string valueSeperator = ",", bool useArray = true)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (value.IsAllClear)
				return false;

			if (useArray)
			{
				var values = value.ToStrings(maxValue, valueSeperator);
				return SetValuesForName(valueName, values);
			}
			else
			{
				string values = value.ToString(maxValue, valueSeperator);
				return SetValueForName(valueName, values);
			}
		}

		/// <typeparam name="TEnum">Members should be bit indices, not literal flag values</typeparam>
		/// <param name="useArray">A JSON array is used instead of a single seperated string</param>
		public bool SetFlagsValue<TEnum>(string valueName, BitVector64 value
			, TEnum maxValue
			, string valueSeperator = ",", bool useArray = true)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (value.IsAllClear)
				return false;

			if (useArray)
			{
				var values = value.ToStrings(maxValue, valueSeperator);
				return SetValuesForName(valueName, values);
			}
			else
			{
				string values = value.ToString(maxValue, valueSeperator);
				return SetValueForName(valueName, values);
			}
		}

		/// <param name="useArray">A JSON array is used instead of a single seperated string</param>
		public bool SetValue(string valueName, List<string> list
			, bool sort = false, string valueSeperator = ",", bool useArray = true)
		{
			if (list == null || list.Count == 0)
				return false;

			if (sort)
				list.Sort();

			if (useArray || string.IsNullOrEmpty(valueSeperator))
			{
				MiniJSON.Json.SetValue(mData, valueName, list);
				return true;
			}

			var value = list.TransformToString(valueSeperator);
			if (value == null)
				return false;

			return SetValueForName(valueName, value);
		}

		public bool SetPodValues<T>(string valueName, IList<T> list
			, string valueSeperator = ",")
		{
			if (list == null || list.Count == 0)
				return false;

			var type = typeof(T);
			var typeCode = Type.GetTypeCode(type);

			switch (typeCode)
			{
				case TypeCode.Boolean:
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.String:
					break;

				default:
				{
					Debug.Trace.IO.TraceDataSansId(System.Diagnostics.TraceEventType.Error, string.Format(
						"We currently don't handle setting JSON data from lists of {0} types (Type={1})",
						typeCode, type));
					return false;
				}
			}

			MiniJSON.Json.SetValue(mData, valueName, list);
			return true;
		}
		#endregion

		#region GetValue
		public object TryGetValueForName(string valueName)
		{
			if (IsEmpty)
				return null;

			object value;
			mData.TryGetValue(valueName, out value);

			return value;
		}

		public bool GetValue(string valueName, ref bool retVal)
		{
			object value = TryGetValueForName(valueName);

			return ParseValue(value, ref retVal);
		}
		private bool ParseValue(object value, ref bool retVal)
		{
			if (value == null)
				return false;

			switch (Type.GetTypeCode(value.GetType()))
			{
				case TypeCode.Boolean:
					retVal = (bool)value;
					return true;

				case TypeCode.String:
				{
					bool temp = false;
					string str = (string)value;

					switch (str)
					{
						case "0":
							temp = false;
							break;

						case "1":
							temp = true;
							break;

						default:
						{
							if (string.Compare(str, "true", StringComparison.OrdinalIgnoreCase) == 0 ||
								string.Compare(str, "on", StringComparison.OrdinalIgnoreCase) == 0)
							{
								temp = true;
							}
							else if (
								string.Compare(str, "false", StringComparison.OrdinalIgnoreCase) == 0 ||
								string.Compare(str, "off", StringComparison.OrdinalIgnoreCase) == 0)
							{
								temp = false;
							}
							else
							{
								return false;
							}

							break;
						}
					}

					retVal = temp;
					return true;
				}

				default:
					return false;
			}
		}

		public bool GetValue(string valueName, ref string retVal)
		{
			object value = TryGetValueForName(valueName);

			return ParseValue(value, ref retVal);
		}
		private bool ParseValue(object value, ref string retVal)
		{
			if (value == null)
				return false;

			switch (Type.GetTypeCode(value.GetType()))
			{
				case TypeCode.String:
					retVal = (string)value;
					return true;

				default:
					return false;
			}
		}

		public bool GetValue(string valueName, ref int retVal)
		{
			object value = TryGetValueForName(valueName);

			return ParseValue(value, ref retVal, valueName);
		}
		public bool GetValue(string valueName, ref byte retVal)
		{
			int integer = 0;
			bool parsed = GetValue(valueName, ref integer);
			if (parsed)
				retVal = (byte)integer;
			return parsed;
		}
		private bool ParseValue(object value, ref int retVal, string valueName = null)
		{
			if (value == null)
				return false;

			switch (Type.GetTypeCode(value.GetType()))
			{
				case TypeCode.Boolean:
					retVal = (bool)value ? 1 : 0;
					return true;

				case TypeCode.Single:
					retVal = (int)(float)value;
					return true;
				case TypeCode.Double:
					retVal = (int)(double)value;
					return true;
				case TypeCode.Int64:
					retVal = (int)(long)value;
					return true;

				case TypeCode.String:
				{
					int tryValue;
					if (!int.TryParse((string)value, out tryValue) && valueName != null)
					{
						Debug.Trace.IO.TraceDataSansId(System.Diagnostics.TraceEventType.Error, string.Format(
							"Failed parsing {0} value '{1}' as an {2}",
							valueName, value, "int"));
						return false;
					}
					retVal = tryValue;

					return true;
				}

				default:
					return false;
			}
		}

		public bool GetValue(string valueName, ref long retVal)
		{
			object value = TryGetValueForName(valueName);

			return ParseValue(value, ref retVal, valueName);
		}
		private bool ParseValue(object value, ref long retVal, string valueName = null)
		{
			if (value == null)
				return false;

			switch (Type.GetTypeCode(value.GetType()))
			{
				case TypeCode.Boolean:
					retVal = (bool)value ? 1 : 0;
					return true;

				case TypeCode.Single:
					retVal = (int)(float)value;
					return true;
				case TypeCode.Double:
					retVal = (int)(double)value;
					return true;
				case TypeCode.Int64:
					retVal = (int)(long)value;
					return true;

				case TypeCode.String:
				{
					long tryValue;
					if (!long.TryParse((string)value, out tryValue) && valueName != null)
					{
						Debug.Trace.IO.TraceDataSansId(System.Diagnostics.TraceEventType.Error, string.Format(
							"Failed parsing {0} value '{1}' as an {2}",
							valueName, value, "long"));
						return false;
					}
					retVal = tryValue;

					return true;
				}

				default:
					return false;
			}
		}

		public bool GetValue(string valueName, ref float retVal)
		{
			double doubleValue = 0;
			if (!GetValue(valueName, ref doubleValue))
				return false;

			retVal = (float)doubleValue;
			return true;
		}
		private bool ParseValue(object value, ref float retVal
			, string valueName = null)
		{
			double doubleValue = 0;
			if (!ParseValue(value, ref doubleValue, valueName))
				return false;

			retVal = (float)doubleValue;
			return true;
		}

		public bool GetValue(string valueName, ref double retVal)
		{
			object value = TryGetValueForName(valueName);

			return ParseValue(value, ref retVal, valueName);
		}
		private bool ParseValue(object value, ref double retVal
			, string valueName = null)
		{
			if (value == null)
				return false;

			switch (Type.GetTypeCode(value.GetType()))
			{
				case TypeCode.Boolean:
					retVal = (bool)value ? 1 : 0;
					return true;

				case TypeCode.Single:
					retVal = (float)value;
					return true;
				case TypeCode.Double:
					retVal = (double)value;
					return true;
				case TypeCode.Int64:
					retVal = (long)value;
					return true;

				case TypeCode.String:
				{
					double tryValue;
					if (!double.TryParse((string)value, out tryValue) && valueName != null)
					{
						Debug.Trace.IO.TraceDataSansId(System.Diagnostics.TraceEventType.Error, string.Format(
							"Failed parsing {0} value '{1}' as an {2}",
							valueName, value, "double"));
						return false;
					}
					retVal = tryValue;

					return true;
				}

				default:
					return false;
			}
		}

		public bool GetEnumValue<TEnum>(string valueName, ref TEnum retVal)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			object value = TryGetValueForName(valueName);
			if (value == null)
				return false;

			switch (Type.GetTypeCode(value.GetType()))
			{
				case TypeCode.String:
				{
					if (!Util.TryParseEnumOpt((string)value, ref retVal))
					{
						Debug.Trace.IO.TraceDataSansId(System.Diagnostics.TraceEventType.Error, string.Format(
							"'{0}' couldn't be parsed for enum named '{1}' (type={2}). Checking spelling",
							value, valueName, typeof(TEnum)));
						return false;
					}

					return true;
				}

				default:
					return false;
			}
		}

		/// <typeparam name="TEnum">Members should be bit indices, not literal flag values</typeparam>
		/// <param name="valueSeperator">regex pattern used to seperate values</param>
		public bool? GetFlagsValue<TEnum>(string valueName, ref BitVector32 retVal
			, string valueSeperator = ",", bool logFailures = true)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			object value = TryGetValueForName(valueName);
			if (value == null)
				return null;

			bool? result = null;

			switch (Type.GetTypeCode(value.GetType()))
			{
				case TypeCode.String:
					result = retVal.TryParseFlags<TEnum>((string)value, valueSeperator, logFailures ? JsonNodeGlobals.gErrorsOutputList : null);
					break;

				case TypeCode.Object:
				{
					var objList = value as List<object>;
					if (objList == null)
						return false;

					result = retVal.TryParseFlags<TEnum>(objList.OfType<string>(), logFailures ? JsonNodeGlobals.gErrorsOutputList : null);
					break;
				}

				default:
					return false;
			}

			if (logFailures && JsonNodeGlobals.gErrorsOutputList.Count > 0)
			{
				Debug.Trace.IO.TraceDataSansId(System.Diagnostics.TraceEventType.Error, string.Format(
					"Failed parsing {0} value '{1}' as an {2}:\n{3}",
					valueName, value, "FLAGS", JsonNodeGlobals.gErrorsOutputList.Join("\n")));
				JsonNodeGlobals.gErrorsOutputList.Clear();
			}

			return result;
		}
		/// <typeparam name="TEnum">Members should be bit indices, not literal flag values</typeparam>
		/// <param name="valueSeperator">regex pattern used to seperate values</param>
		public bool? GetFlagsValue<TEnum>(string valueName, ref BitVector64 retVal
			, string valueSeperator = ",", bool logFailures = true)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			object value = TryGetValueForName(valueName);
			if (value == null)
				return null;

			bool? result = null;

			switch (Type.GetTypeCode(value.GetType()))
			{
				case TypeCode.String:
					result = retVal.TryParseFlags<TEnum>((string)value, valueSeperator, logFailures ? JsonNodeGlobals.gErrorsOutputList : null);
					break;

				case TypeCode.Object:
				{
					var objList = value as List<object>;
					if (objList == null)
						return false;

					result = retVal.TryParseFlags<TEnum>(objList.OfType<string>(), logFailures ? JsonNodeGlobals.gErrorsOutputList : null);
					break;
				}

				default:
					return false;
			}

			if (logFailures && JsonNodeGlobals.gErrorsOutputList.Count > 0)
			{
				Debug.Trace.IO.TraceDataSansId(System.Diagnostics.TraceEventType.Error, string.Format(
					"Failed parsing {0} value '{1}' as an {2}:\n{3}",
					valueName, value, "FLAGS", JsonNodeGlobals.gErrorsOutputList.Join("\n")));
				JsonNodeGlobals.gErrorsOutputList.Clear();
			}

			return result;
		}

		public bool GetValue(string valueName, List<string> list
			, bool sort = false, string valueSeperator = ",")
		{
			object value = TryGetValueForName(valueName);
			if (value == null)
				return false;

			switch (Type.GetTypeCode(value.GetType()))
			{
				case TypeCode.String:
					return Util.ParseStringList((string)value, list, sort, valueSeperator);

				case TypeCode.Object:
				{
					var objList = value as List<object>;
					if (objList == null)
						return false;

					return Util.ParseStringList(objList.OfType<string>(), list, sort);
				}

				default:
					return false;
			}
		}

		public bool GetPodValues<T>(string valueName, IList<T> list
			, string valueSeperator = ",", bool clearFirst = true)
		{
			object value = TryGetValueForName(valueName);
			if (value == null)
				return false;

			var objList = value as List<object>;
			if (objList == null)
				return false;

			if (clearFirst)
				list.Clear();

			var type = typeof(T);
			var typeCode = Type.GetTypeCode(type);

			foreach (object o in objList)
			{
				object parsedValue = null;
				switch (typeCode)
				{
					case TypeCode.Boolean:
					{
						bool boolean = false;
						if (ParseValue(o, ref boolean))
							parsedValue = Convert.ChangeType(boolean, typeCode);
						break;
					}

					case TypeCode.SByte:
					case TypeCode.Int16:
					case TypeCode.Int32:
					{
						int integer = 0;
						if (ParseValue(o, ref integer))
							parsedValue = Convert.ChangeType(integer, typeCode);
						break;
					}

					case TypeCode.Int64:
					{
						long integer = 0;
						if (ParseValue(o, ref integer))
							parsedValue = Convert.ChangeType(integer, typeCode);
						break;
					}

					case TypeCode.Single:
					{
						float real = 0;
						if (ParseValue(o, ref real))
							parsedValue = Convert.ChangeType(real, typeCode);
						break;
					}

					case TypeCode.Double:
					{
						double real = 0;
						if (ParseValue(o, ref real))
							parsedValue = Convert.ChangeType(real, typeCode);
						break;
					}

					case TypeCode.String:
					{
						string str = null;
						if (ParseValue(o, ref str))
							parsedValue = Convert.ChangeType(str, typeCode);
						break;
					}

					default:
					{
						Debug.Trace.IO.TraceDataSansId(System.Diagnostics.TraceEventType.Error, string.Format(
							"We currently don't handle parsing JSON data into lists of {0} types (Type={1})",
							typeCode, type));
						continue;
					}
				}

				if (parsedValue != null)
				{
					list.Add((T)parsedValue);
				}
			}

			return true;
		}

		public bool GetRangeValues(string valueName, ref int min, ref int max)
		{
			object value = TryGetValueForName(valueName);
			if (value == null)
				return false;

			switch (Type.GetTypeCode(value.GetType()))
			{
				case TypeCode.Object:
				{
					var objList = value as List<object>;
					if (objList == null)
						return false;

					if (objList.Count == 1)
					{
						bool success = ParseValue(objList[0], ref min, valueName);
						if (success)
							max = min;

						return success;
					}

					int tempMin = 0, tempMax = 0;
					if (objList.Count < 2 ||
						!ParseValue(objList[0], ref tempMin, valueName) ||
						!ParseValue(objList[1], ref tempMax, valueName))
						return false;

					min = tempMin;
					max = tempMax;
					return true;
				}

				default:
				{
					bool success = ParseValue(value, ref min, valueName);
					if (success)
						max = min;

					return success;
				}
			}
		}

		public bool GetRangeValues(string valueName, ref float min, ref float max)
		{
			object value = TryGetValueForName(valueName);
			if (value == null)
				return false;

			switch (Type.GetTypeCode(value.GetType()))
			{
				case TypeCode.Object:
				{
					var objList = value as List<object>;
					if (objList == null)
						return false;

					if (objList.Count == 1)
					{
						bool success = ParseValue(objList[0], ref min, valueName);
						if (success)
							max = min;

						return success;
					}

					float tempMin = 0, tempMax = 0;
					if (objList.Count < 2 ||
						!ParseValue(objList[0], ref tempMin, valueName) ||
						!ParseValue(objList[1], ref tempMax, valueName))
						return false;

					min = tempMin;
					max = tempMax;
					return true;
				}

				default:
				{
					bool success = ParseValue(value, ref min, valueName);
					if (success)
						max = min;

					return success;
				}
			}
		}
		#endregion
	};
}
