using System;
using System.Reflection;
using System.Runtime.Serialization;
using OfficeOpenXml;

namespace Greg.Xrm.Batches.Core.Parsing
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ParseAttribute : Attribute
	{

		public ParseAttribute(int columnIndex)
		{
			if (columnIndex <= 0)
				throw new ArgumentOutOfRangeException(nameof(columnIndex), nameof(columnIndex) + " must be > 0");

			this.ColumnHeader = null;
			this.ColumnIndex = columnIndex;
		}

		public ParseAttribute(string columnHeader = null)
		{
			if (string.IsNullOrWhiteSpace(columnHeader))
				throw new ArgumentNullException(nameof(columnHeader));

			this.ColumnHeader = columnHeader.Trim();
			this.ColumnIndex = -1;
		}

		public string ColumnHeader { get; }

		public int ColumnIndex { get; private set; }

		public Type StringConverter { get; set; }


		public IPropertySetter CreateDataParser(PropertyInfo propertyInfo, ExcelWorksheet worksheet, int headerRow)
		{
			var propertyType = propertyInfo.PropertyType;

			if (this.ColumnIndex < 0)
			{
				var cellHeader = this.ColumnHeader ?? propertyInfo.Name;
				this.ColumnIndex = this.FindColumnIndex(worksheet, headerRow, cellHeader);
			}


			var nullable = typeof(Nullable<>);
			bool allowNulls = false;
			if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == nullable)
			{
				propertyType = propertyType.GetGenericArguments()[0];
				allowNulls = true;
			}



			if (propertyType == typeof (string))
			{
				return new PropertySetterString(propertyInfo, this.ColumnIndex, this.StringConverter);
			}

			if (propertyType.IsEnum)
			{
				return new PropertySetterEnum(propertyInfo, propertyType, this.ColumnIndex, allowNulls);
			}

			if (propertyType == typeof(bool))
			{
				return new PropertySetterBool(propertyInfo, this.ColumnIndex, allowNulls);
			}

			if (propertyType == typeof(int))
			{
				return new PropertySetterInt(propertyInfo, this.ColumnIndex, allowNulls);
			}

			if (propertyType == typeof(decimal))
			{
				return new PropertySetterDecimal(propertyInfo, this.ColumnIndex, allowNulls);
			}

			if (propertyType == typeof(DateTime))
			{
				return new PropertySetterDateTime(propertyInfo, this.ColumnIndex, allowNulls);
			}


			throw new NotSupportedException("Property type not supported: " + propertyType);
		}

		private int FindColumnIndex(ExcelWorksheet worksheet, int row, string cellHeader)
		{
			for (var col = 1; col <= worksheet.Dimension.Columns; col++)
			{
				var rowValue = worksheet.Cells[row, col].Text?.Trim();
				if (string.Equals(rowValue, cellHeader, StringComparison.InvariantCultureIgnoreCase))
					return col;
			}

			throw new HeaderNotFoundException(this.ColumnHeader);
		}
	}


	[Serializable]
	public class HeaderNotFoundException : Exception
	{
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//


		public HeaderNotFoundException(string header) : base("Header not found: " + header)
		{
			this.Header = header;
		}

		public HeaderNotFoundException(string header, string message, Exception inner) : base(message, inner)
		{
			this.Header = header;
		}

		protected HeaderNotFoundException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}

		public string Header { get; private set; }
	}
}