using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unclassified.Util
{
	[TestClass]
	public class DeepConvertTests
	{
		[TestMethod]
		public void ChangeType_Numerics()
		{
			Assert.AreEqual(20, DeepConvert.ChangeType<byte>(20));
			Assert.AreEqual("20", DeepConvert.ChangeType<string>(20.0));
			Assert.AreEqual(false, DeepConvert.ChangeType<bool>(0));
			Assert.AreEqual(true, DeepConvert.ChangeType<bool>(20));
			Assert.AreEqual(false, DeepConvert.ChangeType<bool>(0.0));
			Assert.AreEqual(true, DeepConvert.ChangeType<bool>(20.0));
			Assert.AreEqual(1, DeepConvert.ChangeType<byte>(true));
		}

		[TestMethod]
		public void ChangeType_Boolean()
		{
			Assert.AreEqual(false, DeepConvert.ChangeType<bool>(0));
			Assert.AreEqual(true, DeepConvert.ChangeType<bool>(1));
			Assert.AreEqual(true, DeepConvert.ChangeType<bool>(1.2));
			Assert.AreEqual(false, DeepConvert.ChangeType<bool>("false"));
			Assert.AreEqual(false, DeepConvert.ChangeType<bool>("no"));
			Assert.AreEqual(false, DeepConvert.ChangeType<bool>("off"));
			Assert.AreEqual(false, DeepConvert.ChangeType<bool>("nein"));
			Assert.AreEqual(false, DeepConvert.ChangeType<bool>(""));
			Assert.AreEqual(true, DeepConvert.ChangeType<bool>("true"));
			Assert.AreEqual(true, DeepConvert.ChangeType<bool>("123"));
			Assert.AreEqual(true, DeepConvert.ChangeType<bool>("abc"));
		}

		private enum Enum1 : short
		{
			Zero1,
			One1,
			Two1
		}

		private enum Enum2 : byte
		{
			Zero2,
			One2,
			Two2
		}

		[TestMethod]
		public void ChangeType_Enums()
		{
			Assert.AreEqual(Enum2.One2, DeepConvert.ChangeType<Enum2>(Enum1.One1));
			Assert.AreEqual(Enum2.One2, DeepConvert.ChangeType<Enum2>(1));
			Assert.AreEqual(Enum2.One2, DeepConvert.ChangeType<Enum2?>(1));
			Assert.AreEqual(Enum1.One1, DeepConvert.ChangeType<Enum1?>(Enum1.One1));
			Assert.AreEqual(Enum1.One1, DeepConvert.ChangeType<Enum1?>(true));
			Assert.AreEqual(Enum1.Zero1, DeepConvert.ChangeType<Enum1?>(false));
			Assert.AreEqual(Enum1.One1, DeepConvert.ChangeType<Enum1?>(new BigInteger(1)));
		}

		[TestMethod]
		public void ChangeType_Nullable()
		{
			Assert.AreEqual(4, DeepConvert.ChangeType<int>(4));
			Assert.AreEqual(null, DeepConvert.ChangeType<int?>(null));
			Assert.AreEqual(null, DeepConvert.ChangeType<int?>(""));
			Assert.AreEqual(null, DeepConvert.ChangeType<bool?>(""));
			Assert.AreEqual(4, DeepConvert.ChangeType<int?>(4));
		}

		[TestMethod]
		public void ChangeType_ArrayToList()
		{
			var intList = DeepConvert.ChangeType<List<int>>(new[] { "1", "2" });
			Assert.AreEqual(typeof(List<int>), intList.GetType());
			Assert.AreEqual(2, intList.Count);
			Assert.AreEqual(1, intList[0]);
			Assert.AreEqual(2, intList[1]);
		}

		[TestMethod]
		public void ChangeType_ArrayToTuple()
		{
			var tuple = DeepConvert.ChangeType<Tuple<short, long, string, char, bool>>(new object[] { "1", "2", 3, 4, 5, 6 });
			Assert.AreEqual(typeof(Tuple<short, long, string, char, bool>), tuple.GetType());
			Assert.AreEqual(1, tuple.Item1);
			Assert.AreEqual(2, tuple.Item2);
			Assert.AreEqual("3", tuple.Item3);
			Assert.AreEqual('\u0004', tuple.Item4);
			Assert.AreEqual(true, tuple.Item5);
		}

		[TestMethod]
		public void ChangeType_ArrayToLongerTuple()
		{
			var tuple = DeepConvert.ChangeType<Tuple<long, string, char, bool>>(new object[] { "1", 2 });
			Assert.AreEqual(typeof(Tuple<long, string, char, bool>), tuple.GetType());
			Assert.AreEqual(1, tuple.Item1);
			Assert.AreEqual("2", tuple.Item2);
			Assert.AreEqual('\0', tuple.Item3);
			Assert.AreEqual(false, tuple.Item4);
		}

		[TestMethod]
		public void ChangeType_ArrayToArray()
		{
			int[] intArray = DeepConvert.ChangeType<int[]>(new object[] { "1", 2, 3.0, '\u0004' });
			Assert.AreEqual(typeof(int[]), intArray.GetType());
			Assert.AreEqual(4, intArray.Length);
			Assert.AreEqual(1, intArray[0]);
			Assert.AreEqual(2, intArray[1]);
			Assert.AreEqual(3, intArray[2]);
			Assert.AreEqual(4, intArray[3]);
		}

		[TestMethod]
		public void ChangeType_EnumerableToQueue()
		{
			var intQueue = DeepConvert.ChangeType<Queue<int>>(new[] { 1 }.Concat(new[] { 2 }));
			Assert.AreEqual(typeof(Queue<int>), intQueue.GetType());
			Assert.AreEqual(2, intQueue.Count);
			Assert.AreEqual(1, intQueue.ElementAt(0));
			Assert.AreEqual(2, intQueue.ElementAt(1));
		}

		[TestMethod]
		public void ToDateTime_NumericDefault()
		{
			var date = DeepConvert.ToDateTime(TimeSpan.TicksPerDay);
			Assert.AreEqual(new DateTime(1, 1, 2, 0, 0, 0), date);
			Assert.AreEqual(DateTimeKind.Unspecified, date.Kind);
		}

		[TestMethod]
		public void ToDateTime_NumericUnixSeconds()
		{
			var date = DeepConvert.ToDateTime(2 * 86400, numericKind: DateNumericKind.UnixSeconds);
			Assert.AreEqual(new DateTime(1970, 1, 3, 0, 0, 0), date);
			Assert.AreEqual(DateTimeKind.Utc, date.Kind);
		}

		[TestMethod]
		public void ToDateTime_NumericUnixMilliseconds()
		{
			var date = DeepConvert.ToDateTime(3 * 86400 * 1000, numericKind: DateNumericKind.UnixMilliseconds);
			Assert.AreEqual(new DateTime(1970, 1, 4, 0, 0, 0), date);
			Assert.AreEqual(DateTimeKind.Utc, date.Kind);
		}

		[TestMethod]
		public void ToDateTime_DoubleUnixMilliseconds()
		{
			var date = DeepConvert.ToDateTime(3.5 * 86400 * 1000, numericKind: DateNumericKind.UnixMilliseconds);
			Assert.AreEqual(new DateTime(1970, 1, 4, 12, 0, 0), date);
			Assert.AreEqual(DateTimeKind.Utc, date.Kind);
		}

		[TestMethod]
		public void ToDateTime_StringFormat()
		{
			var date = DeepConvert.ToDateTime("18.02.03 04.05.06", dateFormat: "yy.MM.dd HH.mm.ss");
			Assert.AreEqual(new DateTime(2018, 2, 3, 4, 5, 6), date);
			Assert.AreEqual(DateTimeKind.Unspecified, date.Kind);
		}

		[TestMethod]
		public void ToDateTime_StringFormatLocal()
		{
			var date = DeepConvert.ToDateTime("18.02.03 04.05.06", dateFormat: "yy.MM.dd HH.mm.ss", styles: DateTimeStyles.AssumeLocal);
			Assert.AreEqual(new DateTime(2018, 2, 3, 4, 5, 6), date);
			Assert.AreEqual(DateTimeKind.Local, date.Kind);
		}

		[TestMethod]
		public void ToDateTime_StringUtcToLocal()
		{
			var date = DeepConvert.ToDateTime("2018-02-03 04:05:06", styles: DateTimeStyles.AssumeUniversal);
			Assert.AreEqual(new DateTime(2018, 2, 3, 4, 5, 6, DateTimeKind.Utc).ToLocalTime(), date);
			Assert.AreEqual(DateTimeKind.Local, date.Kind);
		}

		[TestMethod]
		public void ToDateTime_StringLocalToUtc()
		{
			var date = DeepConvert.ToDateTime("2018-02-03 04:05:06", styles: DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal);
			Assert.AreEqual(new DateTime(2018, 2, 3, 4, 5, 6, DateTimeKind.Local).ToUniversalTime(), date);
			Assert.AreEqual(DateTimeKind.Utc, date.Kind);
		}

		[TestMethod]
		public void ToDateTime_NestedStringFormatLocal()
		{
			var dateList = DeepConvert.ChangeType<List<DateTime>>(
				new object[] { 10 * 86400, "18.02.03 04.05.06" },
				dateFormat: "yy.MM.dd HH.mm.ss",
				dateNumericKind: DateNumericKind.UnixSeconds,
				dateTimeStyles: DateTimeStyles.AssumeLocal);
			Assert.AreEqual(typeof(List<DateTime>), dateList.GetType());
			Assert.AreEqual(2, dateList.Count);
			Assert.AreEqual(new DateTime(1970, 1, 11, 0, 0, 0), dateList[0]);
			Assert.AreEqual(DateTimeKind.Utc, dateList[0].Kind);
			Assert.AreEqual(new DateTime(2018, 2, 3, 4, 5, 6), dateList[1]);
			Assert.AreEqual(DateTimeKind.Local, dateList[1].Kind);
		}

		[TestMethod]
		public void ToDateTime_NoSeparators()
		{
			var date = DeepConvert.ChangeType<DateTime>(
				"20100101000003",
				provider: CultureInfo.InvariantCulture,
				dateFormat: "yyyyMMddHHmmss",
				dateNumericKind: DateNumericKind.None,
				dateTimeStyles: DateTimeStyles.None);
			Assert.AreEqual(typeof(DateTime), date.GetType());
			Assert.AreEqual(new DateTime(2010, 1, 1, 0, 0, 3), date);
			Assert.AreEqual(DateTimeKind.Unspecified, date.Kind);
		}
	}
}
