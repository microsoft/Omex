// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Omex.System.Context;
using Microsoft.Omex.System.Diagnostics;
using Microsoft.Omex.System.TimedScopes;
using Microsoft.Omex.System.UnitTests.Shared.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = Xunit.Assert;

namespace Microsoft.Omex.System.AspNetCore.UnitTests
{
	/// <summary>
	/// Unit tests for Correlation
	/// </summary>
	[TestClass]
	public class CorrelationUnitTests : AspNetCoreUnitTestsBase
	{

		[TestInitialize]
		public void TestInitialize()
		{
			CallContextManagerInstance = CreateCallContextManager();
			CallContextManagerInstance.CallContextOverride = new HttpCallContext(useLogicalCallContext: true);

			MachineInformation = new UnitTestMachineInformation();

			HttpCallContext callContext = CallContextManagerInstance.CallContextHandler(MachineInformation) as HttpCallContext;
			callContext.ExistingCallContext();
			callContext.StartCallContext();
		}

		[TestMethod]
		public void CurrentCorrelation_WithNoActiveCorrelation_ShouldReturnNull()
		{
			Correlation = new Correlation(new MemoryCorrelationHandler(), CallContextManagerInstance, MachineInformation);
			Correlation.CorrelationStart(new CorrelationData());

			while (CallContextManagerInstance.CallContextHandler(MachineInformation).ExistingCallContext() != null)
			{
				CallContextManagerInstance.CallContextHandler(MachineInformation).EndCallContext();
			}

			Assert.Null(Correlation.CurrentCorrelation);
		}

		[TestMethod]
		public void CorrelationStart_WithNullData_ShouldStartNewCorrelation()
		{
			Correlation = new Correlation(new MemoryCorrelationHandler(), CallContextManagerInstance, MachineInformation);
			Correlation.CorrelationStart(null);

			Assert.NotNull(Correlation.CurrentCorrelation);

			CallContextManagerInstance.CallContextOverride.EndCallContext();
		}

		[TestMethod]
		public void CorrelationStart_MultipleCorrelations_ShouldCreateAHierarchy()
		{
			Correlation = new Correlation(new MemoryCorrelationHandler(), CallContextManagerInstance, MachineInformation);

			for (int i = 0; i < 3; i++)
			{
				Correlation.CorrelationStart(null);
				CorrelationData currentCorrelation = Correlation.CurrentCorrelation;
				for (int j = i; j > 0; j--)
				{
					CorrelationData parentCorrelation = currentCorrelation.ParentCorrelation;

					Assert.NotNull(parentCorrelation);
					Assert.NotEqual(currentCorrelation.VisibleId, parentCorrelation.VisibleId);

					currentCorrelation = parentCorrelation;
				}
				Assert.Null(currentCorrelation.ParentCorrelation);
			}

			CallContextManagerInstance.CallContextOverride.EndCallContext();
		}

		[TestMethod]
		public void CorrelationEnd_MultipleCorrelations_ShouldUnwindHierarchy()
		{
			Correlation = new Correlation(new MemoryCorrelationHandler(), CallContextManagerInstance, MachineInformation);

			Guid[] correlations = new Guid[3];
			for (int i = 0; i < 3; i++)
			{
				Correlation.CorrelationStart(null);
				correlations[i] = Correlation.CurrentCorrelation.VisibleId;
			}

			for (int i = 2; i >= 0; i--)
			{
				CorrelationData currentCorrelation = Correlation.CurrentCorrelation;
				Assert.Equal(correlations[i], currentCorrelation.VisibleId);

				Correlation.CorrelationEnd();
			}

			Assert.Null(Correlation.CurrentCorrelation);

			CallContextManagerInstance.CallContextOverride.EndCallContext();
		}

