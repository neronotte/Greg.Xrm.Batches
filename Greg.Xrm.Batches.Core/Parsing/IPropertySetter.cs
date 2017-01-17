using OfficeOpenXml;

namespace Greg.Xrm.Batches.Core.Parsing
{
	public interface IPropertySetter
	{
		bool TrySetProperty(ExcelRange range, int rowIndex, object target);
	}
}