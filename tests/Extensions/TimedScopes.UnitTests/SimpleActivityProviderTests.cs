using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	[TestClass]
	public class SimpleActivityProviderTests
	{
		[DataTestMethod]
		[DataRow(null)]
		[DataRow("")]
		[DataRow("TestName")]
		public void CheckNameOfCreatedActivuty(string expectedName)
		{
			Activity activity = new SimpleActivityProvider().Create(expectedName);

			Assert.IsNotNull(activity);

			string actualName = activity.OperationName;

			Assert.ReferenceEquals(expectedName, actualName);
		}
	}


	[TestClass]
	public class TimedScopeTests
	{
		[TestMethod]
		public void A()
		{
			Mock<ITimedScopeEventSource> eventSource = new Mock<ITimedScopeEventSource>();
			Activity activity = new Activity("TestName");
			TimedScopeResult result = TimedScopeResult.SystemError;

			TimedScope scope = new TimedScope(eventSource.Object, activity, result);

			Assert.ReferenceEquals(activity, scope.Activity);
			Assert.AreEqual(result, scope.Result);
		}
	}
}
