using System.Collections.Generic;

namespace Unclassified.Util
{
	public static partial class DeepConvert
	{
		#region int8

		/// <summary>
		/// Returns a <see cref="byte"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <returns>A <see cref="byte"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static byte ToByte(object value) =>
			ChangeType<byte>(value);

		/// <summary>
		/// Returns a <see cref="byte"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="settings">The conversion settings.</param>
		/// <returns>A <see cref="byte"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static byte ToByte(object value, DeepConvertSettings settings) =>
			ChangeType<byte>(value, settings);

		/// <summary>
		/// Returns a <see cref="sbyte"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <returns>A <see cref="sbyte"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static sbyte ToSByte(object value) =>
			ChangeType<sbyte>(value);

		/// <summary>
		/// Returns a <see cref="sbyte"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="settings">The conversion settings.</param>
		/// <returns>A <see cref="sbyte"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static sbyte ToSByte(object value, DeepConvertSettings settings) =>
			ChangeType<sbyte>(value, settings);

		#endregion int8

		#region int16

		/// <summary>
		/// Returns a <see cref="ushort"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <returns>A <see cref="ushort"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static ushort ToUInt16(object value) =>
			ChangeType<ushort>(value);

		/// <summary>
		/// Returns a <see cref="ushort"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="settings">The conversion settings.</param>
		/// <returns>A <see cref="ushort"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static ushort ToUInt16(object value, DeepConvertSettings settings) =>
			ChangeType<ushort>(value, settings);

		/// <summary>
		/// Returns a <see cref="short"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <returns>A <see cref="short"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static short ToInt16(object value) =>
			ChangeType<short>(value);

		/// <summary>
		/// Returns a <see cref="short"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="settings">The conversion settings.</param>
		/// <returns>A <see cref="short"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static short ToInt16(object value, DeepConvertSettings settings) =>
			ChangeType<short>(value, settings);

		#endregion int16

		#region int32

		/// <summary>
		/// Returns a <see cref="uint"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <returns>A <see cref="uint"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static uint ToUInt32(object value) =>
			ChangeType<uint>(value);

		/// <summary>
		/// Returns a <see cref="uint"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="settings">The conversion settings.</param>
		/// <returns>A <see cref="uint"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static uint ToUInt32(object value, DeepConvertSettings settings) =>
			ChangeType<uint>(value, settings);

		/// <summary>
		/// Returns a <see cref="int"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <returns>A <see cref="int"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static int ToInt32(object value) =>
			ChangeType<int>(value);

		/// <summary>
		/// Returns a <see cref="int"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="settings">The conversion settings.</param>
		/// <returns>A <see cref="int"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static int ToInt32(object value, DeepConvertSettings settings) =>
			ChangeType<int>(value, settings);

		#endregion int32

		#region int64

		/// <summary>
		/// Returns a <see cref="ulong"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <returns>A <see cref="ulong"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static ulong ToUInt64(object value) =>
			ChangeType<ulong>(value);

		/// <summary>
		/// Returns a <see cref="ulong"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="settings">The conversion settings.</param>
		/// <returns>A <see cref="ulong"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static ulong ToUInt64(object value, DeepConvertSettings settings) =>
			ChangeType<ulong>(value, settings);

		/// <summary>
		/// Returns a <see cref="long"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <returns>A <see cref="long"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static long ToInt64(object value) =>
			ChangeType<long>(value);

		/// <summary>
		/// Returns a <see cref="long"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="settings">The conversion settings.</param>
		/// <returns>A <see cref="long"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static long ToInt64(object value, DeepConvertSettings settings) =>
			ChangeType<long>(value, settings);

		#endregion int16

		#region decimal

		/// <summary>
		/// Returns a <see cref="decimal"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <returns>A <see cref="decimal"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static decimal ToDecimal(object value) =>
			ChangeType<decimal>(value);

		/// <summary>
		/// Returns a <see cref="decimal"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="settings">The conversion settings.</param>
		/// <returns>A <see cref="decimal"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static decimal ToDecimal(object value, DeepConvertSettings settings) =>
			ChangeType<decimal>(value, settings);

