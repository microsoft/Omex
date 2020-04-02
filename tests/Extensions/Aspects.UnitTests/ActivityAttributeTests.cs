using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.TimedScopes.UnitTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Aspects.UnitTests
{
	[TestClass]
	public class ActivityAttributeTests
	{
		private Activity? m_currentActivity;

		[TestMethod]
		public void MethodReturnsVoid_WrapedInActivity()
		{
			MethodReturnsVoid();
			m_currentActivity!.AssertResult(TimedScopeResult.Success);
		}

		[TestMethod]
		public void MethodReturnsObject_WrapedInActivity()
		{
			MethodReturnsObject();
			m_currentActivity!.AssertResult(TimedScopeResult.Success);
		}

		[TestMethod]
		public void MethodReturnsInt_WrapedInActivity()
		{
			MethodReturnsInt();
			m_currentActivity!.AssertResult(TimedScopeResult.Success);
		}

		[TestMethod]
		public async Task MethodReturtnsGenericTaskNotAwaited_WrapedInActivity()
		{
			TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
			Task task = MethodReturtnsGenericTaskNotAwaitedAsync(source);
			await AssertTaskActivity(source, task);
		}

		[TestMethod]
		public async Task MethodReturtnsGenericTask_WrapedInActivity()
		{
			TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
			Task task = MethodReturtnsGenericTaskAsync(source);
			await AssertTaskActivity(source, task);
		}

		[TestMethod]
		public async Task MethodReturtnsTask_WrapedInActivity()
		{
			TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
			Task task = MethodReturtnsTaskAsync(source);
			await AssertTaskActivity(source, task);
		}

		[Activity]
		public void MethodReturnsVoid()
		{
			AssertActivity();
		}

		[Activity]
		public object MethodReturnsObject()
		{
			AssertActivity();
			return new object();
		}

		[Activity]
		public int MethodReturnsInt()
		{
			AssertActivity();
			return 1;
		}

		[Activity]
		public Task<bool> MethodReturtnsGenericTaskNotAwaitedAsync(TaskCompletionSource<bool> source)
		{
			AssertActivity();
			return source.Task;
		}

		[Activity]
		public async Task<bool> MethodReturtnsGenericTaskAsync(TaskCompletionSource<bool> source)
		{
			AssertActivity();
			return await source.Task;
		}

		[Activity]
		public async Task MethodReturtnsTaskAsync(TaskCompletionSource<bool> source)
		{
			AssertActivity();
			await source.Task;
		}

		[TestMethod]
		public void MethodThowsException_WrapedInActivity()
		{
			Assert.ThrowsException<Exception>(() => MethodThowsException());
			m_currentActivity!.AssertResult(TimedScopeResult.SystemError);
		}

		[TestMethod]
		public void MethodThowsExpectedException_WrapedInActivity()
		{
			Assert.ThrowsException<ArgumentNullException>(() => MethodThowsExpectedException());
			m_currentActivity!.AssertResult(TimedScopeResult.ExpectedError);
		}

		[TestMethod]
		public async Task MethodThowsExceptionAsync_WrapedInActivity()
		{
			await Assert.ThrowsExceptionAsync<Exception>(() => MethodThowsExceptionAsync());
			m_currentActivity!.AssertResult(TimedScopeResult.SystemError);
		}

		[TestMethod]
		public async Task MethodThowsExpectedExceptionAsync_WrapedInActivity()
		{
			await Assert.ThrowsExceptionAsync<ArithmeticException>(() => MethodThowsExpectedExceptionAsync());
			await Task.Delay(1);
			m_currentActivity!.AssertResult(TimedScopeResult.ExpectedError);
		}

		//[Activity]
		//public async ValueTask MethodAsyncReturnsValueTask_WrapedInActivity(TaskCompletionSource<bool> source)
		//{
		//	Assert.IsNotNull(Activity.Current);
		//	await source.Task;
		//}

		//[Activity]
		//public async ValueTask<bool> MethodAsyncReturnsValueTaskT_WrapedInActivity(TaskCompletionSource<bool> source)
		//{
		//	Assert.IsNotNull(Activity.Current);
		//	return await source.Task;
		//}

		[Activity]
		public void MethodThowsException()
		{
			AssertActivity();
			throw new Exception();
		}

		[Activity(typeof(ArgumentNullException))]
		public void MethodThowsExpectedException()
		{
			AssertActivity();
			throw new ArgumentNullException();
		}

		[Activity]
		public async Task MethodThowsExceptionAsync()
		{
			AssertActivity();
			await Task.Yield();
			throw new Exception();
		}

		[Activity(typeof(ArithmeticException))]
		public async Task MethodThowsExpectedExceptionAsync()
		{
			AssertActivity();
			await Task.Yield();
			throw new ArithmeticException();
		}

		private void AssertActivity([CallerMemberName]string? name = null)
		{
			m_currentActivity = Activity.Current;
			Assert.IsNotNull(m_currentActivity);
			Assert.AreEqual(name, m_currentActivity.OperationName);
			m_currentActivity.AssertResult(TimedScopeResult.SystemError);
		}

		private async ValueTask AssertTaskActivity(TaskCompletionSource<bool> source, Task task)
		{
			Assert.IsNotNull(m_currentActivity);
			Assert.IsFalse(IsActivityStoped(m_currentActivity));
			source.SetResult(true);
			await task;
			await Task.Delay(1); // need to give for continuation on task to execute, it would be nice to find another way
			Assert.IsTrue(IsActivityStoped(m_currentActivity));
			m_currentActivity!.AssertResult(TimedScopeResult.Success);
		}

		private bool IsActivityStoped(Activity? activity) => activity?.Duration != TimeSpan.Zero;
	}
}