		[TestMethod]
		public void CorrelationData_SetParent_ShouldThrowIfAlreadySet()
		{
			CorrelationData data = new CorrelationData();
			data.ParentCorrelation = new CorrelationData();
			Assert.True(true, "Should not throw when the parent correlation is not set");

			Assert.Throws<InvalidOperationException>(
				() => data.ParentCorrelation = new CorrelationData());
		}

		[TestMethod]
		public void CorrelationData_SetParentCausingCircularReference_ShouldThrow()
		{
			CorrelationData data = new CorrelationData();
			data.ParentCorrelation = new CorrelationData();
			Assert.Throws<InvalidOperationException>(
				() => data.ParentCorrelation.ParentCorrelation = data);
		}

		[TestMethod]
		public void CorrelationData_CloneWithNullData_ShouldReturnNull()
		{
			CorrelationData data = null;
			Assert.Null(data.Clone());
		}

		[TestMethod]
		public void CorrelationData_Clone_ShouldReturnCopyOfCorrelationData()
		{
			CorrelationData data = new CorrelationData();
			data.ParentCorrelation = new CorrelationData();
			data.AddData(CorrelationData.TransactionIdKey, "1");
			data.ShouldLogDirectly = true;

			CorrelationData clone = data.Clone();
			Assert.NotSame(data, clone);
			Assert.Equal(data.VisibleId, clone.VisibleId);
			Assert.Equal(data.ShouldLogDirectly, clone.ShouldLogDirectly);
			Assert.Equal(data.HasData, clone.HasData);
		}

		[TestMethod]
		public void CorrelationData_Add_WithNullKey_ShouldThrowException()
		{
			try
			{
				FailOnErrors = false;

				Correlation = new Correlation(new MemoryCorrelationHandler(), CallContextManagerInstance, MachineInformation);
				Correlation.CorrelationStart(null);

				Assert.Throws<ArgumentNullException>(() =>
				{
					Correlation.CorrelationAdd(null, "value");
				});
			}
			finally
			{
				EndRequest();
			}
		}

		[TestMethod]
		public void CorrelationData_Add_WithNullValue_ShouldThrowException()
		{
			try
			{
				FailOnErrors = false;

				Correlation = new Correlation(new MemoryCorrelationHandler(), CallContextManagerInstance, MachineInformation);
				Correlation.CorrelationStart(null);

				Assert.Throws<ArgumentNullException>(() =>
				{
					Correlation.CorrelationAdd("key", null);
				});
			}
			finally
			{
				EndRequest();
			}
		}

		[TestMethod]
		public void CorrelationData_NullObjectToTransactionData_ShouldReturnNull()
		{
			CorrelationData data = null;
			Assert.Null(data.ToTransactionData());
		}

		[TestMethod]
		public void CorrelationData_ToTransactionData_ShouldReturnSameData()
		{
			try
			{
				Correlation = new Correlation(new MemoryCorrelationHandler(), CallContextManagerInstance, MachineInformation);
				Correlation.CorrelationStart(null);

				CorrelationData data = Correlation.CurrentCorrelation;
				TransactionData transaction = data.ToTransactionData();

				Assert.True(data.CallDepth == transaction.CallDepth, "Call depth properties should be equal.");
				Assert.True(data.EventSequenceNumber == transaction.EventSequenceNumber, "EventSequenceNumber properties should be equal.");
				Assert.True(data.TransactionId == transaction.TransactionId, "TransactionId properties should be equal.");
				Assert.True(data.UserHash == transaction.UserHash, "UserHash properties should be equal.");
				Assert.True(data.VisibleId == transaction.CorrelationId, "VisibleId and CorrelationId properties should be equal.");
			}
			finally
			{
				EndRequest();
			}
		}

