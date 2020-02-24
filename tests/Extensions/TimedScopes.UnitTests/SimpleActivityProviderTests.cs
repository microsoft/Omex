using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	[TestClass]
	public class SimpleActivityProviderTests
	{
		[TestMethod]
		public void CheckNameOfCreatedActivuty()
		{
			string testActivityName = "TestName";
			Activity activity = new SimpleActivityProvider().Create(testActivityName);

			Assert.IsNotNull(activity);

			string actualName = activity.OperationName;

			Assert.ReferenceEquals(testActivityName, actualName);
		}
	}


	[TestClass]
	public class TimedScopeTests
	{
		[TestMethod]
		public void A()
		{
			TimedScopeProvider provider = new TimedScope();
		}
	}


	[TestClass]
	public class TimedScopeProviderTests
	{
		[TestMethod]
		public void A()
		{
			TimedScopeProvider provider = new TimedScopeProvider();
		}
	}
}
