/*
 * Copyright (c) 2013 Calvin Rien
 *
 * Based on the JSON parser by Patrick van Bergen
 * http://techblog.procurios.nl/k/618/news/view/14605/14863/How-do-I-write-my-own-parser-for-JSON.html
 *
 * Simplified it so that it doesn't throw exceptions
 * and can be used in Unity iPhone with maximum code stripping.
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 * Pulled from: https://gist.github.com/darktable/1411710
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace MiniJSON {
    // Example usage:
    //
    //  using UnityEngine;
    //  using System.Collections;
    //  using System.Collections.Generic;
    //  using MiniJSON;
    //
    //  public class MiniJSONTest : MonoBehaviour {
    //      void Start () {
    //          var jsonString = "{ \"array\": [1.44,2,3], " +
    //                          "\"object\": {\"key1\":\"value1\", \"key2\":256}, " +
    //                          "\"string\": \"The quick brown fox \\\"jumps\\\" over the lazy dog \", " +
    //                          "\"unicode\": \"\\u3041 Men\u00fa sesi\u00f3n\", " +
    //                          "\"int\": 65536, " +
    //                          "\"float\": 3.1415926, " +
    //                          "\"bool\": true, " +
    //                          "\"null\": null }";
    //
    //          var dict = Json.Deserialize(jsonString) as Dictionary<string,object>;
    //
    //          Debug.Log("deserialized: " + dict.GetType());
    //          Debug.Log("dict['array'][0]: " + ((List<object>) dict["array"])[0]);
    //          Debug.Log("dict['string']: " + (string) dict["string"]);
    //          Debug.Log("dict['float']: " + (double) dict["float"]); // floats come out as doubles
    //          Debug.Log("dict['int']: " + (long) dict["int"]); // ints come out as longs
    //          Debug.Log("dict['unicode']: " + (string) dict["unicode"]);
    //
    //          var str = Json.Serialize(dict);
    //
    //          Debug.Log("serialized: " + str);
    //      }
    //  }

    /// <summary>
    /// This class encodes and decodes JSON strings.
    /// Spec. details, see http://www.json.org/
    ///
    /// JSON uses Arrays and Objects. These correspond here to the datatypes IList and IDictionary.
    /// All numbers are parsed to doubles.
    /// </summary>
    public static class Json {
        public static string PrettyPrintSpace = "\t";

        private static object ConvertInt64(Type returnType, long number)
        {
            switch (Type.GetTypeCode(returnType))
            {
                case TypeCode.Char:
                    return (char)number;
                case TypeCode.SByte:
                    return (sbyte)number;
                case TypeCode.Byte:
                    return (byte)number;
                case TypeCode.Int16:
                    return (short)number;
                case TypeCode.UInt16:
                    return (ushort)number;
                case TypeCode.Int32:
                    return (int)number;
                case TypeCode.UInt32:
                    return (uint)number;
                case TypeCode.Int64:
                    return number;
                case TypeCode.UInt64:
                    return (ulong)number;
                case TypeCode.Single:
                    return (float)number;
                case TypeCode.Double:
                    return (double)number;

                default:
                    return null;
            }
        }

        private static object ConvertDouble(Type returnType, double number)
        {
            switch (Type.GetTypeCode(returnType))
            {
                case TypeCode.Char:
                    return (char)number;
                case TypeCode.SByte:
                    return (sbyte)number;
                case TypeCode.Byte:
                    return (byte)number;
                case TypeCode.Int16:
                    return (short)number;
                case TypeCode.UInt16:
                    return (ushort)number;
                case TypeCode.Int32:
                    return (int)number;
                case TypeCode.UInt32:
                    return (uint)number;
                case TypeCode.Int64:
                    return (long)number;
                case TypeCode.UInt64:
                    return (ulong)number;
                case TypeCode.Single:
                    return (float)number;
                case TypeCode.Double:
                    return number;

                default:
                    return null;
            }
        }

        private static bool IsNumericType(Type returnType)
        {
            if (returnType == null)
                return false;

            var typeCode = Type.GetTypeCode(returnType);

            return typeCode >= TypeCode.SByte && typeCode <= TypeCode.Double;
        }

        #region GetValue
        public static T GetValue<T>(object jsonObject, string[] keyPath, T defaultValue = default(T))
        {
            if (keyPath.Length == 0)
            {
                return defaultValue;
            }

            for (var i = 0; i < keyPath.Length - 1; i++)
            {
                jsonObject = GetValue<object>(jsonObject, keyPath[i]);
            }

            return GetValue<T>(jsonObject, keyPath[keyPath.Length - 1], defaultValue);
        }

        public static object GetValue(object jsonObject, string key, object defaultValue = null)
        {
            return GetValue<object>(jsonObject, key, defaultValue);
        }

        public static T GetValue<T>(object jsonObject, string key, T defaultValue = default(T))
        {
            var dict = jsonObject as Dictionary<string, object>;
            if (dict == null)
            {
                return defaultValue;
            }

            object result;
            if (!dict.TryGetValue(key, out result))
            {
                return defaultValue;
            }

            var returnType = typeof(T);

            if (IsNumericType(returnType))
            {
                if (result is double)
                {
                    var obj = ConvertDouble(returnType, (double)result);
                    if (obj == null)
                        return defaultValue;

                    return (T)obj;
                }
                else if (result is long)
                {
                    var obj = ConvertInt64(returnType, (long)result);
                    if (obj == null)
                        return defaultValue;

                    return (T)obj;
                }

                return default(T);
            }

            if (returnType == typeof(string) && !(result is string))
            {
                var canConvertToString = IsNumericType(result.GetType());
                if (canConvertToString)
                {
                    result = result.ToString();
                }
            }

            return (T)result;
        }
        #endregion

        #region SetValue
        public static void SetValue(object jsonObject, string[] keyPath, object value)
        {
            for (int i = 0; i < keyPath.Length - 1; i++)
            {
                var next = Json.GetValue(jsonObject, keyPath[i]);
                if (next == null)
                {
                    next = new Dictionary<string, object>();
                    Json.SetValue(jsonObject, keyPath[i], next);
                }
                jsonObject = next;
            }

            Json.SetValue(jsonObject, keyPath[keyPath.Length - 1], value);
        }

        public static void SetValue(object jsonObject, string key, object value)
        {
            var dict = jsonObject as Dictionary<string, object>;
            if (dict == null)
                return;

            if (key.IndexOf('.') != -1)
            {
                Json.SetValue(dict, key.Split('.'), value);
            }
            else
            {
                dict[key] = value;
            }
        }
        #endregion

        /// <summary>
        /// Parses the string json into a value
        /// </summary>
        /// <param name="json">A JSON string.</param>
        /// <returns>An List&lt;object&gt;, a Dictionary&lt;string, object&gt;, a double, an integer,a string, null, true, or false</returns>
        public static object Deserialize(string json) {
            // save the string for debug information
            if (json == null) {
                return null;
            }

            return Parser.Parse(json);
        }

        sealed class Parser : IDisposable {
            const string WORD_BREAK = "{}[],:\"";

            public static bool IsWordBreak(char c) {
                return Char.IsWhiteSpace(c) || WORD_BREAK.IndexOf(c) != -1;
            }

            enum TOKEN {
                NONE,
                CURLY_OPEN,
                CURLY_CLOSE,
                SQUARED_OPEN,
                SQUARED_CLOSE,
                COLON,
                COMMA,
                STRING,
                NUMBER,
                TRUE,
                FALSE,
                NULL
            };

            StringReader json;

            Parser(string jsonString) {
                json = new StringReader(jsonString);
            }

            public static object Parse(string jsonString) {
                using (var instance = new Parser(jsonString)) {
                    return instance.ParseValue();
                }
            }

            public void Dispose() {
                json.Dispose();
                json = null;
            }

            Dictionary<string, object> ParseObject() {
                Dictionary<string, object> table = new Dictionary<string, object>();

                // ditch opening brace
                json.Read();

                // {
                while (true) {
                    switch (NextToken) {
                    case TOKEN.NONE:
                        return null;
                    case TOKEN.COMMA:
                        continue;
                    case TOKEN.CURLY_CLOSE:
                        return table;
                    case TOKEN.STRING:
                        // name
                        string name = ParseString();
                        if (name == null) {
                            return null;
                        }

                        // :
                        if (NextToken != TOKEN.COLON) {
                            return null;
                        }
                        // ditch the colon
                        json.Read();

                        // value
                        TOKEN valueToken = NextToken;
                         object value = ParseByToken(valueToken);
                         if (value == null && valueToken != TOKEN.NULL)
                             return null;
                         table[name] = value;
                        break;
                    default:
                        return null;
                    }
                }
            }

            List<object> ParseArray() {
                // KM00: changed this method to handle invalid arrays
                // see: https://gist.github.com/darktable/1411710/16b47b4865745c4f6278e2e60d2cda53b84447d3
                // "MiniJSON causes an OutOfMemoryException..."
                // but also had to change the way 'array' is allocated

                // KM00 start
                List<object> array = null;// = new List<object>();
                // KM00 end

                // ditch opening bracket
                json.Read();

                // [
                var parsing = true;
                while (parsing) {
                    TOKEN nextToken = NextToken;

                    switch (nextToken) {
                    case TOKEN.NONE:
                        return null;
                    case TOKEN.COMMA:
                        continue;
                    case TOKEN.SQUARED_CLOSE:
                        parsing = false;
                        break;
                    // KM00 start
                    case TOKEN.COLON:
                        json.Read(); //invalid array: consume colon to prevent infinite loop
                        break;
                    // KM00 end
                    default:
                        object value = ParseByToken(nextToken);
                        if (value == null && nextToken != TOKEN.NULL)
                            return null;

                            // KM00 start
                            if (array == null)
                            array = new List<object>();
                        // KM00 end


                        array.Add(value);
                        break;
                    }
                }

                // KM00 start
                if (array == null)
                    array = new List<object>();
                // KM00 end

                return array;
            }

            object ParseValue() {
                TOKEN nextToken = NextToken;
                return ParseByToken(nextToken);
            }

            object ParseByToken(TOKEN token) {
                switch (token) {
                case TOKEN.STRING:
                    return ParseString();
                case TOKEN.NUMBER:
                    return ParseNumber();
                case TOKEN.CURLY_OPEN:
                    return ParseObject();
                case TOKEN.SQUARED_OPEN:
                    return ParseArray();
                case TOKEN.TRUE:
                    return true;
                case TOKEN.FALSE:
                    return false;
                case TOKEN.NULL:
                    return null;
                default:
                    return null;
                }
            }

            // KM00 start
            private StringBuilder mParseStringBuffer;
            private char[] mParseStringHexBuffer;
            private bool IsHexDigit(char c)
            {
                return
                    (c >= '0' && c <= '9') ||
                    (c >= 'A' && c <= 'F') ||
                    (c >= 'a' && c <= 'f')
                    ;
            }
            // KM00 end
            string ParseString() {
                // KM00 start
                if (mParseStringBuffer == null)
                    mParseStringBuffer = new StringBuilder();
                else
                    mParseStringBuffer.Length = 0;

                StringBuilder s = mParseStringBuffer;
                // KM00 end
                char c;

                // ditch opening quote
                json.Read();

                bool parsing = true;
                while (parsing) {

                    if (json.Peek() == -1) {
                        parsing = false;
                        break;
                    }

                    c = NextChar;
                    switch (c) {
                    case '"':
                        parsing = false;
                        break;
                    case '\\':
                        if (json.Peek() == -1) {
                            parsing = false;
                            break;
                        }

                        c = NextChar;
                        switch (c) {
                        case '"':
                        case '\\':
                        case '/':
                            s.Append(c);
                            break;
                        case 'b':
                            s.Append('\b');
                            break;
                        case 'f':
                            s.Append('\f');
                            break;
                        case 'n':
                            s.Append('\n');
                            break;
                        case 'r':
                            s.Append('\r');
                            break;
                        case 't':
                            s.Append('\t');
                            break;
                        case 'u':
                            // KM00 start
                            if (mParseStringHexBuffer == null)
                                mParseStringHexBuffer = new char[4];

                            var hex = mParseStringHexBuffer;
                            // KM00 end

                            for (int i=0; i< 4; i++) {
                                hex[i] = NextChar;
                                if (!IsHexDigit(hex[i]))
                                    return null;
                            }

                            s.Append((char) Convert.ToInt32(new string(hex), 16));
                            break;
                        }
                        break;
                    default:
                        s.Append(c);
                        break;
                    }
                }

                return s.ToString();
            }

            object ParseNumber() {
                string number = NextWord;

                // Allow scientific notation in floating point numbers by @shiwano
                // https://github.com/Jackyjjc/MiniJSON.cs/commit/6de00beb134bbab9d873033a48b32e4067ed0c25
                if (number.IndexOf('.') == -1 && number.IndexOf('E') == -1 && number.IndexOf('e') == -1) {
                    long parsedInt;
                    Int64.TryParse(number, out parsedInt);
                    return parsedInt;
                }

                double parsedDouble;
                // KM00 start
                KSoft.Numbers.DoubleTryParseInvariant(number, out parsedDouble);
                // KM00 end
                return parsedDouble;
            }

            void EatWhitespace() {
                // KM00 start
                if (json.Peek () == -1)
                    return;
                // KM00 end

                while (Char.IsWhiteSpace(PeekChar)) {
                    json.Read();

                    if (json.Peek() == -1) {
                        break;
                    }
                }
            }

            char PeekChar {
                get {
                    return Convert.ToChar(json.Peek());
                }
            }

            char NextChar {
                get {
                    return Convert.ToChar(json.Read());
                }
            }

            // KM00 start
            private StringBuilder mNextWordStringBuffer;
            // KM00 end
            string NextWord {
                get {
                    // KM00 start
                    if (mNextWordStringBuffer == null)
                        mNextWordStringBuffer = new StringBuilder();
                    else
                        mNextWordStringBuffer.Length = 0;

                    StringBuilder word = mNextWordStringBuffer;
                    // KM00 end

                    while (!IsWordBreak(PeekChar)) {
                        word.Append(NextChar);

                        if (json.Peek() == -1) {
                            break;
                        }
                    }

                    return word.ToString();
                }
            }

            TOKEN NextToken {
                get {
                    EatWhitespace();

                    if (json.Peek() == -1) {
                        return TOKEN.NONE;
                    }

                    switch (PeekChar) {
                    case '{':
                        return TOKEN.CURLY_OPEN;
                    case '}':
                        json.Read();
                        return TOKEN.CURLY_CLOSE;
                    case '[':
                        return TOKEN.SQUARED_OPEN;
                    case ']':
                        json.Read();
                        return TOKEN.SQUARED_CLOSE;
                    case ',':
                        json.Read();
                        return TOKEN.COMMA;
                    case '"':
                        return TOKEN.STRING;
                    case ':':
                        return TOKEN.COLON;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '-':
                        return TOKEN.NUMBER;
                    }

                    switch (NextWord) {
                    case "false":
                        return TOKEN.FALSE;
                    case "true":
                        return TOKEN.TRUE;
                    case "null":
                        return TOKEN.NULL;
                    }

                    return TOKEN.NONE;
                }
            }
        }

        /// <summary>
        /// Converts a IDictionary / IList object or a simple type (string, int, etc.) into a JSON string
        /// </summary>
        /// <param name="json">A Dictionary&lt;string, object&gt; / List&lt;object&gt;</param>
        /// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
        public static string Serialize(object obj, bool prettyPrint = false, int numPrettyPrintLevels = 0) {
            return Serializer.Serialize(obj);
        }

        sealed class Serializer {
            StringBuilder builder;
            bool prettyPrint;
            int numPrettyPrintLevels;

            Serializer() {
                builder = new StringBuilder();
            }

            public static string Serialize(object obj, bool prettyPrint = false, int numPrettyPrintLevels = 0) {
                var instance = new Serializer();

                instance.prettyPrint = prettyPrint;
                instance.numPrettyPrintLevels = numPrettyPrintLevels;
                instance.SerializeValue(obj);

                return instance.builder.ToString();
            }

            void SerializeValue(object value, int level = 0) {
                IList asList;
                IDictionary asDict;
                string asStr;

                if (value == null) {
                    builder.Append("null");
                } else if ((asStr = value as string) != null) {
                    SerializeString(asStr);
                } else if (value is bool) {
                    builder.Append((bool) value ? "true" : "false");
                } else if ((asList = value as IList) != null) {
                    SerializeArray(asList, level);
                } else if ((asDict = value as IDictionary) != null) {
                    SerializeObject(asDict, level);
                } else if (value is char) {
                    SerializeString(new string((char) value, 1));
                } else {
                    SerializeOther(value);
                }
            }

            void SerializeObject(IDictionary obj, int level = 0) {
                bool first = true;

                builder.Append('{');

                foreach (object e in obj.Keys) {
                    if (!first) {
                        builder.Append(',');
                    }

                    if (numPrettyPrintLevels <= 0 || (level + 1) <= numPrettyPrintLevels)
                    {
                        PrettyPrintNewLine(level + 1);
                    }

                    SerializeString(e.ToString());
                    builder.Append(':');

                    SerializeValue(obj[e], level + 1);

                    first = false;
                }

                if (!first && (numPrettyPrintLevels <= 0 || level < numPrettyPrintLevels))
                {
                    PrettyPrintNewLine(level);
                }

                builder.Append('}');
            }

            void SerializeArray(IList anArray, int level = 0) {
                builder.Append('[');

                bool first = true;

                foreach (object obj in anArray) {
                    if (!first) {
                        builder.Append(',');
                    }

                    if (numPrettyPrintLevels <= 0 || (level + 1) <= numPrettyPrintLevels)
                    {
                        PrettyPrintNewLine(level + 1);
                    }

                    SerializeValue(obj, level + 1);

                    first = false;
                }

                if (!first && (numPrettyPrintLevels <= 0 || level < numPrettyPrintLevels))
                {
                    PrettyPrintNewLine(level);
                }

                builder.Append(']');
            }

            void SerializeString(string str) {
                builder.Append('\"');

                foreach (var c in str) {
                    switch (c) {
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '\b':
                        builder.Append("\\b");
                        break;
                    case '\f':
                        builder.Append("\\f");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        int codepoint = Convert.ToInt32(c);
                        if ((codepoint >= 32) && (codepoint <= 126)) {
                            builder.Append(c);
                        } else {
                            builder.Append("\\u");
                            builder.Append(codepoint.ToString("x4"));
                        }
                        break;
                    }
                }

                builder.Append('\"');
            }

            void SerializeOther(object value) {
                // NOTE: decimals lose precision during serialization.
                // They always have, I'm just letting you know.
                // Previously floats and doubles lost precision too.
                if (value is float) {
                    // KM00 added CultureInfo.InvariantCulture
                    builder.Append(((float) value).ToString("R", CultureInfo.InvariantCulture));
                } else if (value is int
                    || value is uint
                    || value is long
                    || value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is ulong) {
                    builder.Append(value);
                } else if (value is double
                    || value is decimal) {
                    // KM00 added CultureInfo.InvariantCulture
                    builder.Append(Convert.ToDouble(value).ToString("R", CultureInfo.InvariantCulture));
                } else {
                    SerializeString(value.ToString());
                }
            }

            void PrettyPrintNewLine(int numSpaces)
            {
                if (!prettyPrint)
                    return;

                builder.Append('\n');
                for (int i = 0; i < numSpaces; i++)
                {
                    builder.Append(Json.PrettyPrintSpace);
                }
            }
        }
    }
}