using System;

namespace Unclassified.Util
{
#if !NET5_0_OR_GREATER
	internal class DynamicallyAccessedMembersAttribute : Attribute
	{
		public DynamicallyAccessedMembersAttribute(DynamicallyAccessedMemberTypes types)
		{
		}
	}

	[Flags]
	internal enum DynamicallyAccessedMemberTypes
	{
		PublicConstructors = 3,
		PublicParameterlessConstructor = 1,
		PublicProperties = 512,
		PublicMethods = 8
	}
#endif
}
