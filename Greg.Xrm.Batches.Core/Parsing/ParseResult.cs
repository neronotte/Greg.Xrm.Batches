using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Greg.Xrm.Batches.Core.Parsing
{
	public class ParseResult<T>
	{
		public ParseResult()
		{
			this.ValidRows = new List<T>();
			this.RowsWithImportErrors = new List<int>();
			this.RowsWithValidationErrors = new Dictionary<T, List<ValidationResult>>();
		}

		public List<T> ValidRows { get; }
		public List<int> RowsWithImportErrors { get; }
		public Dictionary<T, List<ValidationResult>> RowsWithValidationErrors { get; } 

		public int TotalRowCount => this.RowsWithImportErrors.Count + this.ValidRows.Count + this.RowsWithValidationErrors.Count;

		public int ErrorRowCount => this.RowsWithImportErrors.Count + this.RowsWithValidationErrors.Count;
	}
}