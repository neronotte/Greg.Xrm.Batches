using System;
using System.Linq;
using System.Reflection;
using Common.Logging;
using OfficeOpenXml;

namespace Greg.Xrm.Batches.Core.Parsing
{
	public class ExcelRowParser<T> : IExcelRowParser<T>
	{
		private readonly ILog log = LogManager.GetLogger<ExcelRowParser<T>>();
		private readonly Lazy<IPropertySetter[]> propertySetters;

		private readonly ExcelWorksheet ws;
		private readonly int headerRowIndex;

		public ExcelRowParser(ExcelWorksheet ws, int headerRowIndex)
		{
			this.ws = ws;
			this.headerRowIndex = headerRowIndex;
			this.propertySetters = new Lazy<IPropertySetter[]>(this.CreatePropertySetters);
		}


		protected virtual IPropertySetter[] CreatePropertySetters()
		{
			var setters = (from p in typeof(T).GetProperties()
						   let parseAttribute = p.GetCustomAttribute<ParseAttribute>()
						   where parseAttribute != null
						   select parseAttribute.CreateDataParser(p, this.ws, this.headerRowIndex)).ToArray();

			return setters;
		}


		public virtual bool TryFillProperties(ExcelRange cells, int rowIndex, T target)
		{
			try
			{
				var result = this.propertySetters.Value.Aggregate(true, (current, propertySetter) => propertySetter.TrySetProperty(cells, rowIndex, target) && current);
				return result;
			}
			catch (Exception ex)
			{
				this.log.Error($"{ex.GetType().FullName}: {ex.Message}", ex);
				return false;
			}
		}
	}
}