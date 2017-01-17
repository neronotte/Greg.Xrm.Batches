namespace Avanade.Rina.Batches.Core.Common
{
	public static class Result
	{
		public static Result<T> Succeeded<T>(T item)
		{
			return Result<T>.Succeeded(item);
		}
		public static Result<T> Error<T>(string errorMessage)
		{
			return Result<T>.Error(errorMessage);
		}
	}


	public class Result<T>
	{
		public static Result<T> Succeeded(T item)
		{
			return new Result<T>
			{
				Value = item,
				HasError = false,
				ErrorMessage = string.Empty
			};
		}


		public static Result<T> Error(string errorMessage)
		{
			return new Result<T>
			{
				Value = default(T),
				HasError = true,
				ErrorMessage = errorMessage
			};
		}

		protected Result()
		{
		}

		/// <summary>
		/// Gets the result of the operation
		/// </summary>
		public T Value { get; protected set; }

		/// <summary>
		/// Returns <c>True</c> if the operation returned an error, <c>False</c> otherwise.
		/// </summary>
		public bool HasError { get; protected set; }

		/// <summary>
		/// Returns the error message returned by the operation, or an 
		/// empty string if the operation completed successfully.
		/// </summary>
		public string ErrorMessage { get; protected set; }
	}
}
