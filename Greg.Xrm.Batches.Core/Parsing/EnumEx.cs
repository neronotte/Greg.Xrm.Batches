using System;
using System.ComponentModel;

namespace Greg.Xrm.Batches.Core.Parsing
{
	public static class EnumEx
	{
		public static bool TryGetValueFromDescription(Type enumType, string description, out object value)
		{
			foreach (var field in enumType.GetFields())
			{
				var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
				if (attribute != null)
				{
					if (attribute.Description == description)
					{
						value = field.GetValue(null);
						return true;
					}
				}
				else
				{
					if (field.Name == description)
					{
						value = field.GetValue(null);
						return true;
					}
				}
			}

			value = null;
			return false;
		}
	}
}