using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

// Expression will always cause a System.NullReferenceException because the default value of
// 'generic type' is null
#pragma warning disable 1720

namespace Unclassified.Util
{
	internal class ConvertTypeInfo
	{
		#region Static members

		private static readonly AssemblyBuilder assemblyBuilder;
		private static readonly ModuleBuilder moduleBuilder;

		static ConvertTypeInfo()
		{
			assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
				new AssemblyName() { Name = Guid.NewGuid().ToString() },
				AssemblyBuilderAccess.Run);
			moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyBuilder.GetName().Name);
		}

		#endregion Static members

		#region Private data

		private readonly string[] names;
		private readonly Func<object, string, object> getter;
		private readonly Action<object, string, object, DeepConvertSettings> setter;

		#endregion Private data

		#region Constructors

		public ConvertTypeInfo(Type type)
		{
			Type = type;

			var namesList = new List<string>();
			var typeBuilder = moduleBuilder.DefineType(type.Name + "Converter", TypeAttributes.Class | TypeAttributes.Public);

			var gettableProperties = new List<PropertyInfo>();
			var settableProperties = new List<PropertyInfo>();

			foreach (PropertyInfo propertyInfo in GetAllProperties(type))
			{
				bool added = false;
				if (propertyInfo.GetGetMethod() != null)
				{
					gettableProperties.Add(propertyInfo);
					added = true;
				}
				if (propertyInfo.GetSetMethod() != null)
				{
					settableProperties.Add(propertyInfo);
					added = true;
				}
				if (added)
				{
					namesList.Add(propertyInfo.Name);
				}
			}

			BuildGetMethod(typeBuilder, gettableProperties);
			BuildSetMethod(typeBuilder, settableProperties);

			names = namesList.ToArray();

			var converterType = typeBuilder.CreateTypeInfo();
			getter = (Func<object, string, object>)converterType.GetMethod("Get")
				.CreateDelegate(typeof(Func<object, string, object>));
			setter = (Action<object, string, object, DeepConvertSettings>)converterType.GetMethod("Set")
				.CreateDelegate(typeof(Action<object, string, object, DeepConvertSettings>));
		}

		#endregion Constructors

		#region Public members

		public Type Type { get; }

		public object CreateInstance() => Activator.CreateInstance(Type);

		public string[] GetNames() => names;

		public object GetValue(object instance, string name)
		{
			return getter(instance, name);
		}

		public void SetValue(object instance, string name, object value, DeepConvertSettings settings)
		{
			setter(instance, name, value, settings);
		}

		#endregion Public members

		#region Private methods

		// From Unclassified.Util.TypeExtensions.GetAllProperties method
		private static PropertyInfo[] GetAllProperties(Type type)
		{
			var properties = new List<PropertyInfo>();
			do
			{
				// Only consider properties declared in this type directly
				foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly))
				{
					properties.Add(property);
				}
				// Also include properties from the base type (recursive)
				type = type.BaseType;
			}
			while (type != null);
			return properties.ToArray();
		}

		private void BuildGetMethod(TypeBuilder typeBuilder, List<PropertyInfo> properties)
		{
			MethodBuilder methodBuilder = typeBuilder.DefineMethod(
				"Get",
				MethodAttributes.Public | MethodAttributes.Static,
				typeof(object),
				new[]
				{
					typeof(object),   // instance
					typeof(string)    // property name
				});

			ILGenerator ilGen = methodBuilder.GetILGenerator();
			ilGen.DeclareLocal(Type);   // typedInstance (local index 0)
			ilGen.DeclareLocal(typeof(string));   // lowerName (local index 1)

			// var typedInstance = (MyType)instance;
			ilGen.Emit(OpCodes.Ldarg_0);
			ilGen.Emit(OpCodes.Castclass, Type);
			ilGen.Emit(OpCodes.Stloc_0);

			// string lowerName = name.ToLowerInvariant();
			ilGen.Emit(OpCodes.Ldarg_1);
			ilGen.EmitCall(OpCodes.Callvirt, MethodOf(() => default(string).ToLowerInvariant()), null);
			ilGen.Emit(OpCodes.Stloc_1);

			var labels = new List<Label>();
			foreach (var propertyInfo in properties)
			{
				string lowerName = propertyInfo.Name.ToLowerInvariant();
				// if (lowerName == "propertyname")
				ilGen.Emit(OpCodes.Ldloc_1);
				ilGen.Emit(OpCodes.Ldstr, lowerName);
				ilGen.EmitCall(OpCodes.Call, typeof(string).GetMethod("op_Equality", new[] { typeof(string), typeof(string) }), null);

				// goto where property is fetched
				var label = ilGen.DefineLabel();
				labels.Add(label);
				ilGen.Emit(OpCodes.Brtrue, label);
			}

			// return null;
			ilGen.Emit(OpCodes.Ldnull);
			var endLabel = ilGen.DefineLabel();
			ilGen.Emit(OpCodes.Br, endLabel);

			for (int i = 0; i < properties.Count; i++)
			{
				var propertyInfo = properties[i];
				var label = labels[i];

				// where property is fetched:
				ilGen.MarkLabel(label);

				// return typedInstance.PropertyName;
				ilGen.Emit(OpCodes.Ldloc_0);
				ilGen.EmitCall(OpCodes.Callvirt, propertyInfo.GetGetMethod(), null);
				if (propertyInfo.PropertyType.IsValueType)
				{
					ilGen.Emit(OpCodes.Box, propertyInfo.PropertyType);
				}
				ilGen.Emit(OpCodes.Br, endLabel);
			}

			ilGen.MarkLabel(endLabel);
			ilGen.Emit(OpCodes.Ret);
		}

		private void BuildSetMethod(TypeBuilder typeBuilder, List<PropertyInfo> properties)
		{
			MethodBuilder methodBuilder = typeBuilder.DefineMethod(
				"Set",
				MethodAttributes.Public | MethodAttributes.Static,
				typeof(void),
				new[]
				{
					typeof(object),   // instance
					typeof(string),   // property name
					typeof(object),   // value to set
					typeof(DeepConvertSettings)   // settings
				});

			ILGenerator ilGen = methodBuilder.GetILGenerator();
			ilGen.DeclareLocal(Type);   // typedInstance (local index 0)
			ilGen.DeclareLocal(typeof(string));   // lowerName (local index 1)

			// var typedInstance = (MyType)instance;
			ilGen.Emit(OpCodes.Ldarg_0);
			ilGen.Emit(OpCodes.Castclass, Type);
			ilGen.Emit(OpCodes.Stloc_0);

			// string lowerName = name.ToLowerInvariant();
			ilGen.Emit(OpCodes.Ldarg_1);
			ilGen.EmitCall(OpCodes.Callvirt, MethodOf(() => default(string).ToLowerInvariant()), null);
			ilGen.Emit(OpCodes.Stloc_1);

			var labels = new List<Label>();
			foreach (var propertyInfo in properties)
			{
				string lowerName = propertyInfo.Name.ToLowerInvariant();
				// if (lowerName == "propertyname")
				ilGen.Emit(OpCodes.Ldloc_1);
				ilGen.Emit(OpCodes.Ldstr, lowerName);
				ilGen.EmitCall(OpCodes.Call, typeof(string).GetMethod("op_Equality", new[] { typeof(string), typeof(string) }), null);

				// goto where property is set
				var label = ilGen.DefineLabel();
				labels.Add(label);
				ilGen.Emit(OpCodes.Brtrue, label);
			}

			// return;
			var endLabel = ilGen.DefineLabel();
			ilGen.Emit(OpCodes.Br, endLabel);

			for (int i = 0; i < properties.Count; i++)
			{
				var propertyInfo = properties[i];
				var label = labels[i];

				// where property is set:
				ilGen.MarkLabel(label);

				// typedInstance.PropertyName = DeepConvert.ChangeType(value, property's type);
				ilGen.Emit(OpCodes.Ldloc_0);
				ilGen.Emit(OpCodes.Ldarg_2);
				ilGen.Emit(OpCodes.Ldtoken, propertyInfo.PropertyType);
				ilGen.EmitCall(OpCodes.Call, MethodOf(() => Type.GetTypeFromHandle(default)), null);
				ilGen.Emit(OpCodes.Ldarg_3);
				ilGen.EmitCall(OpCodes.Call, MethodOf(() => DeepConvert.ChangeType(null, null, null)), null);
				if (propertyInfo.PropertyType.IsValueType)
				{
					ilGen.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
				}
				ilGen.EmitCall(OpCodes.Callvirt, propertyInfo.GetSetMethod(), null);
				ilGen.Emit(OpCodes.Br, endLabel);
			}

			ilGen.MarkLabel(endLabel);
			ilGen.Emit(OpCodes.Ret);
		}

		// Source: http://stackoverflow.com/q/1213862/143684
		private static MethodInfo MethodOf(Expression<Action> expression)
		{
			var body = (MethodCallExpression)expression.Body;
			return body.Method;
		}

		private static MethodInfo MethodOf<T>(Expression<Action<T>> expression)
		{
			var body = (MethodCallExpression)expression.Body;
			return body.Method;
		}

		private static MethodInfo MethodOf<T1, T2>(Expression<Action<T1, T2>> expression)
		{
			var body = (MethodCallExpression)expression.Body;
			return body.Method;
		}

		private static MethodInfo MethodOf<TResult>(Expression<Func<TResult>> expression)
		{
			var body = (MethodCallExpression)expression.Body;
			return body.Method;
		}

		private static MethodInfo MethodOf<T, TResult>(Expression<Func<T, TResult>> expression)
		{
			var body = (MethodCallExpression)expression.Body;
			return body.Method;
		}

		private static MethodInfo MethodOf<T1, T2, TResult>(Expression<Func<T1, T2, TResult>> expression)
		{
			var body = (MethodCallExpression)expression.Body;
			return body.Method;
		}

		#endregion Private methods

		#region Sample code

		// This code can be compiled and inspected in a decompiler to understand how the code to be
		// generated looks like.

		//public static object Get(object instance, string name)
		//{
		//	var typedInstance = (DataClass)instance;
		//	switch (name.ToLowerInvariant())
		//	{
		//		case "flag":
		//			return typedInstance.Flag;
		//		case "number":
		//			return typedInstance.Number;
		//		case "text":
		//			return typedInstance.Text;
		//		case "numbers":
		//			return typedInstance.Numbers;
		//		case "floats":
		//			return typedInstance.Floats;
		//	}
		//	return null;
		//}

		//public static void Set(object instance, string name, object value)
		//{
		//	var typedInstance = (DataClass)instance;
		//	switch (name.ToLowerInvariant())
		//	{
		//		case "flag":
		//			typedInstance.Flag = (bool)DeepConvert.ChangeType(value, typeof(bool));
		//			break;
		//		case "number":
		//			typedInstance.Number = (int)DeepConvert.ChangeType(value, typeof(int));
		//			break;
		//		case "text":
		//			typedInstance.Text = (string)DeepConvert.ChangeType(value, typeof(string));
		//			break;
		//		case "numbers":
		//			typedInstance.Numbers = (int[])DeepConvert.ChangeType(value, typeof(int[]));
		//			break;
		//		case "floats":
		//			typedInstance.Floats = (List<float>)DeepConvert.ChangeType(value, typeof(List<float>));
		//			break;
		//	}
		//}

		//public class DataClass
		//{
		//	public bool Flag { get; set; }
		//	public int Number { get; set; }
		//	public string Text { get; set; }
		//	public int[] Numbers { get; set; }
		//	public List<float> Floats { get; set; }
		//}

		#endregion Sample code
	}
}
