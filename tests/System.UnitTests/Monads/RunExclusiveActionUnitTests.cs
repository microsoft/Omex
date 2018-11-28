/***************************************************************************************************
	RunExclusiveActionUnitTests.cs

	Owner: mwoulfe
	Copyright (c) Microsoft Corporation

	Unit tests for the RunExclusiveAction class.
***************************************************************************************************/

#region Using directives

using System;
using System.Threading;
using Microsoft.Omex.System.Monads;
using Microsoft.Omex.System.UnitTests.Shared;
using Xunit;

#endregion

namespace Microsoft.Omex.System.UnitTests.Monads
{
	/// <summary>
	/// Unit tests for the RunExclusiveAction class.
	/// </summary>
	public class RunExclusiveActionUnitTests : UnitTestBase
	{
		[Fact]
		public void DoViaConstructor_WithNullAction_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new RunExclusiveAction(null));
		}


		[Fact]
		public void DoViaConstructor_WithRunOnlyOnce_RunsOnce()
		{
			int value = 0;
			RunExclusiveAction action = new RunExclusiveAction(
				() => Interlocked.Increment(ref value), true);
			Assert.False(action.HasRun);

			for (int i = 0; i < 10; i++)
			{
				action.Do();
			}

			Assert.Equal(1, value);
			Assert.True(action.HasRun);
		}


		[Fact]
		public void DoViaConstructor_WithoutRunOnlyOnce_RunsEachTime()
		{
			int value = 0;
			RunExclusiveAction action = new RunExclusiveAction(
				() => Interlocked.Increment(ref value));

			for (int i = 0; i < 10; i++)
			{
				action.Do();
			}

			Assert.Equal(10, value);
			Assert.True(action.HasRun);
		}


		[Fact]
		[Trait("TestOwner", "mwoulfe")]
		public void DoViaConstructor_WithException_Throws()
		{
			RunExclusiveAction action = new RunExclusiveAction(
				() =>
				{
					throw new NotImplementedException();
				});

			Assert.Throws<NotImplementedException>(() => action.Do());
			Assert.True(action.HasRun);
		}


		[Fact]
		public void Do_WithNullAction_DoesNotThrow()
		{
			FailOnErrors = false;

			RunExclusiveAction action = new RunExclusiveAction();

			action.Do(null);

			Assert.False(action.HasRun);
		}


		[Fact]
		public void Do_WithNoAction_ThrowsArgumentNullException()
		{
			FailOnErrors = false;

			RunExclusiveAction action = new RunExclusiveAction();

			action.Do();

			Assert.False(action.HasRun);
		}
	}
}
