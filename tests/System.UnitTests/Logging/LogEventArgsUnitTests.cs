/***************************************************************************************************
	LogEventArgsUnitTests.cs

	Unit tests for LogEventArgs class
***************************************************************************************************/

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
			LogEventArgs logEventArgs = new LogEventArgs(0, Categories.ArgumentValidation, Levels.Error, "My message {0}", "is this!");

			Assert.Equal<uint>(0, logEventArgs.TagId);
			Assert.Equal(Categories.ArgumentValidation, logEventArgs.Category);
			Assert.Equal(Levels.Error, logEventArgs.Level);
			Assert.Equal("My message {0}", logEventArgs.Message);
			Assert.Equal(1, logEventArgs.MessageParameters.Length);
			Assert.Equal("is this!", logEventArgs.MessageParameters[0]);
			Assert.Equal("My message is this!", logEventArgs.FullMessage);
		}


		[Fact]
		public void FullMessage_WithIncorrectFormattingString_DoesNotThrow()
		{
			LogEventArgs logEventArgs = new LogEventArgs(0, Categories.ArgumentValidation, Levels.Error, "My message {1}", "is this!");

			Assert.Equal<uint>(0, logEventArgs.TagId);
			Assert.Equal(Categories.ArgumentValidation, logEventArgs.Category);
			Assert.Equal(Levels.Error, logEventArgs.Level);
			Assert.Equal("My message {1}", logEventArgs.Message);
			Assert.Equal(1, logEventArgs.MessageParameters.Length);
			Assert.Equal("is this!", logEventArgs.MessageParameters[0]);
			Assert.Contains("My message {1}", logEventArgs.FullMessage);
		}


		[Fact]
		public void FullMessage_WithNullMessageString_DoesNotThrow()
		{
			LogEventArgs logEventArgs = new LogEventArgs(0, Categories.ArgumentValidation, Levels.Error, null, "is this!");

			Assert.Equal<uint>(0, logEventArgs.TagId);
			Assert.Equal(Categories.ArgumentValidation, logEventArgs.Category);
			Assert.Equal(Levels.Error, logEventArgs.Level);
			Assert.Equal(null, logEventArgs.Message);
			Assert.Equal(1, logEventArgs.MessageParameters.Length);
			Assert.Equal("is this!", logEventArgs.MessageParameters[0]);
			Assert.Equal(string.Empty, logEventArgs.FullMessage);
		}
	}
}
