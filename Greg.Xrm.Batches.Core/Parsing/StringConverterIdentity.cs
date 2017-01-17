namespace Greg.Xrm.Batches.Core.Parsing
{
	public class StringConverterIdentity : IStringConverter
	{
		public virtual bool TryConvert(string input, out string output)
		{
			output = input;
			return true;
		}
	}
}