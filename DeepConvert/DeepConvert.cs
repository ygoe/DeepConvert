// Copyright (c) 2018, Yves Goergen, http://unclassified.software
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

namespace Unclassified.Util
{
	/// <summary>
	/// Converts a data type to another data type, including collections and their items.
	/// </summary>
	public static class DeepConvert
	{
		/// <summary>
		/// All words that are considered boolean false. Anything else is considered true.
		/// </summary>
		private static readonly string[] falseWords = new[]
		{
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
		private static DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Returns an object of the specified type whose value is equivalent to the specified
		/// object.
		/// </summary>
		/// <typeparam name="T">The type to convert the data to.</typeparam>
		/// <param name="value">The data to convert.</param>
		/// <param name="provider">An object that supplies culture-specific formatting information.
		///    If unset, CultureInfo.CurrentCulture is used.</param>
		/// <param name="dateFormat">A date format string to parse non-numeric strings with.</param>
		/// <param name="dateNumericKind">Specifies how numeric values can be interpreted as date.</param>
		/// <param name="dateTimeStyles">Specifies how to interpret the parsed date in relation to the
		///   current time zone or the current date.</param>
		/// <returns>An object whose type is <typeparamref name="T"/> and whose value is equivalent
		///   to <paramref name="value"/>.</returns>
		public static T ChangeType<T>(
			object value,
			IFormatProvider provider = null,
			string dateFormat = null,
			DateNumericKind dateNumericKind = DateNumericKind.Ticks,
			DateTimeStyles dateTimeStyles = DateTimeStyles.None) =>
			(T)ChangeType(value, typeof(T), provider, dateFormat, dateNumericKind, dateTimeStyles);

		/// <summary>
		/// Returns an object of the specified type whose value is equivalent to the specified
		/// object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="destType">The type to convert the data to.</param>
		/// <param name="provider">An object that supplies culture-specific formatting information.
		///    If unset, CultureInfo.CurrentCulture is used.</param>
		/// <param name="dateFormat">A date format string to parse non-numeric strings with.</param>
		/// <param name="dateNumericKind">Specifies how numeric values can be interpreted as date.</param>
		/// <param name="dateTimeStyles">Specifies how to interpret the parsed date in relation to the
		///   current time zone or the current date.</param>
		/// <returns>An object whose type is <paramref name="destType"/> and whose value is
		///   equivalent to <paramref name="value"/>.</returns>
		public static object ChangeType(
			object value,
			Type destType,
			IFormatProvider provider = null,
			string dateFormat = null,
			DateNumericKind dateNumericKind = DateNumericKind.Ticks,
			DateTimeStyles dateTimeStyles = DateTimeStyles.None)
		{
			if (provider == null)
				provider = CultureInfo.CurrentCulture;

			// Handle null value
			if (value == null)
			{
				if (Nullable.GetUnderlyingType(destType) != null || destType.IsClass)
				{
					return value;
				}
				throw new InvalidCastException($"The null value cannot be converted into a {destType.FullName}.");
			}

			var srcType = value.GetType();

			// Convert to Nullables
			// (srcType is never Nullable, it's either null or the element type)
			if (Nullable.GetUnderlyingType(destType) != null)
			{
				return ChangeType(value, Nullable.GetUnderlyingType(destType), provider, dateFormat, dateNumericKind, dateTimeStyles);
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
				if (destType == typeof(string) || destType == typeof(char) ||
					destType == typeof(byte) || destType == typeof(sbyte) ||
					destType == typeof(short) || destType == typeof(ushort) ||
					destType == typeof(int) || destType == typeof(uint) ||
					destType == typeof(long) || destType == typeof(ulong) ||
					destType == typeof(float) || destType == typeof(double) || destType == typeof(decimal))
				{
					return Convert.ChangeType(value, destType, provider);
				}
				if (destType.IsEnum) return Convert.ChangeType(value, Enum.GetUnderlyingType(destType), provider);
			}
			if (srcType == typeof(string))
			{
				if (destType == typeof(bool)) return !falseWords.Any(w => string.Equals(w, (string)value, StringComparison.OrdinalIgnoreCase));
				if (destType == typeof(BigInteger)) return BigInteger.Parse((string)value, provider);
				if (destType == typeof(DateTime)) return ToDateTime(value, provider, dateFormat, dateNumericKind, dateTimeStyles); //DateTime.Parse((string)value, provider);
				if (destType == typeof(TimeSpan)) return TimeSpan.Parse((string)value, provider);
				if (destType == typeof(Guid)) return Guid.Parse((string)value);
				// TODO: Convert with collection of chars (also in reverse direction)
			}
			if (srcType == typeof(char))
			{
				if (destType == typeof(bool)) return !value.Equals((char)0) ? true : false;
				if (destType == typeof(BigInteger)) return new BigInteger((char)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, provider, dateFormat, dateNumericKind, dateTimeStyles); //new DateTime((char)value);
				if (destType == typeof(TimeSpan)) return new TimeSpan((char)value);
				// No conversion to Guid
			}
			if (srcType == typeof(byte))
			{
				if (destType == typeof(bool)) return !value.Equals((byte)0) ? true : false;
				if (destType == typeof(BigInteger)) return new BigInteger((byte)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, provider, dateFormat, dateNumericKind, dateTimeStyles); //new DateTime((byte)value);
				if (destType == typeof(TimeSpan)) return new TimeSpan((byte)value);
				// No conversion to Guid
			}
			if (srcType == typeof(sbyte))
			{
				if (destType == typeof(bool)) return !value.Equals((sbyte)0) ? true : false;
				if (destType == typeof(BigInteger)) return new BigInteger((sbyte)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, provider, dateFormat, dateNumericKind, dateTimeStyles); //new DateTime((sbyte)value);
				if (destType == typeof(TimeSpan)) return new TimeSpan((sbyte)value);
				// No conversion to Guid
			}
			if (srcType == typeof(short))
			{
				if (destType == typeof(bool)) return !value.Equals((short)0) ? true : false;
				if (destType == typeof(BigInteger)) return new BigInteger((short)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, provider, dateFormat, dateNumericKind, dateTimeStyles); //new DateTime((short)value);
				if (destType == typeof(TimeSpan)) return new TimeSpan((short)value);
				// No conversion to Guid
			}
			if (srcType == typeof(ushort))
			{
				if (destType == typeof(bool)) return !value.Equals((ushort)0) ? true : false;
				if (destType == typeof(BigInteger)) return new BigInteger((ushort)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, provider, dateFormat, dateNumericKind, dateTimeStyles); //new DateTime((ushort)value);
				if (destType == typeof(TimeSpan)) return new TimeSpan((ushort)value);
				// No conversion to Guid
			}
			if (srcType == typeof(int))
			{
				if (destType == typeof(bool)) return !value.Equals(0) ? true : false;
				if (destType == typeof(BigInteger)) return new BigInteger((int)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, provider, dateFormat, dateNumericKind, dateTimeStyles); //new DateTime((int)value);
				if (destType == typeof(TimeSpan)) return new TimeSpan((int)value);
				// No conversion to Guid
			}
			if (srcType == typeof(uint))
			{
				if (destType == typeof(bool)) return !value.Equals((uint)0) ? true : false;
				if (destType == typeof(BigInteger)) return new BigInteger((uint)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, provider, dateFormat, dateNumericKind, dateTimeStyles); //new DateTime((uint)value);
				if (destType == typeof(TimeSpan)) return new TimeSpan((uint)value);
				// No conversion to Guid
			}
			if (srcType == typeof(long))
			{
				if (destType == typeof(bool)) return !value.Equals((long)0) ? true : false;
				if (destType == typeof(BigInteger)) return new BigInteger((long)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, provider, dateFormat, dateNumericKind, dateTimeStyles); //new DateTime((long)value);
				if (destType == typeof(TimeSpan)) return new TimeSpan((long)value);
				// No conversion to Guid
			}
			if (srcType == typeof(ulong))
			{
				if (destType == typeof(bool)) return !value.Equals((ulong)0) ? true : false;
				if (destType == typeof(BigInteger)) return new BigInteger((ulong)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, provider, dateFormat, dateNumericKind, dateTimeStyles); //new DateTime((long)(ulong)value);
				if (destType == typeof(TimeSpan)) return new TimeSpan((long)(ulong)value);
				// No conversion to Guid
			}
			if (srcType == typeof(float))
			{
				if (destType == typeof(bool)) return !value.Equals((float)0) ? true : false;
				if (destType == typeof(BigInteger)) return new BigInteger((float)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, provider, dateFormat, dateNumericKind, dateTimeStyles); //new DateTime((long)(float)value);
				if (destType == typeof(TimeSpan)) return new TimeSpan((long)(float)value);
				// No conversion to Guid
			}
			if (srcType == typeof(double))
			{
				if (destType == typeof(bool)) return !value.Equals((double)0) ? true : false;
				if (destType == typeof(BigInteger)) return new BigInteger((double)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, provider, dateFormat, dateNumericKind, dateTimeStyles); //new DateTime((long)(double)value);
				if (destType == typeof(TimeSpan)) return new TimeSpan((long)(double)value);
				// No conversion to Guid
			}
			if (srcType == typeof(decimal))
			{
				if (destType == typeof(bool)) return !value.Equals((decimal)0) ? true : false;
				if (destType == typeof(BigInteger)) return new BigInteger((decimal)value);
				if (destType == typeof(DateTime)) return ToDateTime(value, provider, dateFormat, dateNumericKind, dateTimeStyles); //new DateTime((long)(decimal)value);
				if (destType == typeof(TimeSpan)) return new TimeSpan((long)(decimal)value);
				// No conversion to Guid
			}

			if (srcType.IsEnum)
			{
				object enumValue = Convert.ChangeType(value, Enum.GetUnderlyingType(srcType), provider);
				if (destType == typeof(string) || destType == typeof(char) ||
					destType == typeof(byte) || destType == typeof(sbyte) ||
					destType == typeof(short) || destType == typeof(ushort) ||
					destType == typeof(int) || destType == typeof(uint) ||
					destType == typeof(long) || destType == typeof(ulong) ||
					destType == typeof(float) || destType == typeof(double) || destType == typeof(decimal))
				{
					return Convert.ChangeType(enumValue, destType, provider);
				}
				if (destType.IsEnum) return Convert.ChangeType(enumValue, Enum.GetUnderlyingType(destType), provider);
				if (destType == typeof(bool)) return !enumValue.Equals(0) ? true : false;
				if (destType == typeof(BigInteger)) return new BigInteger(Convert.ToInt64(enumValue));
				if (destType == typeof(DateTime)) return ToDateTime(value, provider, dateFormat, dateNumericKind, dateTimeStyles); //new DateTime(Convert.ToInt64(enumValue));
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
				if (destType.IsEnum) return (bool)value ? Convert.ChangeType(1, Enum.GetUnderlyingType(destType)) : Convert.ChangeType(0, Enum.GetUnderlyingType(destType));
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
				if (destType.IsEnum) return Convert.ChangeType(((BigInteger)value).ToString(), Enum.GetUnderlyingType(destType), provider);
				if (destType == typeof(bool)) return !((BigInteger)value).IsZero ? true : false;
				if (destType == typeof(BigInteger)) return value;
				if (destType == typeof(DateTime)) return ToDateTime(value, provider, dateFormat, dateNumericKind, dateTimeStyles); //new DateTime(Convert.ToInt64(((BigInteger)value).ToString(), provider));
				if (destType == typeof(TimeSpan)) return new TimeSpan(Convert.ToInt64(((BigInteger)value).ToString(), provider));
				// No conversion to Guid
			}
			if (srcType == typeof(DateTime))
			{
				if (destType == typeof(string))
				{
					if (!string.IsNullOrEmpty(dateFormat))
					{
						return ((DateTime)value).ToString(dateFormat, provider);
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
					switch (dateNumericKind)
					{
						case DateNumericKind.Ticks:
							return ChangeType(((DateTime)value).Ticks, destType, provider, dateFormat, dateNumericKind, dateTimeStyles);
						case DateNumericKind.UnixSeconds:
							return ChangeType((((DateTime)value).ToUniversalTime() - unixEpoch).TotalSeconds, destType, provider, dateFormat, dateNumericKind, dateTimeStyles);
						case DateNumericKind.UnixMilliseconds:
							return ChangeType((((DateTime)value).ToUniversalTime() - unixEpoch).TotalMilliseconds, destType, provider, dateFormat, dateNumericKind, dateTimeStyles);
						default:
							throw new ArgumentException("Unknown date numeric kind.");
					}
				}
				if (destType == typeof(bool)) return ((DateTime)value).Ticks != 0 ? true : false;
				if (destType == typeof(DateTime)) return value;
				if (destType == typeof(TimeSpan)) return ((DateTime)value).TimeOfDay; //new TimeSpan(((DateTime)value).Ticks);
				// No conversion to Guid
			}
			if (srcType == typeof(TimeSpan))
			{
				if (destType == typeof(string)) return ((TimeSpan)value).ToString();
				if (destType == typeof(char) ||
					destType == typeof(byte) || destType == typeof(sbyte) ||
					destType == typeof(short) || destType == typeof(ushort) ||
					destType == typeof(int) || destType == typeof(uint) ||
					destType == typeof(long) || destType == typeof(ulong) ||
					destType == typeof(float) || destType == typeof(double) || destType == typeof(decimal) ||
					destType == typeof(BigInteger) ||
					destType.IsEnum)
				{
					switch (dateNumericKind)
					{
						case DateNumericKind.Ticks:
							return ChangeType(((TimeSpan)value).Ticks, destType, provider, dateFormat, dateNumericKind, dateTimeStyles);
						case DateNumericKind.UnixSeconds:
							return ChangeType(((TimeSpan)value).TotalSeconds, destType, provider, dateFormat, dateNumericKind, dateTimeStyles);
						case DateNumericKind.UnixMilliseconds:
							return ChangeType(((TimeSpan)value).TotalMilliseconds, destType, provider, dateFormat, dateNumericKind, dateTimeStyles);
						default:
							throw new ArgumentException("Unknown date numeric kind.");
					}
					//return Convert.ChangeType(((TimeSpan)value).Ticks, destType);
				}
				if (destType == typeof(bool)) return ((TimeSpan)value).Ticks != 0 ? true : false;
				if (destType == typeof(DateTime)) return ToDateTime(value, provider, dateFormat, dateNumericKind, dateTimeStyles); //new DateTime(((TimeSpan)value).Ticks);
				if (destType == typeof(TimeSpan)) return value;
				// No conversion to Guid
			}
			if (srcType == typeof(Guid))
			{
				if (destType == typeof(string)) return ((Guid)value).ToString();
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

			// Convert between collection types, then recurse into each item
			IEnumerable<object> items = null;
			if (typeof(IEnumerable).IsAssignableFrom(srcType))
			{
				items = ((IEnumerable)value).OfType<object>();
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
					foreach (object item in items.Select(o => ChangeType(o, destType.GetElementType(), provider, dateFormat, dateNumericKind, dateTimeStyles)))
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
					foreach (object item in items.Select(o => ChangeType(o, destType.GenericTypeArguments[0], provider, dateFormat, dateNumericKind, dateTimeStyles)))
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
					return CreateTuple(destType, items, provider, dateFormat, dateNumericKind, dateTimeStyles);
				}
				var collectionType = destType.GetInterfaces().FirstOrDefault(t => t.GetGenericTypeDefinition() == typeof(ICollection<>));
				if (collectionType != null)
				{
					object list = Activator.CreateInstance(destType);
					var add = collectionType.GetMethod("Add");
					foreach (object item in items.Select(o => ChangeType(o, collectionType.GenericTypeArguments[0], provider, dateFormat, dateNumericKind, dateTimeStyles)))
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
			}

			// TODO: Convert between atomic and collection types (needs recursion?) (take care of KeyValuePair (a tuple) <-> Dictionary (a collection))

			// At least try IConvertible if srcType implements it
			if (typeof(IConvertible).IsAssignableFrom(srcType))
			{
				return Convert.ChangeType(value, destType, provider);
			}

			throw new InvalidCastException($"The value '{value}' ({srcType.FullName}) cannot be converted to {destType.FullName}.");
		}

		/// <summary>
		/// Returns a DateTime value whose value is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="provider">An object that supplies culture-specific formatting information.
		///    If unset, CultureInfo.CurrentCulture is used.</param>
		/// <param name="dateFormat">A date format string to parse non-numeric strings with.</param>
		/// <param name="numericKind">Specifies how numeric values can be interpreted as date.</param>
		/// <param name="styles">Specifies how to interpret the parsed date in relation to the
		///   current time zone or the current date.</param>
		/// <returns>A DateTime value whose value is equivalent to <paramref name="value"/>.</returns>
		public static DateTime ToDateTime(
			object value,
			IFormatProvider provider = null,
			string dateFormat = null,
			DateNumericKind numericKind = DateNumericKind.Ticks,
			DateTimeStyles styles = DateTimeStyles.None)
		{
			if (provider == null)
				provider = CultureInfo.CurrentCulture;

			var defaultKind = DateTimeKind.Unspecified;
			if ((styles & DateTimeStyles.AssumeLocal) != 0)
				defaultKind = DateTimeKind.Local;
			if ((styles & DateTimeStyles.AssumeUniversal) != 0)
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
				switch (numericKind)
				{
					case DateNumericKind.Ticks:
						if ((styles & DateTimeStyles.NoCurrentDateDefault) != 0)
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
				long num = ChangeType<long>(value, provider, dateFormat, numericKind, styles);
				return ConvertNumeric(num, numericKind, defaultKind);
			}
			if (srcType == typeof(float) || srcType == typeof(double) || srcType == typeof(decimal))
			{
				double num = ChangeType<double>(value, provider, dateFormat, numericKind, styles);
				return ConvertNumeric(num, numericKind, defaultKind);
			}
			if (srcType == typeof(string))
			{
				string str = ((string)value).Trim();
				if (str == "")
					throw new InvalidCastException($"The empty string cannot be converted to DateTime.");
				if (long.TryParse(str, NumberStyles.AllowLeadingSign, provider, out long num))
					return ConvertNumeric(num, numericKind, defaultKind);
				if (double.TryParse(str, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, provider, out double num2))
					return ConvertNumeric(num2, numericKind, defaultKind);

				if (!string.IsNullOrWhiteSpace(dateFormat) && DateTime.TryParseExact(str, dateFormat, provider, styles, out DateTime date))
					return date;
				if (DateTime.TryParse(str, provider, styles, out date))
					return date;
			}
			throw new InvalidCastException($"The value '{value}' ({srcType.FullName}) cannot be converted to DateTime.");
		}

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
			IFormatProvider provider,
			string dateFormat,
			DateNumericKind dateNumericKind,
			DateTimeStyles dateTimeStyles)
		{
			object[] args = GetConvertedValues(items, tupleType.GenericTypeArguments, provider, dateFormat, dateNumericKind, dateTimeStyles)
				.Concat(GetDefaultValues(tupleType.GenericTypeArguments.Skip(items.Count())))
				.ToArray();
			return Activator.CreateInstance(tupleType, args);
		}

		private static IEnumerable<object> GetConvertedValues(
			IEnumerable<object> items,
			IEnumerable<Type> types,
			IFormatProvider provider,
			string dateFormat,
			DateNumericKind dateNumericKind,
			DateTimeStyles dateTimeStyles)
		{
			return items
				.Take(types.Count())
				.Select((o, i) => ChangeType(o, types.ElementAt(i), provider, dateFormat, dateNumericKind, dateTimeStyles));
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
	}

	/// <summary>
	/// Defines how numeric values can be interpreted as date.
	/// </summary>
	public enum DateNumericKind
	{
		/// <summary>
		/// Ticks of 100 nanoseconds since 0001-01-01T00:00:00 (.NET and Windows).
		/// </summary>
		Ticks,
		/// <summary>
		/// Seconds since UNIX epoch, 1970-01-01T00:00:00Z.
		/// </summary>
		UnixSeconds,
		/// <summary>
		/// Millieseconds since UNIX epoch, 1970-01-01T00:00:00Z (JavaScript).
		/// </summary>
		UnixMilliseconds,
	}
}
