using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace KSoft.ObjectModel
{
	public abstract class BasicViewModel
		: INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged(
			[System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
		{
			var handler = PropertyChanged;
#pragma warning disable IDE0031 // Use null propagation
			if (handler != null)
			{
				handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
#pragma warning restore IDE0031 // Use null propagation
		}

		protected bool SetFieldVal<T>(ref T field, T value
			, bool overrideChecks = false
			, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
			where T : struct, IEquatable<T>
		{
			return TypeExtensions.SetFieldVal(this, PropertyChanged,
				ref field, value, overrideChecks, propertyName);
		}
		protected bool SetFieldVal<T>(ref T field, T value
			, PropertyChangedEventArgs eventArgs)
			where T : struct, IEquatable<T>
		{
			var handler = PropertyChanged;
			if (field.Equals(value))
			{
				return false;
			}

			field = value;
			handler?.Invoke(this, eventArgs);
			return true;
		}

		protected bool SetFieldEnum<TEnum>(ref TEnum field, TEnum value
			, bool overrideChecks = false
			, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
			where TEnum : struct, Enum
		{
			return TypeExtensions.SetFieldEnum(this, PropertyChanged,
				ref field, value, overrideChecks, propertyName);
		}
		protected bool SetFieldEnum<TEnum>(ref TEnum field, TEnum value
			, PropertyChangedEventArgs eventArgs)
			where TEnum : struct, Enum
		{
			var handler = PropertyChanged;
			if (EqualityComparer<TEnum>.Default.Equals(field, value))
			{
				return false;
			}

			field = value;
			handler?.Invoke(this, eventArgs);
			return true;
		}

		protected bool SetFieldObj<T>(ref T field, T value
			, bool overrideChecks = false
			, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
			where T : class, IEquatable<T>
		{
			return TypeExtensions.SetFieldObj(this, PropertyChanged,
				ref field, value, overrideChecks, propertyName);
		}
		protected bool SetField<T>(ref T field, T value
			, bool overrideChecks = false
			, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
		{
			return TypeExtensions.SetField(this, PropertyChanged,
				ref field, value, overrideChecks, propertyName);
		}
		protected bool SetField<T>(ref T field, T value
			, PropertyChangedEventArgs eventArgs)
		{
			var handler = PropertyChanged;
			if (EqualityComparer<T>.Default.Equals(field, value))
			{
				return false;
			}

			field = value;
			handler?.Invoke(this, eventArgs);
			return true;
		}
		protected bool SetField<T>(ref T field, T value
			, PropertyChangedEventArgs eventArgs
			, bool overrideChecks)
		{
			var handler = PropertyChanged;
			if (!overrideChecks && EqualityComparer<T>.Default.Equals(field, value))
			{
				return false;
			}

			field = value;
			handler?.Invoke(this, eventArgs);
			return true;
		}
	};
}