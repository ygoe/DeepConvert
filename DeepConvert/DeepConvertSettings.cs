using System;
using System.Globalization;
using System.Text;

namespace Unclassified.Util
{
	/// <summary>
	/// Provides settings for special data conversions.
	/// </summary>
	public class DeepConvertSettings
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DeepConvertSettings"/> class.
		/// </summary>
		public DeepConvertSettings()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DeepConvertSettings"/> class from another
		/// instance.
		/// </summary>
		/// <param name="source">The initial settings.</param>
		public DeepConvertSettings(DeepConvertSettings source)
		{
			Provider = source.Provider;
			Format = source.Format;
			DateNumericKind = source.DateNumericKind;
			DateTimeStyles = source.DateTimeStyles;
			Encoding = source.Encoding;
		}

		/// <summary>
		/// Gets or sets an object that supplies culture-specific formatting information.
		/// If unset, CultureInfo.CurrentCulture is used.
		/// </summary>
		public IFormatProvider Provider { get; set; }

		/// <summary>
		/// Gets or sets a date format string to parse non-numeric strings with.
		/// </summary>
		public string Format { get; set; }

		/// <summary>
		/// Gets or sets a date format string to parse non-numeric strings with.
		/// </summary>
		[Obsolete("Use the Format property instead.")]
		public string DateFormat
		{
			get => Format;
			set => Format = value;
		}

		/// <summary>
		/// Gets or sets a value specifying how numeric values can be interpreted as date.
		/// </summary>
		public DateNumericKind DateNumericKind { get; set; } = DateNumericKind.Ticks;

		/// <summary>
		/// Gets or sets a value specifying how to interpret the parsed date in relation to the
		/// current time zone or the current date.
		/// </summary>
		public DateTimeStyles DateTimeStyles { get; set; }

		/// <summary>
		/// Gets or sets the encoding that is used for conversions between characters and byte
		/// collections. The encoding is not used for conversions from or to a single byte value
		/// since an encoding might use multiple bytes for a given character.
		/// </summary>
		public Encoding Encoding { get; set; } = Encoding.UTF8;
	}

	/// <summary>
	/// Defines how numeric values can be interpreted as date.
	/// </summary>
	public enum DateNumericKind
	{
		/// <summary>
		/// No numeric interpretation of strings converting to <see cref="DateTime"/>.
		/// When converting other types, this is equivalent to <see cref="Ticks"/>.
		/// </summary>
		None,
		/// <summary>
		/// Ticks of 100 nanoseconds since 0001-01-01T00:00:00 (.NET and Windows).
		/// </summary>
		Ticks,
		/// <summary>
		/// Seconds since UNIX epoch, 1970-01-01T00:00:00Z (Linux, PHP, and more).
		/// </summary>
		UnixSeconds,
		/// <summary>
		/// Milliseconds since UNIX epoch, 1970-01-01T00:00:00Z (JavaScript).
		/// </summary>
		UnixMilliseconds,
	}
}
