using System;
using System.Reflection;
using Common.Logging;
using OfficeOpenXml;

namespace Greg.Xrm.Batches.Core.Parsing
{
	public class PropertySetterEnum : IPropertySetter
	{
		private readonly ILog log = LogManager.GetLogger<PropertySetterEnum>();
		private readonly PropertyInfo propertyInfo;
		private readonly int columnIndex;
		private readonly bool allowNulls;
		private readonly Type propertyType;

		public PropertySetterEnum(PropertyInfo propertyInfo, int columnIndex, bool allowNulls)
		{
			this.propertyInfo = propertyInfo;
			this.columnIndex = columnIndex;
			this.allowNulls = allowNulls;
		}

		public PropertySetterEnum(PropertyInfo propertyInfo, Type propertyType, int columnIndex, bool allowNulls)
		{
			this.propertyInfo = propertyInfo;
			this.propertyType = propertyType;
			this.columnIndex = columnIndex;
			this.allowNulls = allowNulls;
		}

		public bool TrySetProperty(ExcelRange range, int rowIndex, object target)
		{
			var enumValueDescription = range[rowIndex, this.columnIndex].Text?.Trim();

			if (this.allowNulls && string.IsNullOrWhiteSpace(enumValueDescription))
			{
				// do nothing, simply exit
				return true;
			}

			object value;
			if (!EnumEx.TryGetValueFromDescription(this.propertyType, enumValueDescription, out value))
			{
				this.log.Warn($"Error while setting property {this.propertyInfo.Name} (col: {this.columnIndex}). Value <{enumValueDescription}> not found in the enum {this.propertyInfo.PropertyType}");
				return false;
			}

			this.propertyInfo.SetValue(target, value);
			return true;
		}
	}
}