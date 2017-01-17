using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Common.Logging;
using OfficeOpenXml;

namespace Greg.Xrm.Batches.Core.Parsing
{
	public class PropertySetterDecimal : IPropertySetter
	{
		private readonly ILog log = LogManager.GetLogger<PropertySetterDecimal>();
		private readonly PropertyInfo propertyInfo;
		private readonly int columnIndex;
		private readonly bool allowNulls;
		

		public PropertySetterDecimal(PropertyInfo propertyInfo, int columnIndex, bool allowNulls)
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
			    var defaultValueAttribute = this.propertyInfo.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(DefaultValueAttribute));
			    if (defaultValueAttribute != null)
			    {
                    var value = defaultValueAttribute.ConstructorArguments.FirstOrDefault().Value;
                    cellValue = Convert.ToDecimal(value);
			    }
			    else
			    {
			        this.log.Warn($"Error while setting property {this.propertyInfo.Name} (col: {this.columnIndex}). Value cannot be null (<System.Decimal> is required).");
			        return false;
			    }
			}

			var cellValueType = cellValue.GetType();
			if (!typeof(decimal).IsAssignableFrom(cellValueType))
			{
				if (typeof(double).IsAssignableFrom(cellValueType))
				{
					cellValue = Convert.ToDecimal(cellValue);
				}
				else
				{
					this.log.Warn($"Error while setting property {this.propertyInfo.Name} (col: {this.columnIndex}). Value <{cellValue}> type is <{cellValueType}> while a <System.Decimal> is required.");
					return false;
				}
			}

			this.propertyInfo.SetValue(target, cellValue);
			return true;
		}
	}
}