using Microsoft.Practices.Unity;

namespace Greg.Xrm.Batches.Core.IoC
{
	public interface IUnityConfigurator
	{
		void Configure(IUnityContainer container);
	}
}