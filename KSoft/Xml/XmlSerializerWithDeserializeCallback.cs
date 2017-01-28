using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Xml
{
	public sealed class XmlSerializerWithDeserializeCallback
		: XmlSerializer
	{
		#region Constructors
		public XmlSerializerWithDeserializeCallback(Type type)
			: base(type)
        {
        }

        public XmlSerializerWithDeserializeCallback(XmlTypeMapping xmlTypeMapping)
			: base(xmlTypeMapping)
        {
        }

        public XmlSerializerWithDeserializeCallback(Type type, string defaultNamespace)
			: base(type, defaultNamespace)
        {
        }

        public XmlSerializerWithDeserializeCallback(Type type, Type[] extraTypes)
			: base(type, extraTypes)
        {
        }

        public XmlSerializerWithDeserializeCallback(Type type, XmlAttributeOverrides overrides)
			: base(type, overrides)
        {
        }

        public XmlSerializerWithDeserializeCallback(Type type, XmlRootAttribute root)
			: base(type, root)
        {
        }

		public XmlSerializerWithDeserializeCallback(Type type, XmlAttributeOverrides overrides, Type[] extraTypes,
            XmlRootAttribute root, string defaultNamespace)
			: base(type, overrides, extraTypes, root, defaultNamespace)
        {
        }

		public XmlSerializerWithDeserializeCallback(Type type, XmlAttributeOverrides overrides, Type[] extraTypes,
            XmlRootAttribute root, string defaultNamespace, string location)
            : base(type, overrides, extraTypes, root, defaultNamespace, location)
        {
        }
		#endregion

		public new object Deserialize(Stream stream)
        {
            var result = base.Deserialize(stream);

            CheckForDeserializationCallbacks(result);

            return result;
        }

        public new object Deserialize(TextReader textReader)
        {
            var result = base.Deserialize(textReader);

            CheckForDeserializationCallbacks(result);

            return result;
        }

        public new object Deserialize(XmlReader xmlReader)
        {
            var result = base.Deserialize(xmlReader);

            CheckForDeserializationCallbacks(result);

            return result;
        }

        public new object Deserialize(XmlSerializationReader reader)
        {
            var result = base.Deserialize(reader);

            CheckForDeserializationCallbacks(result);

            return result;
        }

        public new object Deserialize(XmlReader xmlReader, string encodingStyle)
        {
            var result = base.Deserialize(xmlReader, encodingStyle);

            CheckForDeserializationCallbacks(result);

            return result;
        }

        public new object Deserialize(XmlReader xmlReader, XmlDeserializationEvents events)
        {
            var result = base.Deserialize(xmlReader, events);

            CheckForDeserializationCallbacks(result);

            return result;
        }

        public new object Deserialize(XmlReader xmlReader, string encodingStyle, XmlDeserializationEvents events)
        {
            var result = base.Deserialize(xmlReader, encodingStyle, events);

            CheckForDeserializationCallbacks(result);

            return result;
        }

		public bool DontRecursivelyCheckForDeserializationCallbacks { get; set; }

        private void CheckForDeserializationCallbacks(object deserializedObject)
        {
			var deserializedObjectType = deserializedObject.GetType();
			// due to boxing, invoking the callback won't modify the original object
			if (deserializedObjectType.IsValueType)
				return;

            var deserializationCallback = deserializedObject as IDeserializationCallback;

            if (deserializationCallback != null)
            {
                deserializationCallback.OnDeserialization(this);
				deserializedObject = deserializationCallback;
            }

			if (DontRecursivelyCheckForDeserializationCallbacks)
				return;

			var properties = deserializedObjectType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var enumerableGenericType = typeof(IEnumerable<>);

            foreach (var propertyInfo in properties)
            {
				var interfaceType = propertyInfo.PropertyType.GetInterface(enumerableGenericType.FullName);
				if (interfaceType != null)
                {
					if (!interfaceType.GenericTypeArguments[0].IsValueType)
						continue;

                    var collection = propertyInfo.GetValue(deserializedObject) as IEnumerable;

                    if (collection != null)
                    {
                        foreach (var item in collection)
                        {
                            CheckForDeserializationCallbacks(item);
                        }
                    }
                }
                else
                {
                    CheckForDeserializationCallbacks(propertyInfo.GetValue(deserializedObject));
                }
            }
        }
	}
}