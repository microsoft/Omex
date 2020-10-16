namespace Microsoft.Omex.Extensions.Hosting.Services.Remoting.UnitTests
{
	internal class MockServiceWithDependencies : IMockService
	{
		private readonly IMockServiceDependency m_dependency;

		public MockServiceWithDependencies(IMockServiceDependency dependency) =>
			m_dependency = dependency;
	}
}
