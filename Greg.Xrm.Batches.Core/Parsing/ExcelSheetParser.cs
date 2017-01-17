using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Common.Logging;
using OfficeOpenXml;

namespace Greg.Xrm.Batches.Core.Parsing
{
	public class ExcelSheetParser<T>
		where T : new()
	{
		private readonly ILog log = LogManager.GetLogger<ExcelSheetParser<T>>();
		private readonly Func<ExcelWorksheet, int, bool> defaultExitCondition;

		public ExcelSheetParser()
		{
			this.defaultExitCondition = (ws, row) => !string.IsNullOrWhiteSpace(ws.Cells[row, 1].Text);
		}

		public Func<T, ValidationContext> ValidationContextFactory { get; set; }

		private ValidationContext DefaultValidationContextFactory(T data)
		{
			return new ValidationContext(data);
		}


		public ParseResult<T> Parse(string fileName, string workSheetName, int firstRow = 1, Func<ExcelWorksheet, int, bool> exitCondition = null)
		{
			if (string.IsNullOrWhiteSpace(fileName))
				throw new ArgumentNullException(nameof(fileName));
			if (string.IsNullOrWhiteSpace(workSheetName))
				throw new ArgumentNullException(nameof(workSheetName));

			using (var package = new ExcelPackage())
			using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				package.Load(stream);
				var ws = package.Workbook.Worksheets[workSheetName];

				return this.Parse(ws, firstRow, exitCondition);
			}
		}


		public ParseResult<T> Parse(string fileName, int workSheetPosition = 1, int firstRow = 1, Func<ExcelWorksheet, int, bool> exitCondition = null)
		{
			if (string.IsNullOrWhiteSpace(fileName))
				throw new ArgumentNullException(nameof(fileName));
			if (workSheetPosition <= 0)
				throw new ArgumentOutOfRangeException(nameof(workSheetPosition), "workSheetPosition must be > 0");


			using (var package = new ExcelPackage())
			using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				package.Load(stream);
				var ws = package.Workbook.Worksheets[workSheetPosition];

				return this.Parse(ws, firstRow, exitCondition);
			}
		}
		

		public ParseResult<T> Parse(ExcelWorksheet worksheet, int firstRow = 1, Func<ExcelWorksheet, int, bool> exitCondition = null)
		{
			if (worksheet == null)
				throw new ArgumentNullException(nameof(worksheet));
			if (firstRow <= 0)
				throw new ArgumentOutOfRangeException(nameof(firstRow), "firstRow must be > 0");

			if (exitCondition == null)
				exitCondition = this.defaultExitCondition;


			var row = firstRow;
			var result = new ParseResult<T>();

			var parser = new ExcelRowParser<T>(worksheet, firstRow);
			row++;
			
			while (exitCondition(worksheet, row))
			{
				this.log.Debug($"Parsing line {row}...");

				var data = new T();

				var rowIndexProvider = data as IRowIndexProvider;
				if (rowIndexProvider != null)
				{
					rowIndexProvider.RowIndex = row;
				}

				if (!parser.TryFillProperties(worksheet.Cells, row, data))
				{
					this.log.Error($"Row {row} discarded.");
					result.RowsWithImportErrors.Add(row);
				    row++;
                    continue;
				}



				var validationResultList = new List<ValidationResult>();
				var validationContextFactory = this.ValidationContextFactory ?? this.DefaultValidationContextFactory;
				var validationContext = validationContextFactory(data);

				if (!Validator.TryValidateObject(data, validationContext, validationResultList))
				{
					var errorsText = validationResultList.Select(x => $"{string.Join(", ", x.MemberNames)}: {x.ErrorMessage}").FirstOrDefault();

					this.log.Error($"Row {row} discarded because of validation errors: {Environment.NewLine}{errorsText}");
					result.RowsWithValidationErrors.Add(data, validationResultList);
				    row++;
					continue;
				}

				result.ValidRows.Add(data);
				row++;
			}

			this.log.Info($"Total rows: {result.TotalRowCount}, total discarded: {result.RowsWithImportErrors.Count + result.RowsWithValidationErrors.Count}");

			return result;
		}
	}
}
