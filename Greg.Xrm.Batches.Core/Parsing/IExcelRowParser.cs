using OfficeOpenXml;

namespace Greg.Xrm.Batches.Core.Parsing
{
	public interface IExcelRowParser<T>
	{
		bool TryFillProperties(ExcelRange cells, int rowIndex, T target);
	}
}