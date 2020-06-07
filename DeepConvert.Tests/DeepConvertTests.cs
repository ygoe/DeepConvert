using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unclassified.Util
{
	[TestClass]
	public class DeepConvertTests
	{
		[TestMethod]
		public void ChangeType_Numerics()
		{
			var ics = new DeepConvertSettings
			{
				Provider = CultureInfo.InvariantCulture,
				Format = "0.00"
			};
			var decs = new DeepConvertSettings
			{
				Provider = CultureInfo.GetCultureInfo("de-DE"),
				Format = "0.00"
			};

			Assert.AreEqual(20, DeepConvert.ChangeType<byte>(20));
			Assert.AreEqual("20", DeepConvert.ChangeType<string>(20.0));
			Assert.AreEqual("20.00", DeepConvert.ChangeType<string>(20.0, ics));
			Assert.AreEqual("20,00", DeepConvert.ChangeType<string>(20.0, decs));
			Assert.AreEqual(false, DeepConvert.ChangeType<bool>(0));
			Assert.AreEqual(true, DeepConvert.ChangeType<bool>(20));
			Assert.AreEqual(false, DeepConvert.ChangeType<bool>(0.0));
			Assert.AreEqual(true, DeepConvert.ChangeType<bool>(20.0));
			Assert.AreEqual(1, DeepConvert.ChangeType<byte>(true));
			Assert.AreEqual(100, DeepConvert.ChangeType<float>(100));
			Assert.AreEqual(100, DeepConvert.ChangeType<float>("100"));
			Assert.AreEqual(100, DeepConvert.ChangeType<float>("100.0", ics));
			Assert.AreEqual(100, DeepConvert.ChangeType<double>("100"));
			Assert.AreEqual(100, DeepConvert.ChangeType<double>("100.0", ics));
			Assert.AreEqual(100, DeepConvert.ChangeType<decimal>("100"));
			Assert.AreEqual(100, DeepConvert.ChangeType<decimal>("100.0", ics));
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

		[TestMethod]
		public void ChangeType_TimeSpan()
		{
			var timeSpan = TimeSpan.FromSeconds(62);

			Assert.AreEqual("00:01:02", DeepConvert.ChangeType<string>(timeSpan));
			Assert.AreEqual("00:01:02", DeepConvert.ChangeType<string>(timeSpan, new DeepConvertSettings { Format = "c" }));
			Assert.AreEqual("1:02.000", DeepConvert.ChangeType<string>(timeSpan, new DeepConvertSettings { Format = @"m\:ss\.fff" }));
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
			Assert.AreEqual(Enum1.Zero1, DeepConvert.ChangeType<Enum1>(null));
			Assert.AreEqual(null, DeepConvert.ChangeType<Enum1?>(null));
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
		public void ChangeType_ArrayToListWithNull()
		{
			var stringList = DeepConvert.ChangeType<List<string>>(new object[] { null, "item2" });
			Assert.AreEqual(typeof(List<string>), stringList.GetType());
			Assert.AreEqual(2, stringList.Count);
			Assert.AreEqual(null, stringList[0]);
			Assert.AreEqual("item2", stringList[1]);
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
		public void ChangeType_DictionaryObjectArray()
		{
			var dict = new Dictionary<object, object>
			{
				["a"] = new object[0]
			};
			var result = DeepConvert.ChangeType<Dictionary<string, object>>(dict);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(typeof(object[]), result["a"].GetType());
			Assert.AreEqual(0, ((object[])result["a"]).Length);
		}

		[TestMethod]
		public void ChangeType_Exponential()
		{
			var ics = new DeepConvertSettings
			{
				Provider = CultureInfo.InvariantCulture
			};
			var decs = new DeepConvertSettings
			{
				Provider = new CultureInfo("de-DE")
			};

			Assert.AreEqual(100, DeepConvert.ChangeType<float>("1.00E+02", ics));
			Assert.AreEqual(100, DeepConvert.ChangeType<double>("1.00E+02", ics));
			Assert.AreEqual(100, DeepConvert.ChangeType<decimal>("1.00E+02", ics));
			Assert.AreEqual(100, DeepConvert.ChangeType<decimal>("1,00E+02", decs));
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
			var date = DeepConvert.ToDateTime(2 * 86400, new DeepConvertSettings { DateNumericKind = DateNumericKind.UnixSeconds });
			Assert.AreEqual(new DateTime(1970, 1, 3, 0, 0, 0), date);
			Assert.AreEqual(DateTimeKind.Utc, date.Kind);
		}

		[TestMethod]
		public void ToDateTime_NumericUnixMilliseconds()
		{
			var date = DeepConvert.ToDateTime(3 * 86400 * 1000, new DeepConvertSettings { DateNumericKind = DateNumericKind.UnixMilliseconds });
			Assert.AreEqual(new DateTime(1970, 1, 4, 0, 0, 0), date);
			Assert.AreEqual(DateTimeKind.Utc, date.Kind);
		}

		[TestMethod]
		public void ToDateTime_DoubleUnixMilliseconds()
		{
			var date = DeepConvert.ToDateTime(3.5 * 86400 * 1000, new DeepConvertSettings { DateNumericKind = DateNumericKind.UnixMilliseconds });
			Assert.AreEqual(new DateTime(1970, 1, 4, 12, 0, 0), date);
			Assert.AreEqual(DateTimeKind.Utc, date.Kind);
		}

		[TestMethod]
		public void ToDateTime_StringFormat()
		{
			var date = DeepConvert.ToDateTime("18.02.03 04.05.06", new DeepConvertSettings { Format = "yy.MM.dd HH.mm.ss" });
			Assert.AreEqual(new DateTime(2018, 2, 3, 4, 5, 6), date);
			Assert.AreEqual(DateTimeKind.Unspecified, date.Kind);
		}

		[TestMethod]
		public void ToDateTime_StringFormatLocal()
		{
			var date = DeepConvert.ToDateTime("18.02.03 04.05.06", new DeepConvertSettings { Format = "yy.MM.dd HH.mm.ss", DateTimeStyles = DateTimeStyles.AssumeLocal });
			Assert.AreEqual(new DateTime(2018, 2, 3, 4, 5, 6), date);
			Assert.AreEqual(DateTimeKind.Local, date.Kind);
		}

		[TestMethod]
		public void ToDateTime_StringUtcToLocal()
		{
			var date = DeepConvert.ToDateTime("2018-02-03 04:05:06", new DeepConvertSettings { DateTimeStyles = DateTimeStyles.AssumeUniversal });
			Assert.AreEqual(new DateTime(2018, 2, 3, 4, 5, 6, DateTimeKind.Utc).ToLocalTime(), date);
			Assert.AreEqual(DateTimeKind.Local, date.Kind);
		}

		[TestMethod]
		public void ToDateTime_StringLocalToUtc()
		{
			var date = DeepConvert.ToDateTime("2018-02-03 04:05:06", new DeepConvertSettings { DateTimeStyles = DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal });
			Assert.AreEqual(new DateTime(2018, 2, 3, 4, 5, 6, DateTimeKind.Local).ToUniversalTime(), date);
			Assert.AreEqual(DateTimeKind.Utc, date.Kind);
		}

		[TestMethod]
		public void ToDateTime_NestedStringFormatLocal()
		{
			var dateList = DeepConvert.ChangeType<List<DateTime>>(
				new object[] { 10 * 86400, "18.02.03 04.05.06" },
				new DeepConvertSettings
				{
					Format = "yy.MM.dd HH.mm.ss",
					DateNumericKind = DateNumericKind.UnixSeconds,
					DateTimeStyles = DateTimeStyles.AssumeLocal
				});
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
				new DeepConvertSettings
				{
					Provider = CultureInfo.InvariantCulture,
					Format = "yyyyMMddHHmmss",
					DateNumericKind = DateNumericKind.None,
					DateTimeStyles = DateTimeStyles.None
				});
			Assert.AreEqual(typeof(DateTime), date.GetType());
			Assert.AreEqual(new DateTime(2010, 1, 1, 0, 0, 3), date);
			Assert.AreEqual(DateTimeKind.Unspecified, date.Kind);
		}

		[TestMethod]
		public void ChangeType_CharsToString()
		{
			char[] chars = new char[] { 'A', 'ö', 'b', 'c', '€' };
			string str = DeepConvert.ChangeType<string>(chars);
			Assert.AreEqual("Aöbc€", str);

			int[] numbers = new int[] { 0x41, 0xf6, 0x62, 0x63, 0x20ac };
			str = DeepConvert.ChangeType<string>(numbers);
			Assert.AreEqual("Aöbc€", str);

			object[] mixed = new object[] { "A", 'ö', 0x62, "c", "€" };   // numeric strings not supported, must be single characters
			str = DeepConvert.ChangeType<string>(mixed);
			Assert.AreEqual("Aöbc€", str);
		}

		[TestMethod]
		public void ChangeType_StringToChars()
		{
			string str = "Aöbc€";
			var chars = DeepConvert.ChangeType<List<char>>(str);
			Assert.AreEqual(5, chars.Count);
			Assert.AreEqual('A', chars[0]);
			Assert.AreEqual('ö', chars[1]);
			Assert.AreEqual('b', chars[2]);
			Assert.AreEqual('c', chars[3]);
			Assert.AreEqual('€', chars[4]);
		}

		[TestMethod]
		public void ChangeType_BytesToString()
		{
			byte[] bytes = new byte[] { /*A*/ 0x41, /*ö*/ 0xc3, 0xb6, /*b*/ 0x62, /*c*/ 0x63, /*€*/ 0xe2, 0x82, 0xac };
			string str = DeepConvert.ChangeType<string>(bytes);   // default UTF-8
			Assert.AreEqual("Aöbc€", str);

			bytes = new byte[] { /*A*/ 0x41, 0, /*ö*/ 0xf6, 0, /*b*/ 0x62, 0, /*c*/ 0x63, 0, /*€*/ 0xac, 0x20 };
			str = DeepConvert.ChangeType<string>(bytes, new DeepConvertSettings { Encoding = Encoding.Unicode });
			Assert.AreEqual("Aöbc€", str);
		}

		[TestMethod]
		public void ChangeType_StringToBytes()
		{
			string str = "Aöbc€";
			var bytes = DeepConvert.ChangeType<List<byte>>(str);   // default UTF-8
			Assert.AreEqual(8, bytes.Count);
			Assert.AreEqual(0x41, bytes[0]);   // A
			Assert.AreEqual(0xc3, bytes[1]);   // ö
			Assert.AreEqual(0xb6, bytes[2]);
			Assert.AreEqual(0x62, bytes[3]);   // b
			Assert.AreEqual(0x63, bytes[4]);   // c
			Assert.AreEqual(0xe2, bytes[5]);   // €
			Assert.AreEqual(0x82, bytes[6]);
			Assert.AreEqual(0xac, bytes[7]);

			byte[] bytesArr = DeepConvert.ChangeType<byte[]>(str, new DeepConvertSettings { Encoding = Encoding.Unicode });
			Assert.AreEqual(10, bytesArr.Length);
			Assert.AreEqual(0x41, bytesArr[0]);   // A
			Assert.AreEqual(0, bytesArr[1]);
			Assert.AreEqual(0xf6, bytesArr[2]);   // ö
			Assert.AreEqual(0, bytesArr[3]);
			Assert.AreEqual(0x62, bytesArr[4]);   // b
			Assert.AreEqual(0, bytesArr[5]);
			Assert.AreEqual(0x63, bytesArr[6]);   // c
			Assert.AreEqual(0, bytesArr[7]);
			Assert.AreEqual(0xac, bytesArr[8]);   // €
			Assert.AreEqual(0x20, bytesArr[9]);
		}

		public class DataClass
		{
			public bool Flag { get; set; }
			public int Number { get; set; }
			public string Text { get; set; }
			public int[] Numbers { get; set; }
			public List<float> Floats { get; set; }
			public SubClass Sub { get; set; }
		}

		public class DataClass2
		{
#pragma warning disable IDE1006   // Lower case member name is intentional for testing purposes
			public int flag { get; set; }
			public long number { get; set; }
#pragma warning restore IDE1006
			public string Text { get; set; }
			public double[] Floats { get; set; }
			public bool Add { get; set; } = true;
			public SubClass2 Sub { get; set; }
		}

		public class SubClass
		{
			public bool IsInitialized { get; set; }
			public Dictionary<string, string> Dict { get; set; }
		}

		public class SubClass2
		{
			public int IsInitialized { get; set; }
			public SubDict2 Dict { get; set; }

			public class SubDict2
			{
				public short A { get; set; }
				public short B { get; set; }
				public short C { get; set; }
			}
		}

		[TestMethod]
		public void ConvertToObject()
		{
			var dict = new Dictionary<object, object>
			{
				["flag"] = true,
				["number"] = 42,
				["Text"] = "Hello",
				["Numbers"] = new[] { 1.0, 2.0, 3.6 },
				["Floats"] = new[] { "3.14", "2.71" },
				["Sub"] = new Dictionary<string, object>
				{
					["IsInitialized"] = true,
					["Dict"] = new Dictionary<string, object>
					{
						["A"] = 1,
						["B"] = 2,
						["C"] = 3
					}
				}
			};
			//var data = DeepConvert.ConvertToObject<DataClass>(dict, new DeepConvertSettings { Provider = CultureInfo.InvariantCulture });
			var data = DeepConvert.ChangeType<DataClass>(dict, new DeepConvertSettings { Provider = CultureInfo.InvariantCulture });

			Assert.AreEqual(true, data.Flag);
			Assert.AreEqual(42, data.Number);
			Assert.AreEqual("Hello", data.Text);

			Assert.AreEqual(3, data.Numbers.Length);
			Assert.AreEqual(1, data.Numbers[0]);
			Assert.AreEqual(2, data.Numbers[1]);
			Assert.AreEqual(4, data.Numbers[2]);

			Assert.AreEqual(2, data.Floats.Count);
			Assert.AreEqual(3.14f, data.Floats[0]);
			Assert.AreEqual(2.71f, data.Floats[1]);

			Assert.AreEqual(true, data.Sub.IsInitialized);

			Assert.AreEqual(3, data.Sub.Dict.Count);
			Assert.AreEqual("1", data.Sub.Dict["A"]);
			Assert.AreEqual("2", data.Sub.Dict["B"]);
			Assert.AreEqual("3", data.Sub.Dict["C"]);
		}

		[TestMethod]
		public void ConvertToDictionary()
		{
			var data = new DataClass
			{
				Flag = true,
				Number = 42,
				Text = "Hello",
				Numbers = new[] { 1, 2, 4 },
				Floats = new List<float> { 3.14f, 2.71f },
				Sub = new SubClass
				{
					IsInitialized = true,
					Dict = new Dictionary<string, string>
					{
						["A"] = "1",
						["B"] = "2",
						["C"] = "3"
					}
				}
			};
			//var dict = DeepConvert.ConvertToDictionary(data);
			var dict = DeepConvert.ChangeType<Dictionary<string, object>>(data);

			Assert.AreEqual(true, dict["Flag"]);
			Assert.AreEqual(42, dict["Number"]);
			Assert.AreEqual("Hello", dict["Text"]);

			Assert.AreEqual(3, ((int[])dict["Numbers"]).Length);
			Assert.AreEqual(1, ((int[])dict["Numbers"])[0]);
			Assert.AreEqual(2, ((int[])dict["Numbers"])[1]);
			Assert.AreEqual(4, ((int[])dict["Numbers"])[2]);

			Assert.AreEqual(2, ((List<float>)dict["Floats"]).Count);
			Assert.AreEqual(3.14f, ((List<float>)dict["Floats"])[0]);
			Assert.AreEqual(2.71f, ((List<float>)dict["Floats"])[1]);

			var sub = (SubClass)dict["Sub"];
			Assert.AreEqual(true, sub.IsInitialized);

			Assert.AreEqual(3, sub.Dict.Count);
			Assert.AreEqual("1", sub.Dict["A"]);
			Assert.AreEqual("2", sub.Dict["B"]);
			Assert.AreEqual("3", sub.Dict["C"]);
		}

		[TestMethod]
		public void ConvertBetweenObjects()
		{
			var data = new DataClass
			{
				Flag = true,
				Number = 42,
				Text = "Hello",
				Numbers = new[] { 1, 2, 4 },
				Floats = new List<float> { 3.14f, 2.71f },
				Sub = new SubClass
				{
					IsInitialized = true,
					Dict = new Dictionary<string, string>
					{
						["A"] = "1",
						["B"] = "2",
						["C"] = "3"
					}
				}
			};
			var data2 = DeepConvert.ChangeType<DataClass2>(data);

			Assert.AreEqual(1, data2.flag);
			Assert.AreEqual(42, data2.number);
			Assert.AreEqual("Hello", data2.Text);
			Assert.AreEqual(true, data2.Add);

			Assert.AreEqual(2, data2.Floats.Length);
			Assert.AreEqual(3.14f, data2.Floats[0]);
			Assert.AreEqual(2.71f, data2.Floats[1]);

			Assert.AreEqual(1, data2.Sub.IsInitialized);

			Assert.AreEqual(1, data2.Sub.Dict.A);
			Assert.AreEqual(2, data2.Sub.Dict.B);
			Assert.AreEqual(3, data2.Sub.Dict.C);
		}
	}
}