		[TestMethod]
		public void CorrelationClear_ShouldClearAllCorrelations()
		{
			try
			{
				Correlation = new Correlation(new MemoryCorrelationHandler(), CallContextManagerInstance, MachineInformation);
				Correlation.CorrelationStart(null);

				CorrelationData previousCorrelation = Correlation.CurrentCorrelation;
				for (int i = 0; i < (new Random()).Next(3, 10); i++)
				{
					Correlation.CorrelationStart(null);
					CorrelationData parentCorrelation = Correlation.CurrentCorrelation.ParentCorrelation;
					Assert.Same(previousCorrelation, parentCorrelation);
					previousCorrelation = Correlation.CurrentCorrelation;
				}

				Assert.NotNull(Correlation.CurrentCorrelation);

				Correlation.CorrelationClear();

				Assert.Null(Correlation.CurrentCorrelation);
			}
			finally
			{
				EndRequest();
			}
		}

		[TestMethod]
		public void SettingShouldLogDirectly_WithoutAnActiveCorrelation_ShouldSetResetShouldLogDirectly()
		{
			try
			{
				Correlation = new Correlation(new MemoryCorrelationHandler(), CallContextManagerInstance, MachineInformation);
				Correlation.CorrelationStart(null);

				Assert.False(Correlation.ShouldLogDirectly, "ShouldLogDirectly should be set to false by default.");

				Correlation.ShouldLogDirectly = true;

				Assert.True(Correlation.ShouldLogDirectly, "ShouldLogDirectly should be set to true.");

				Correlation.ShouldLogDirectly = false;

				Assert.False(Correlation.ShouldLogDirectly, "ShouldLogDirectly should be reset to false.");
			}
			finally
			{
				EndRequest();
			}
		}

		[TestMethod]
		public void SettingShouldLogDirectly_OnCurrentCorrelation_ShouldSetOnCorrelationOnly()
		{
			try
			{
				Correlation = new Correlation(new MemoryCorrelationHandler(), CallContextManagerInstance, MachineInformation);

				Assert.False(Correlation.ShouldLogDirectly, "ShouldLogDirectly should be set to false by default.");

				Correlation.CorrelationStart(null);
				Correlation.ShouldLogDirectly = true;

				Assert.True(Correlation.ShouldLogDirectly, "ShouldLogDirectly should be set to true.");

				Correlation.CorrelationEnd();

				Assert.False(Correlation.ShouldLogDirectly, "ShouldLogDirectly should be set to false after correlation ended.");
			}
			finally
			{
				EndRequest();
			}
		}

		[TestMethod]
		public void SettingShouldLogDirectly_OnBaseCorrelation_ShouldBeInheritedByCurrentCorrelation()
		{
			try
			{
				Correlation = new Correlation(new MemoryCorrelationHandler(), CallContextManagerInstance, MachineInformation);

				Assert.False(Correlation.ShouldLogDirectly, "ShouldLogDirectly should be set to false by default.");

				Correlation.CorrelationStart(null);
				Correlation.ShouldLogDirectly = true;

				Correlation.CorrelationStart(null);

				Assert.True(Correlation.ShouldLogDirectly, "ShouldLogDirectly should be set to true.");
				Correlation.CorrelationClear();
			}
			finally
			{
				EndRequest();
			}
		}

		private ICallContextManager CreateCallContextManager()
		{
			return new CallContextManager();
		}

		private Correlation Correlation { get; set; }

		private ICallContextManager CallContextManagerInstance { get; set; }

		private IMachineInformation MachineInformation { get; set; }

		/// <summary>
		/// Create an HttpContext and configure HttpContext.Current
		/// </summary>
		private static void SetHttpContextCurrent()
		{
			HttpContext httpContext = new DefaultHttpContext();
			httpContext.Request.Host = new HostString("localhost");

			m_httpContextAccessor.Value.HttpContext = httpContext;
		}

		/// <summary>
		/// End a request
		/// </summary>
		private void EndRequest()
		{
			ICallContext context = CallContextManagerInstance.CallContextOverride;

			ICallContext existingContext = context.ExistingCallContext();
			Correlation.CorrelationEnd();
			context.EndCallContext();
		}
	}
}
