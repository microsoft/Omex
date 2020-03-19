// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.UnitTests.Shared;
using Xunit;

namespace Microsoft.Omex.System.UnitTests.Logging
{
	public sealed class LogEventArgsUnitTests : UnitTestBase
	{
		[Fact]
		public void Constructor_SetsMemberVariables()
		{
			LogEventArgs logEventArgs = new LogEventArgs(false, 0, Categories.ArgumentValidation, Levels.Error, "My message {0}", string.Empty, "is this!");

			Assert.Equal<uint>(0, logEventArgs.TagId);
			Assert.Equal(Thread.CurrentThread.ManagedThreadId, logEventArgs.ThreadId);
			Assert.Equal(Categories.ArgumentValidation, logEventArgs.CategoryId);
			Assert.Equal(Levels.Error, logEventArgs.Level);
			Assert.Equal("My message {0}", logEventArgs.Message);
			Assert.Equal(1, logEventArgs.MessageParameters.Length);
			Assert.Equal("is this!", logEventArgs.MessageParameters[0]);
			Assert.Equal("My message is this!", logEventArgs.FullMessage);
		}

		[Fact]
		public void FullMessage_WithIncorrectFormattingString_DoesNotThrow()
		{
			LogEventArgs logEventArgs = new LogEventArgs(false, 0, Categories.ArgumentValidation, Levels.Error, "My message {1}", string.Empty, "is this!");

			Assert.Equal<uint>(0, logEventArgs.TagId);
			Assert.Equal(Thread.CurrentThread.ManagedThreadId, logEventArgs.ThreadId);
			Assert.Equal(Categories.ArgumentValidation, logEventArgs.CategoryId);
			Assert.Equal(Levels.Error, logEventArgs.Level);
			Assert.Equal("My message {1}", logEventArgs.Message);
			Assert.Equal(1, logEventArgs.MessageParameters.Length);
			Assert.Equal("is this!", logEventArgs.MessageParameters[0]);
			Assert.Contains("My message {1}", logEventArgs.FullMessage);
		}

		[Fact]
		public void FullMessage_WithNullMessageString_DoesNotThrow()
		{
			LogEventArgs logEventArgs = new LogEventArgs(false, 0, Categories.ArgumentValidation, Levels.Error, null, string.Empty, "is this!");

			Assert.Equal<uint>(0, logEventArgs.TagId);
			Assert.Equal(Thread.CurrentThread.ManagedThreadId, logEventArgs.ThreadId);
			Assert.Equal(Categories.ArgumentValidation, logEventArgs.CategoryId);
			Assert.Equal(Levels.Error, logEventArgs.Level);
			Assert.Equal(null, logEventArgs.Message);
			Assert.Equal(1, logEventArgs.MessageParameters.Length);
			Assert.Equal("is this!", logEventArgs.MessageParameters[0]);
			Assert.Equal(null, logEventArgs.FullMessage);
		}
	}
}
