using System;
using System.Linq;
using System.Reflection;
using Common.Logging;
using OfficeOpenXml;

namespace Greg.Xrm.Batches.Core.Parsing
{
	public class PropertySetterBool : IPropertySetter
	{
		private static readonly string[] TrueValues = { "Y", "YES", "True", "1", "T", "S", "SI" };
		private static readonly string[] FalseValues = { "N", "NO", "False", "0", "F", "N", string.Empty};

		private readonly ILog log = LogManager.GetLogger<PropertySetterBool>();
		private readonly PropertyInfo propertyInfo;
		private readonly int columnIndex;
		private readonly bool allowNulls;
		
		public PropertySetterBool(PropertyInfo propertyInfo, int columnIndex, bool allowNulls)
		{
			this.propertyInfo = propertyInfo;
			this.columnIndex = columnIndex;
			this.allowNulls = allowNulls;
		}

		public bool TrySetProperty(ExcelRange range, int rowIndex, object target)
		{
			var stringValue = (range[rowIndex, this.columnIndex].Text ?? string.Empty).Trim();

			if (this.allowNulls && string.IsNullOrWhiteSpace(stringValue))
			{
				// do nothing, simply exit
				return true;
			}


			if (TrueValues.Any(trueValue => string.Equals(stringValue, trueValue, StringComparison.InvariantCultureIgnoreCase)))
			{
				this.propertyInfo.SetValue(target, true);
				return true;
			}

			if (FalseValues.Any(trueValue => string.Equals(stringValue, trueValue, StringComparison.InvariantCultureIgnoreCase)))
			{
				this.propertyInfo.SetValue(target, false);
				return true;
			}

			this.log.Warn($"Error while setting property {this.propertyInfo.Name} (col: {this.columnIndex}). Value <{stringValue}> is not a valid boolean.");
			return false;
		}
	}
}