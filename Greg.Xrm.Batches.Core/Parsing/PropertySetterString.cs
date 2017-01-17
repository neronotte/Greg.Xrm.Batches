using System;
using System.Reflection;
using Common.Logging;
using OfficeOpenXml;

namespace Greg.Xrm.Batches.Core.Parsing
{
	public class PropertySetterString : IPropertySetter
	{
		private readonly ILog log = LogManager.GetLogger<PropertySetterString>();
		private readonly PropertyInfo propertyInfo;
		private readonly int columnIndex;
		private readonly Type stringConverterType;

		public PropertySetterString(PropertyInfo propertyInfo, int columnIndex, Type stringConverterType)
		{
			this.propertyInfo = propertyInfo;
			this.columnIndex = columnIndex;
			this.stringConverterType = stringConverterType;
		}

		private IStringConverter CreateStringConverter()
		{
			if (this.stringConverterType == null)
			{
				return new StringConverterIdentity();
			}

			if (typeof(IStringConverter).IsAssignableFrom(this.stringConverterType))
			{
				return (IStringConverter)Activator.CreateInstance(this.stringConverterType);
			}

			throw new ArgumentException($"The specified type <{this.stringConverterType.FullName}> is not a valid <IStringConverter>.");
		}



		public bool TrySetProperty(ExcelRange range, int rowIndex, object target)
		{
			var text = range[rowIndex, this.columnIndex].Text;

			if (string.IsNullOrWhiteSpace(text))
			{
				// do nothing
				return true;
			}

			var converter = this.CreateStringConverter();

			string output;
			if (!converter.TryConvert(text, out output))
			{
				throw new InvalidCastException( $"The specified value <{text}> cannot be converted using <{converter.GetType().Name}> for property <{this.propertyInfo.Name}>.");
			}

			output = output.Trim();

			this.propertyInfo.SetValue(target, output);
			return true;

		}
	}
}