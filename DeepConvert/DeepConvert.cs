// Copyright (c) 2018-2022, Yves Goergen, https://ygoe.de
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Unclassified.Util
{
	/// <summary>
	/// Converts a data type to another data type, including collections and their items.
	/// </summary>
	public static partial class DeepConvert
	{
		#region Private data

		/// <summary>
		/// All words that are considered boolean false. Anything else is considered true.
		/// </summary>
		private static readonly string[] falseWords = new[]
		{
			"",
			"0",
			"false", "no", "n", "off",   // en
			"falsch", "nein",   // de
			"falso", "no",   // es
			"epätosi", "kyllä",   // fi
			"faux", "non",   // fr
			"hamis",   // hu
			"rangt",   // is
			"falso", "no",   // it
			"onwaar", "nee",   // nl
			"usant", "nei",   // nn, no
			"fałsz", "nie",   // pl
			"falso", "nao", "não",   // pt
			"ложь", "нет",   // ru
			"nie",   // sk
			"falskt", "nej",   // sv
			"sai", "không",   // vi
			"否",   // zh-cn, zh-tw
		};

		/// <summary>
		/// The UNIX epoch time.
		/// </summary>
		private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// An empty settings instance that can be used to avoid allocation.
		/// </summary>
		private static readonly DeepConvertSettings emptySettings = new DeepConvertSettings();

		/// <summary>
		/// A preset settings instance for the invariant culture to avoid allocation.
		/// </summary>
		private static readonly DeepConvertSettings invariantSettings = new DeepConvertSettings { Provider = CultureInfo.InvariantCulture };

		#endregion Private data

		#region Main ChangeType methods

		/// <summary>
		/// Returns an object of the specified type whose value is equivalent to the specified
		/// object.
		/// </summary>
		/// <typeparam name="T">The type to convert the data to.</typeparam>
		/// <param name="value">The data to convert.</param>
		/// <returns>An object whose type is <typeparamref name="T"/> and whose value is equivalent
		///   to <paramref name="value"/>.</returns>
		public static T ChangeType<T>(object value) =>
			(T)ChangeType(value, typeof(T), emptySettings);

		/// <summary>
		/// Returns an object of the specified type whose value is equivalent to the specified
		/// object.
		/// </summary>
		/// <typeparam name="T">The type to convert the data to.</typeparam>
		/// <param name="value">The data to convert.</param>
		/// <param name="settings">The conversion settings.</param>
		/// <returns>An object whose type is <typeparamref name="T"/> and whose value is equivalent
		///   to <paramref name="value"/>.</returns>
		public static T ChangeType<T>(object value, DeepConvertSettings settings) =>
			(T)ChangeType(value, typeof(T), settings);

		/// <summary>
		/// Returns an object of the specified type whose value is equivalent to the specified
		/// object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="destType">The type to convert the data to.</param>
		/// <returns>An object whose type is <paramref name="destType"/> and whose value is
		///   equivalent to <paramref name="value"/>.</returns>
		public static object ChangeType(object value, Type destType) =>
			ChangeType(value, destType, emptySettings);

		/// <summary>
		/// Returns an object of the specified type whose value is equivalent to the specified
		/// object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="destType">The type to convert the data to.</param>
		/// <param name="settings">The conversion settings.</param>
		/// <returns>An object whose type is <paramref name="destType"/> and whose value is
		///   equivalent to <paramref name="value"/>.</returns>
		public static object ChangeType(object value, Type destType, DeepConvertSettings settings)
		{
			var provider = settings.Provider;
			if (provider == null)
				provider = CultureInfo.CurrentCulture;

			// Handle null value
			if (value == null)
			{
				if (Nullable.GetUnderlyingType(destType) != null || destType.IsClass)
				{
					return value;
				}
				if (destType.IsEnum)
				{
					return Enum.ToObject(destType, 0);
				}
				throw new InvalidCastException($"The null value cannot be converted into a {destType.FullName}.");
			}

			var srcType = value.GetType();

			// Convert to Nullables
			// (srcType is never Nullable, it's either null or the element type)
			if (Nullable.GetUnderlyingType(destType) != null)
			{
				// Special handling for empty strings
				if (value is string str && str == "")
					return null;

				return ChangeType(value, Nullable.GetUnderlyingType(destType), settings);
			}

			// Atomic types:
			// string, char, byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal,
			// Enum, bool, BigInteger, DateTime, TimeSpan, Guid
			if (srcType == typeof(string) || srcType == typeof(char) ||
				srcType == typeof(byte) || srcType == typeof(sbyte) ||
				srcType == typeof(short) || srcType == typeof(ushort) ||
				srcType == typeof(int) || srcType == typeof(uint) ||
				srcType == typeof(long) || srcType == typeof(ulong) ||
				srcType == typeof(float) || srcType == typeof(double) || srcType == typeof(decimal))
			{
				if (destType == typeof(char) ||
					destType == typeof(byte) || destType == typeof(sbyte) ||
					destType == typeof(short) || destType == typeof(ushort) ||
					destType == typeof(int) || destType == typeof(uint) ||
					destType == typeof(long) || destType == typeof(ulong) ||
					srcType != typeof(string) && (destType == typeof(float) || destType == typeof(double) || destType == typeof(decimal)))
				{
					return Convert.ChangeType(value, destType, provider);
				}
				if (destType == typeof(float))
				{
					return float.Parse((string)value, NumberStyles.Float, provider);
				}
				if (destType == typeof(double))
				{
					return double.Parse((string)value, NumberStyles.Float, provider);
				}
				if (destType == typeof(decimal))
				{
					return decimal.Parse((string)value, NumberStyles.Float, provider);
				}
				if (destType.IsEnum)
				{
					// Convert to enum's numeric type, then explicitly to enum type.
					// This is necessary to further convert to a nullable of the enum.
					object enumNumericValue = Convert.ChangeType(value, Enum.GetUnderlyingType(destType), provider);
					return Enum.ToObject(destType, enumNumericValue);
				}
			}
			if (srcType == typeof(string))
			{
				if (destType == typeof(bool)) return !falseWords.Any(w => string.Equals(w, (string)value, StringComparison.OrdinalIgnoreCase));
				if (destType == typeof(BigInteger)) return BigInteger.Parse((string)value, provider);
				if (destType == typeof(DateTime)) return ToDateTime(value, settings);
				if (destType == typeof(TimeSpan)) return TimeSpan.Parse((string)value, provider);
				if (destType == typeof(Guid)) return Guid.Parse((string)value);
			}
			if (srcType == typeof(char))
			{
				if (destType == typeof(bool)) return !value.Equals((char)0);
				if (destType == typeof(BigInteger)) return new BigInteger((char)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, settings);
				if (destType == typeof(TimeSpan)) return new TimeSpan((char)value);
				// No conversion to Guid
			}
			if (srcType == typeof(byte))
			{
				if (destType == typeof(bool)) return !value.Equals((byte)0);
				if (destType == typeof(BigInteger)) return new BigInteger((byte)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, settings);
				if (destType == typeof(TimeSpan)) return new TimeSpan((byte)value);
				// No conversion to Guid
			}
			if (srcType == typeof(sbyte))
			{
				if (destType == typeof(bool)) return !value.Equals((sbyte)0);
				if (destType == typeof(BigInteger)) return new BigInteger((sbyte)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, settings);
				if (destType == typeof(TimeSpan)) return new TimeSpan((sbyte)value);
				// No conversion to Guid
			}
			if (srcType == typeof(short))
			{
				if (destType == typeof(bool)) return !value.Equals((short)0);
				if (destType == typeof(BigInteger)) return new BigInteger((short)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, settings);
				if (destType == typeof(TimeSpan)) return new TimeSpan((short)value);
				// No conversion to Guid
			}
			if (srcType == typeof(ushort))
			{
				if (destType == typeof(bool)) return !value.Equals((ushort)0);
				if (destType == typeof(BigInteger)) return new BigInteger((ushort)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, settings);
				if (destType == typeof(TimeSpan)) return new TimeSpan((ushort)value);
				// No conversion to Guid
			}
			if (srcType == typeof(int))
			{
				if (destType == typeof(bool)) return !value.Equals(0);
				if (destType == typeof(BigInteger)) return new BigInteger((int)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, settings);
				if (destType == typeof(TimeSpan)) return new TimeSpan((int)value);
				// No conversion to Guid
			}
			if (srcType == typeof(uint))
			{
				if (destType == typeof(bool)) return !value.Equals((uint)0);
				if (destType == typeof(BigInteger)) return new BigInteger((uint)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, settings);
				if (destType == typeof(TimeSpan)) return new TimeSpan((uint)value);
				// No conversion to Guid
			}
			if (srcType == typeof(long))
			{
				if (destType == typeof(bool)) return !value.Equals((long)0);
				if (destType == typeof(BigInteger)) return new BigInteger((long)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, settings);
				if (destType == typeof(TimeSpan)) return new TimeSpan((long)value);
				// No conversion to Guid
			}
			if (srcType == typeof(ulong))
			{
				if (destType == typeof(bool)) return !value.Equals((ulong)0);
				if (destType == typeof(BigInteger)) return new BigInteger((ulong)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, settings);
				if (destType == typeof(TimeSpan)) return new TimeSpan((long)(ulong)value);
				// No conversion to Guid
			}
			if (srcType == typeof(float))
			{
				if (destType == typeof(bool)) return !value.Equals((float)0);
				if (destType == typeof(BigInteger)) return new BigInteger((float)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, settings);
				if (destType == typeof(TimeSpan)) return new TimeSpan((long)(float)value);
				// No conversion to Guid
			}
			if (srcType == typeof(double))
			{
				if (destType == typeof(bool)) return !value.Equals((double)0);
				if (destType == typeof(BigInteger)) return new BigInteger((double)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, settings);
				if (destType == typeof(TimeSpan)) return new TimeSpan((long)(double)value);
				// No conversion to Guid
			}
			if (srcType == typeof(decimal))
			{
				if (destType == typeof(bool)) return !value.Equals((decimal)0);
				if (destType == typeof(BigInteger)) return new BigInteger((decimal)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, settings);
				if (destType == typeof(TimeSpan)) return new TimeSpan((long)(decimal)value);
				// No conversion to Guid
			}

			if (srcType.IsEnum)
			{
				object enumValue = Convert.ChangeType(value, Enum.GetUnderlyingType(srcType), provider);
				if (destType == typeof(char) ||
					destType == typeof(byte) || destType == typeof(sbyte) ||
					destType == typeof(short) || destType == typeof(ushort) ||
					destType == typeof(int) || destType == typeof(uint) ||
					destType == typeof(long) || destType == typeof(ulong) ||
					destType == typeof(float) || destType == typeof(double) || destType == typeof(decimal))
				{
					return Convert.ChangeType(enumValue, destType, provider);
				}
				if (destType.IsEnum)
				{
					object enumNumericValue = Convert.ChangeType(enumValue, Enum.GetUnderlyingType(destType), provider);
					return Enum.ToObject(destType, enumNumericValue);
				}
				if (destType == typeof(bool)) return !enumValue.Equals(0);
				if (destType == typeof(BigInteger)) return new BigInteger(Convert.ToInt64(enumValue));
				if (destType == typeof(DateTime)) return ToDateTime(value, settings);
				if (destType == typeof(TimeSpan)) return new TimeSpan(Convert.ToInt64(enumValue));
				// No conversion to Guid
			}
			if (srcType == typeof(bool))
			{
				if (destType == typeof(string)) return (bool)value ? "true" : "false";
				if (destType == typeof(char)) return (bool)value ? (char)1 : (char)0;
				if (destType == typeof(byte)) return (bool)value ? (byte)1 : (byte)0;
				if (destType == typeof(sbyte)) return (bool)value ? (sbyte)1 : (sbyte)0;
				if (destType == typeof(short)) return (bool)value ? (short)1 : (short)0;
				if (destType == typeof(ushort)) return (bool)value ? (ushort)1 : (ushort)0;
				if (destType == typeof(int)) return (bool)value ? 1 : 0;
				if (destType == typeof(uint)) return (bool)value ? (uint)1 : (uint)0;
				if (destType == typeof(long)) return (bool)value ? (long)1 : (long)0;
				if (destType == typeof(ulong)) return (bool)value ? (ulong)1 : (ulong)0;
				if (destType == typeof(float)) return (bool)value ? (float)1 : (float)0;
				if (destType == typeof(double)) return (bool)value ? (double)1 : (double)0;
				if (destType == typeof(decimal)) return (bool)value ? (decimal)1 : (decimal)0;
				if (destType.IsEnum)
				{
					object enumNumericValue = Convert.ChangeType((bool)value ? 1 : 0, Enum.GetUnderlyingType(destType), provider);
					return Enum.ToObject(destType, enumNumericValue);
				}
				if (destType == typeof(bool)) return value;
				if (destType == typeof(BigInteger)) return (bool)value ? BigInteger.One : BigInteger.Zero;
				if (destType == typeof(DateTime)) return new DateTime((bool)value ? 1 : 0);
				if (destType == typeof(TimeSpan)) return new TimeSpan((bool)value ? 1 : 0);
				// No conversion to Guid
			}
			if (srcType == typeof(BigInteger))
			{
				if (destType == typeof(string) || destType == typeof(char) ||
					destType == typeof(byte) || destType == typeof(sbyte) ||
					destType == typeof(short) || destType == typeof(ushort) ||
					destType == typeof(int) || destType == typeof(uint) ||
					destType == typeof(long) || destType == typeof(ulong) ||
					destType == typeof(float) || destType == typeof(double) || destType == typeof(decimal))
				{
					return Convert.ChangeType(((BigInteger)value).ToString(), destType, provider);
				}
				if (destType.IsEnum)
				{
					object enumNumericValue = Convert.ChangeType(((BigInteger)value).ToString(), Enum.GetUnderlyingType(destType), provider);
					return Enum.ToObject(destType, enumNumericValue);
				}
				if (destType == typeof(bool)) return !((BigInteger)value).IsZero;
				if (destType == typeof(BigInteger)) return value;
				if (destType == typeof(DateTime)) return ToDateTime(value, settings);
				if (destType == typeof(TimeSpan)) return new TimeSpan(Convert.ToInt64(((BigInteger)value).ToString(), provider));
				// No conversion to Guid
			}
			if (srcType == typeof(DateTime))
			{
				if (destType == typeof(string))
				{
					if (!string.IsNullOrEmpty(settings.Format))
					{
						return ((DateTime)value).ToString(settings.Format, provider);
					}
					return ((DateTime)value).ToString(provider);
				}
				if (destType == typeof(char) ||
					destType == typeof(byte) || destType == typeof(sbyte) ||
					destType == typeof(short) || destType == typeof(ushort) ||
					destType == typeof(int) || destType == typeof(uint) ||
					destType == typeof(long) || destType == typeof(ulong) ||
					destType == typeof(float) || destType == typeof(double) || destType == typeof(decimal) ||
					destType == typeof(BigInteger) ||
					destType.IsEnum)
				{
					switch (settings.DateNumericKind)
					{
						case DateNumericKind.None:
						case DateNumericKind.Ticks:
							return ChangeType(((DateTime)value).Ticks, destType, settings);
						case DateNumericKind.UnixSeconds:
							return ChangeType((((DateTime)value).ToUniversalTime() - unixEpoch).TotalSeconds, destType, settings);
						case DateNumericKind.UnixMilliseconds:
							return ChangeType((((DateTime)value).ToUniversalTime() - unixEpoch).TotalMilliseconds, destType, settings);
						default:
							throw new ArgumentException("Unknown date numeric kind.");
					}
				}
				if (destType == typeof(bool)) return ((DateTime)value).Ticks != 0;
				if (destType == typeof(DateTime)) return value;
				if (destType == typeof(TimeSpan)) return ((DateTime)value).TimeOfDay;
				// No conversion to Guid
			}
			if (srcType == typeof(TimeSpan))
			{
				if (destType == typeof(string))
				{
					if (!string.IsNullOrEmpty(settings.Format))
					{
						return ((TimeSpan)value).ToString(settings.Format, provider);
					}
					return ((TimeSpan)value).ToString("c", provider);
				}
				if (destType == typeof(char) ||
					destType == typeof(byte) || destType == typeof(sbyte) ||
					destType == typeof(short) || destType == typeof(ushort) ||
					destType == typeof(int) || destType == typeof(uint) ||
					destType == typeof(long) || destType == typeof(ulong) ||
					destType == typeof(float) || destType == typeof(double) || destType == typeof(decimal) ||
					destType == typeof(BigInteger) ||
					destType.IsEnum)
				{
					switch (settings.DateNumericKind)
					{
						case DateNumericKind.None:
						case DateNumericKind.Ticks:
							return ChangeType(((TimeSpan)value).Ticks, destType, settings);
						case DateNumericKind.UnixSeconds:
							return ChangeType(((TimeSpan)value).TotalSeconds, destType, settings);
						case DateNumericKind.UnixMilliseconds:
							return ChangeType(((TimeSpan)value).TotalMilliseconds, destType, settings);
						default:
							throw new ArgumentException("Unknown date numeric kind.");
					}
					//return Convert.ChangeType(((TimeSpan)value).Ticks, destType);
				}
				if (destType == typeof(bool)) return ((TimeSpan)value).Ticks != 0;
				if (destType == typeof(DateTime)) return ToDateTime(value, settings);
				if (destType == typeof(TimeSpan)) return value;
				// No conversion to Guid
			}
			if (srcType == typeof(Guid))
			{
				//if (destType == typeof(string)) return ((Guid)value).ToString();
				// No conversion to char, byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal,
				// Enum, BigInteger, DateTime and TimeSpan
				if (destType == typeof(Guid)) return value;
				// TODO: Convert with collection of bytes (also in reverse direction)
			}

			// Collection types:
			// Array, List<>, Queue<>, Stack<>, HashSet<>, Tuple<...>, ValueTuple<...>, KeyValuePair<,>,
			// ObservableCollection<>, ConcurrentBag<>, ConcurrentQueue<>, ConcurrentStack<>,
			// any classes implementing: ICollection<> (includes IList<>, ISet<>), IList
			// dictionaries are considered collections of KeyValuePair:
			// Dictionary<,>, ConcurrentDictionary<,>, IDictionary<,>, IDictionary
			// strings are also considered collections of chars, if converting from/to collections

			// Convert from bytes to string
			if (typeof(IEnumerable<byte>).IsAssignableFrom(srcType) &&
				destType == typeof(string))
			{
				var encoding = settings.Encoding ?? Encoding.UTF8;
				var bytes = (IEnumerable<byte>)value;
				return encoding.GetString(bytes.ToArray());
			}

			// Convert between collection types, then recurse into each item
			IEnumerable<object> items = null;
			// Convert from string to bytes
			if (srcType == typeof(string) &&
				typeof(IEnumerable<byte>).IsAssignableFrom(destType))
			{
				var encoding = settings.Encoding ?? Encoding.UTF8;
				items = encoding.GetBytes((string)value).Cast<object>();
			}
			else if (typeof(IEnumerable).IsAssignableFrom(srcType))
			{
				items = ((IEnumerable)value).Cast<object>();
			}
			// TODO: Also consider derived types from these tuples (which may no longer be generic)
			else if (srcType.IsGenericType &&
				(srcType.GetGenericTypeDefinition().FullName.StartsWith("System.Tuple`") ||
				srcType.GetGenericTypeDefinition().FullName.StartsWith("System.ValueTuple`")))
			{
				var list = new List<object>();
				for (int index = 1; index <= 8; index++)
				{
					var prop = srcType.GetProperty("Item" + index);
					if (prop == null) break;
					list.Add(prop.GetValue(value));
				}
				items = list;
			}
			// TODO: Also consider derived types from these KeyValuePairs (which may no longer be generic)
			else if (srcType.IsGenericType && srcType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
			{
				object[] array = new object[2];
				array[0] = srcType.GetProperty("Key").GetValue(value);
				array[1] = srcType.GetProperty("Value").GetValue(value);
				items = array;
			}
			if (items != null)
			{
				if (destType.IsArray)
				{
					var destArray = Array.CreateInstance(destType.GetElementType(), items.Count());
					int index = 0;
					foreach (object item in items.Select(o => ChangeType(o, destType.GetElementType(), settings)))
					{
						destArray.SetValue(item, index++);
					}
					return destArray;
				}
				if (destType.IsGenericType &&
					(destType.GetGenericTypeDefinition() == typeof(List<>) ||
					destType.GetGenericTypeDefinition() == typeof(Queue<>) ||
					destType.GetGenericTypeDefinition() == typeof(Stack<>) ||
					destType.GetGenericTypeDefinition() == typeof(HashSet<>) ||
					destType.GetGenericTypeDefinition() == typeof(ObservableCollection<>) ||
					destType.GetGenericTypeDefinition() == typeof(ConcurrentBag<>) ||
					destType.GetGenericTypeDefinition() == typeof(ConcurrentQueue<>) ||
					destType.GetGenericTypeDefinition() == typeof(ConcurrentStack<>)))
				{
					var destArray = Array.CreateInstance(destType.GenericTypeArguments[0], items.Count());
					int index = 0;
					foreach (object item in items.Select(o => ChangeType(o, destType.GenericTypeArguments[0], settings)))
					{
						destArray.SetValue(item, index++);
					}
					return Activator.CreateInstance(destType, destArray);
				}
				if (destType.IsGenericType &&
					(destType.GetGenericTypeDefinition().FullName.StartsWith("System.Tuple`") ||
					destType.GetGenericTypeDefinition().FullName.StartsWith("System.ValueTuple`") ||
					destType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)))
				{
					return CreateTuple(destType, items, settings);
				}
				var collectionType = destType.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>));
				if (collectionType != null)
				{
					object list = Activator.CreateInstance(destType);
					var add = collectionType.GetMethod("Add");
					foreach (object item in items.Select(o => ChangeType(o, collectionType.GenericTypeArguments[0], settings)))
					{
						add.Invoke(list, new[] { item });
					}
					return list;
				}
				if (typeof(IList).IsAssignableFrom(destType))
				{
					object list = Activator.CreateInstance(destType);
					var add = typeof(IList).GetMethod("Add");
					foreach (object item in items)
					{
						add.Invoke(list, new[] { item });
					}
					return list;
				}
				if (destType == typeof(string))
				{
					var sb = new StringBuilder(items.Count());
					foreach (char item in items.Select(o => ChangeType(o, typeof(char), settings)))
					{
						sb.Append(item);
					}
					return sb.ToString();
				}
			}

			// TODO: Convert between atomic and collection types (needs recursion?) (take care of KeyValuePair (a tuple) <-> Dictionary (a collection))

			// Try IFormattable if srcType implements it
			if (destType == typeof(string) &&
				typeof(IFormattable).IsAssignableFrom(srcType))
			{
				return ((IFormattable)value).ToString(settings.Format, provider);
			}

			// Try IConvertible if srcType implements it
			if (typeof(IConvertible).IsAssignableFrom(srcType))
			{
				return Convert.ChangeType(value, destType, provider);
			}

			// Always convert to an object
			if (destType == typeof(object))
			{
				return value;
			}

			// Try to convert between object and dictionary
			if (typeof(IDictionary).IsAssignableFrom(srcType) &&
				destType.IsClass)
			{
				return ConvertToObject((IDictionary)value, destType, settings);
			}
			if (srcType.IsClass &&
				typeof(IDictionary).IsAssignableFrom(destType))
			{
				var dict = ConvertToDictionary(value);
				return ChangeType(dict, destType, settings);
			}
			if (srcType.IsClass &&
				destType.IsClass)
			{
				var dict = ConvertToDictionary(value);
				return ConvertToObject(dict, destType, settings);
			}

			throw new InvalidCastException($"The value '{value}' ({srcType.FullName}) cannot be converted to {destType.FullName}.");
		}

		#endregion Main ChangeType methods

		#region ChangeTypeInvariant methods

		/// <summary>
		/// Returns an object of the specified type whose value is equivalent to the specified
		/// object, using the invariant culture.
		/// </summary>
		/// <typeparam name="T">The type to convert the data to.</typeparam>
		/// <param name="value">The data to convert.</param>
		/// <returns>An object whose type is <typeparamref name="T"/> and whose value is equivalent
		///   to <paramref name="value"/>.</returns>
		public static T ChangeTypeInvariant<T>(object value) =>
			(T)ChangeType(value, typeof(T), invariantSettings);

		/// <summary>
		/// Returns an object of the specified type whose value is equivalent to the specified
		/// object, using the invariant culture.
		/// </summary>
		/// <typeparam name="T">The type to convert the data to.</typeparam>
		/// <param name="value">The data to convert.</param>
		/// <param name="settings">The conversion settings. The <see cref="DeepConvertSettings.Provider"/>
		///   will be overwritten with <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>An object whose type is <typeparamref name="T"/> and whose value is equivalent
		///   to <paramref name="value"/>.</returns>
		public static T ChangeTypeInvariant<T>(object value, DeepConvertSettings settings) =>
			(T)ChangeType(value, typeof(T), new DeepConvertSettings(settings) { Provider = CultureInfo.InvariantCulture });

		/// <summary>
		/// Returns an object of the specified type whose value is equivalent to the specified
		/// object, using the invariant culture.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="destType">The type to convert the data to.</param>
		/// <returns>An object whose type is <paramref name="destType"/> and whose value is
		///   equivalent to <paramref name="value"/>.</returns>
		public static object ChangeTypeInvariant(object value, Type destType) =>
			ChangeType(value, destType, invariantSettings);

		/// <summary>
		/// Returns an object of the specified type whose value is equivalent to the specified
		/// object, using the invariant culture.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="destType">The type to convert the data to.</param>
		/// <param name="settings">The conversion settings. The <see cref="DeepConvertSettings.Provider"/>
		///   will be overwritten with <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>An object whose type is <paramref name="destType"/> and whose value is
		///   equivalent to <paramref name="value"/>.</returns>
		public static object ChangeTypeInvariant(object value, Type destType, DeepConvertSettings settings) =>
			ChangeType(value, destType, new DeepConvertSettings(settings) { Provider = CultureInfo.InvariantCulture });

		#endregion ChangeTypeInvariant methods

		#region ToDateTime methods

		/// <summary>
		/// Returns a <see cref="DateTime"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <returns>A <see cref="DateTime"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static DateTime ToDateTime(object value) =>
			ToDateTime(value, emptySettings);

		/// <summary>
		/// Returns a <see cref="DateTime"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="settings">The conversion settings.</param>
		/// <returns>A <see cref="DateTime"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static DateTime ToDateTime(object value, DeepConvertSettings settings)
		{
			var provider = settings.Provider;
			if (provider == null)
				provider = CultureInfo.CurrentCulture;

			var defaultKind = DateTimeKind.Unspecified;
			if ((settings.DateTimeStyles & DateTimeStyles.AssumeLocal) != 0)
				defaultKind = DateTimeKind.Local;
			if ((settings.DateTimeStyles & DateTimeStyles.AssumeUniversal) != 0)
				defaultKind = DateTimeKind.Utc;

			if (value == null)
				throw new ArgumentNullException(nameof(value));

			var srcType = value.GetType();

			if (srcType == typeof(DateTime))
			{
				return (DateTime)value;
			}
			if (srcType == typeof(TimeSpan))
			{
				var timeSpan = (TimeSpan)value;
				switch (settings.DateNumericKind)
				{
					case DateNumericKind.None:
					case DateNumericKind.Ticks:
						if ((settings.DateTimeStyles & DateTimeStyles.NoCurrentDateDefault) != 0)
						{
							return new DateTime(timeSpan.Ticks, defaultKind);
						}
						var today = defaultKind == DateTimeKind.Utc ? DateTime.UtcNow.Date : DateTime.Today;
						return new DateTime(today.Ticks + timeSpan.Ticks, defaultKind);
					case DateNumericKind.UnixSeconds:
					case DateNumericKind.UnixMilliseconds:
						return unixEpoch.AddTicks(timeSpan.Ticks);
					default:
						throw new ArgumentException("Unknown date numeric kind.");
				}
			}
			if (srcType == typeof(char) ||
				srcType == typeof(byte) || srcType == typeof(sbyte) ||
				srcType == typeof(short) || srcType == typeof(ushort) ||
				srcType == typeof(int) || srcType == typeof(uint) ||
				srcType == typeof(long) || srcType == typeof(ulong) ||
				srcType == typeof(BigInteger))
			{
				long num = ChangeType<long>(value, settings);
				return ConvertNumeric(num, settings.DateNumericKind, defaultKind);
			}
			if (srcType == typeof(float) || srcType == typeof(double) || srcType == typeof(decimal))
			{
				double num = ChangeType<double>(value, settings);
				return ConvertNumeric(num, settings.DateNumericKind, defaultKind);
			}
			if (srcType == typeof(string))
			{
				string str = ((string)value).Trim();
				if (str == "")
					throw new InvalidCastException($"The empty string cannot be converted to DateTime.");
				if (settings.DateNumericKind != DateNumericKind.None)
				{
					if (long.TryParse(str, NumberStyles.AllowLeadingSign, provider, out long num))
						return ConvertNumeric(num, settings.DateNumericKind, defaultKind);
					if (double.TryParse(str, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, provider, out double num2))
						return ConvertNumeric(num2, settings.DateNumericKind, defaultKind);
				}

				if (!string.IsNullOrWhiteSpace(settings.Format) &&
					DateTime.TryParseExact(str, settings.Format, provider, settings.DateTimeStyles, out DateTime date))
					return date;
				if (DateTime.TryParse(str, provider, settings.DateTimeStyles, out date))
					return date;
			}
			throw new InvalidCastException($"The value '{value}' ({srcType.FullName}) cannot be converted to DateTime.");
		}

		#endregion ToDateTime methods

		#region Helper methods

		/// <summary>
		/// Returns the default value of the specified type.
		/// </summary>
		/// <param name="type">The type to return the default value of.</param>
		/// <returns>The default value of <paramref name="type"/>.</returns>
		public static object GetDefaultValue(Type type)
		{
			if (type.IsValueType)
				return Activator.CreateInstance(type);

			return null;
		}

		private static object CreateTuple(
			Type tupleType,
			IEnumerable<object> items,
			DeepConvertSettings settings)
		{
			object[] args = GetConvertedValues(items, tupleType.GenericTypeArguments, settings)
				.Concat(GetDefaultValues(tupleType.GenericTypeArguments.Skip(items.Count())))
				.ToArray();
			return Activator.CreateInstance(tupleType, args);
		}

		private static IEnumerable<object> GetConvertedValues(
			IEnumerable<object> items,
			IEnumerable<Type> types,
			DeepConvertSettings settings)
		{
			return items
				.Take(types.Count())
				.Select((o, i) => ChangeType(o, types.ElementAt(i), settings));
		}

		private static IEnumerable<object> GetDefaultValues(IEnumerable<Type> types)
		{
			foreach (var type in types)
			{
				yield return GetDefaultValue(type);
			}
		}

		private static DateTime ConvertNumeric(long num, DateNumericKind numericKind, DateTimeKind defaultKind)
		{
			switch (numericKind)
			{
				case DateNumericKind.None:
					throw new ArgumentException("Unsupported date numeric kind.");
				case DateNumericKind.Ticks:
					return new DateTime(num, defaultKind);
				case DateNumericKind.UnixSeconds:
					return unixEpoch.AddSeconds(num);
				case DateNumericKind.UnixMilliseconds:
					return unixEpoch.AddMilliseconds(num);
				default:
					throw new ArgumentException("Unknown date numeric kind.");
			}
		}

		private static DateTime ConvertNumeric(double num, DateNumericKind numericKind, DateTimeKind defaultKind)
		{
			switch (numericKind)
			{
				case DateNumericKind.None:
					throw new ArgumentException("Unsupported date numeric kind.");
				case DateNumericKind.Ticks:
					return new DateTime((long)num, defaultKind);
				case DateNumericKind.UnixSeconds:
					return unixEpoch.AddSeconds(num);
				case DateNumericKind.UnixMilliseconds:
					return unixEpoch.AddMilliseconds(num);
				default:
					throw new ArgumentException("Unknown date numeric kind.");
			}
		}

		#endregion Helper methods

		private static readonly ConcurrentDictionary<Type, ConvertTypeInfo> typeInfos =
			new ConcurrentDictionary<Type, ConvertTypeInfo>();

		/// <summary>
		/// Returns an object of the specified type whose properties have the values from equally
		/// named dictionary keys.
		/// </summary>
		/// <typeparam name="T">The type to convert the data to.</typeparam>
		/// <param name="dict">The data to set in the object properties.</param>
		/// <param name="settings">The conversion settings.</param>
		/// <returns>An object whose properties have the values from equally named dictionary keys.</returns>
		public static T ConvertToObject<T>(IDictionary dict, DeepConvertSettings settings) =>
			(T)ConvertToObject(dict, typeof(T), settings);

		/// <summary>
		/// Returns an object of the specified type whose properties have the values from equally
		/// named dictionary keys.
		/// </summary>
		/// <param name="dict">The data to set in the object properties.</param>
		/// <param name="type">The type to convert the data to.</param>
		/// <param name="settings">The conversion settings.</param>
		/// <returns>An object whose properties have the values from equally named dictionary keys.</returns>
		public static object ConvertToObject(IDictionary dict, Type type, DeepConvertSettings settings)
		{
			var typeInfo = GetTypeInfo(type);
			object obj = typeInfo.CreateInstance();
			foreach (object key in dict.Keys)
			{
				string name = ToString(key, settings);
				typeInfo.SetValue(obj, name, dict[key], settings);
			}
			return obj;
		}

		/// <summary>
		/// Returns a dictionary that contains keys and values of equally named properties of the
		/// specified object.
		/// </summary>
		/// <param name="obj">The object to convert to a dictionary.</param>
		/// <returns>A dictionary that contains the data from the object properties.</returns>
		public static Dictionary<string, object> ConvertToDictionary(object obj)
		{
			var typeInfo = GetTypeInfo(obj.GetType());
			var dict = new Dictionary<string, object>();
			foreach (string name in typeInfo.GetNames())
			{
				dict[name] = typeInfo.GetValue(obj, name);
			}
			return dict;
		}

		/// <summary>
		/// Creates and caches the reflection type info to access the properties of a type.
		/// </summary>
		/// <param name="type">The type to inspect.</param>
		/// <returns>The <see cref="ConvertTypeInfo"/> instance to access the type properties.</returns>
		private static ConvertTypeInfo GetTypeInfo(Type type)
		{
			return typeInfos.GetOrAdd(type, t => new ConvertTypeInfo(t));
		}
	}
}
