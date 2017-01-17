namespace Greg.Xrm.Batches.Core.Parsing
{
	public interface IStringConverter
	{
		bool TryConvert(string input, out string output);
	}
}
