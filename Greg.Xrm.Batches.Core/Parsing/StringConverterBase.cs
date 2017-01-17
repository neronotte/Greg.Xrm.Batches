using System.Collections.Generic;

namespace Greg.Xrm.Batches.Core.Parsing
{
	public abstract class StringConverterBase : IStringConverter
	{
		private Dictionary<string, string> valueMap;
		 
		public virtual bool TryConvert(string input, out string output)
		{
			if (this.valueMap == null)
			{
				this.valueMap = new Dictionary<string, string>();
				this.Initialize();
			}

			if (!this.valueMap.TryGetValue(input, out output))
			{
				return false;
			}
			return true;
		}

		protected void Map(string source, string target)
		{
			this.valueMap[source] = target;
		}

		public abstract void Initialize();
	}
}