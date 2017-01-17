using System;
using System.Reflection;
using Common.Logging;
using OfficeOpenXml;

namespace Greg.Xrm.Batches.Core.Parsing
{
	public class PropertySetterDateTime : IPropertySetter
	{
		private readonly ILog log = LogManager.GetLogger<PropertySetterDateTime>();
		private readonly PropertyInfo propertyInfo;
		private readonly int columnIndex;
		private readonly bool allowNulls;

		public PropertySetterDateTime(PropertyInfo propertyInfo, int columnIndex, bool allowNulls)
		{
			this.propertyInfo = propertyInfo;
			this.columnIndex = columnIndex;
			this.allowNulls = allowNulls;
		}

		public bool TrySetProperty(ExcelRange range, int rowIndex, object target)
		{
			var cellValue = range[rowIndex, this.columnIndex].Value;

			if (this.allowNulls && (cellValue == null || Equals(cellValue, string.Empty)))
			{
				// do nothing
				return true;
			}
			if (cellValue == null)
			{
				this.log.Warn($"Error while setting property {this.propertyInfo.Name} (col: {this.columnIndex}). Value cannot be null (<System.DateTime> is required).");
				return false;
			}

			var cellValueType = cellValue.GetType();
			if (!typeof(DateTime).IsAssignableFrom(cellValueType))
			{
				this.log.Warn($"Error while setting property {this.propertyInfo.Name} (col: {this.columnIndex}). Value <{cellValue}> type is <{cellValueType}> while a <System.DateTime> is required.");
				return false;
			}

			this.propertyInfo.SetValue(target, cellValue);
			return true;
		}
	}
}