		#endregion decimal

		#region floats

		/// <summary>
		/// Returns a <see cref="float"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <returns>A <see cref="float"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static float ToSingle(object value) =>
			ChangeType<float>(value);

		/// <summary>
		/// Returns a <see cref="float"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="settings">The conversion settings.</param>
		/// <returns>A <see cref="float"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static float ToSingle(object value, DeepConvertSettings settings) =>
			ChangeType<float>(value, settings);

		/// <summary>
		/// Returns a <see cref="double"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <returns>A <see cref="double"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static double ToDouble(object value) =>
			ChangeType<double>(value);

		/// <summary>
		/// Returns a <see cref="double"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="settings">The conversion settings.</param>
		/// <returns>A <see cref="double"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static double ToDouble(object value, DeepConvertSettings settings) =>
			ChangeType<double>(value, settings);

		#endregion floats

		#region bool

		/// <summary>
		/// Returns a <see cref="bool"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <returns>A <see cref="bool"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static bool ToBoolean(object value) =>
			ChangeType<bool>(value);

		/// <summary>
		/// Returns a <see cref="bool"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="settings">The conversion settings.</param>
		/// <returns>A <see cref="bool"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static bool ToBoolean(object value, DeepConvertSettings settings) =>
			ChangeType<bool>(value, settings);

		#endregion bool

		#region string

		/// <summary>
		/// Returns a <see cref="string"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <returns>A <see cref="string"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static string ToString(object value) =>
			ChangeType<string>(value);

		/// <summary>
		/// Returns a <see cref="string"/> value that is equivalent to the specified object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="settings">The conversion settings.</param>
		/// <returns>A <see cref="string"/> value that is equivalent to <paramref name="value"/>.</returns>
		public static string ToString(object value, DeepConvertSettings settings) =>
			ChangeType<string>(value, settings);

		#endregion string

		#region array

		/// <summary>
		/// Returns an array of the specified type whose value is equivalent to the specified
		/// object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <returns>An array whose value is equivalent to <paramref name="value"/>.</returns>
		public static T[] ToArray<T>(object value) =>
			ChangeType<T[]>(value);

		/// <summary>
		/// Returns an array of the specified type whose value is equivalent to the specified
		/// object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="settings">The conversion settings.</param>
		/// <returns>An array whose value is equivalent to <paramref name="value"/>.</returns>
		public static T[] ToArray<T>(object value, DeepConvertSettings settings) =>
			ChangeType<T[]>(value, settings);

		#endregion array

		#region list

		/// <summary>
		/// Returns a list of the specified type whose value is equivalent to the specified
		/// object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <returns>A list whose value is equivalent to <paramref name="value"/>.</returns>
		public static List<T> ToList<T>(object value) =>
			ChangeType<List<T>>(value);

		/// <summary>
		/// Returns a list of the specified type whose value is equivalent to the specified
		/// object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="settings">The conversion settings.</param>
		/// <returns>A list whose value is equivalent to <paramref name="value"/>.</returns>
		public static List<T> ToList<T>(object value, DeepConvertSettings settings) =>
			ChangeType<List<T>>(value, settings);

		#endregion list

		#region dictionary

		/// <summary>
		/// Returns a dictionary of the specified type whose value is equivalent to the specified
		/// object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <returns>A dictionary whose value is equivalent to <paramref name="value"/>.</returns>
		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(object value) =>
			ChangeType<Dictionary<TKey, TValue>>(value);

		/// <summary>
		/// Returns a dictionary of the specified type whose value is equivalent to the specified
		/// object.
		/// </summary>
		/// <param name="value">The data to convert.</param>
		/// <param name="settings">The conversion settings.</param>
		/// <returns>A dictionary whose value is equivalent to <paramref name="value"/>.</returns>
		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(object value, DeepConvertSettings settings) =>
			ChangeType<Dictionary<TKey, TValue>>(value, settings);

		#endregion dictionary
	}
}
