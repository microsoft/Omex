// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.UnitTests.Shared;
using Microsoft.Omex.System.Validation;
using Xunit;

namespace Microsoft.Omex.System.UnitTests.Validation
{
	/// <summary>
	/// Unit tests for Code static validation
	/// </summary>
	public sealed class CodeUnitTests : UnitTestBase
	{
		[Fact]
		public void Expects_CorrectArgument_DoesNotLogOrThrowException()
		{
			FailOnErrors = true;

			uint? tagId = 1234;

			Code.Expects<ArgumentException>(true, ArgumentName, tagId);

			CheckLogCount(tagId, false);
		}


		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void Expects_IncorrectArgument_LogsAndThrowsException(bool log)
		{
			FailOnErrors = !log;

			uint? tagId = 1234;

			Assert.Throws<ArgumentException>(() => Code.Expects<ArgumentException>(false, ArgumentName, log ? tagId : null));

			CheckLogCount(tagId, log);
		}


		[Theory]
		[InlineData("")]
		[InlineData("", "")]
		public void ExpectsAnyAndAllNotNull_CorrectArgument_DoesNotLogOrThrowException(params string[] argumentValues)
		{
			FailOnErrors = true;

			uint? tagId = 1234;

			Code.ExpectsAllNotNull(argumentValues, ArgumentName, tagId);

			CheckLogCount(tagId, false);
		}


		[Theory]
		[InlineData(null, true)]
		[InlineData(null, false)]
		[InlineData(new string[0], true)]
		[InlineData(new string[0], false)]
		[InlineData(new string[] { null }, true)]
		[InlineData(new string[] { null }, false)]
		[InlineData(new string[] { "", null }, true)]
		[InlineData(new string[] { "", null }, false)]
		public void ExpectsAnyAndAllNotNull_IncorrectArgument_LogsAndThrowsException(string[] argumentValues, bool log)
		{
			FailOnErrors = !log;

			uint? tagId = 1234;

			if (argumentValues is null)
			{
				Assert.Throws<ArgumentNullException>(() => Code.ExpectsAnyAndAllNotNull(argumentValues, ArgumentName, log ? tagId : null));
			}
			else
			{
				Assert.Throws<ArgumentException>(() => Code.ExpectsAnyAndAllNotNull(argumentValues, ArgumentName, log ? tagId : null));
			}

			CheckLogCount(tagId, log);
		}


		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(2)]
		public void ExpectsAllNotNull_CorrectArgument_DoesNotLogOrThrowException(int count)
		{
			FailOnErrors = true;

			uint? tagId = 1234;

			string[] argumentValues = new string[count];
			for (int i = 0; i < count; i++)
			{
				argumentValues[i] = "";
			}

			Code.ExpectsAllNotNull(argumentValues, ArgumentName, tagId);

			CheckLogCount(tagId, false);
		}


		[Theory]
		[InlineData(0, true)]
		[InlineData(0, false)]
		[InlineData(1, true)]
		[InlineData(1, false)]
		[InlineData(2, true)]
		[InlineData(2, false)]
		public void ExpectsAllNotNull_IncorrectArgument_LogsAndThrowsException(int count, bool log)
		{
			FailOnErrors = !log;

			uint? tagId = 1234;

			string[] argumentValues = count > 0 ? new string[count] : null;
			for (int i = 0; i < count; i++)
			{
				if (i == count - 1)
				{
					argumentValues[i] = null;
				}
				else
				{
					argumentValues[i] = "";
				}
			}

			if (count == 0)
			{
				Assert.Throws<ArgumentNullException>(() => Code.ExpectsAllNotNull(argumentValues, ArgumentName, log ? tagId : null));
			}
			else
			{
				Assert.Throws<ArgumentException>(() => Code.ExpectsAllNotNull(argumentValues, ArgumentName, log ? tagId : null));
			}

			CheckLogCount(tagId, log);
		}


		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		public void ExpectsAny_CorrectArgument_DoesNotLogOrThrowException(int count)
		{
			FailOnErrors = true;

			uint? tagId = 1234;

			string[] argumentValues = new string[count];
			for (int i = 0; i < count; i++)
			{
				argumentValues[i] = "";
			}

			Code.ExpectsAny(argumentValues, ArgumentName, tagId);

			CheckLogCount(tagId, false);
		}



		[Theory]
		[InlineData(true, true)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(false, false)]
		public void ExpectsAny_IncorrectArgument_LogsAndThrowsException(bool emptyArray, bool log)
		{
			FailOnErrors = !log;

			uint? tagId = 1234;

			string[] argumentValues = emptyArray ? new string[0] : null;

			if (!emptyArray)
			{
				Assert.Throws<ArgumentNullException>(() => Code.ExpectsAny(argumentValues, ArgumentName, log ? tagId : null));
			}
			else
			{
				Assert.Throws<ArgumentException>(() => Code.ExpectsAny(argumentValues, ArgumentName, log ? tagId : null));
			}

			CheckLogCount(tagId, log);
		}


		[Theory]
		[InlineData("")]
		[InlineData("fd")]
		public void ExpectsArgument_CorrectArgument_DoesNotLogOrThrowException(string argument)
		{
			FailOnErrors = true;

			uint? tagId = 1234;

			Code.ExpectsArgument(argument, ArgumentName, tagId);

			CheckLogCount(tagId, false);
		}


		[Theory]
		[InlineData(null, true)]
		[InlineData(null, false)]
		public void ExpectsArgument_IncorrectArgument_LogsAndThrowsException(string argument, bool log)
		{
			FailOnErrors = !log;

			uint? tagId = 1234;

			Assert.Throws<ArgumentNullException>(() => Code.ExpectsArgument(argument, ArgumentName, log ? tagId : null));

			CheckLogCount(tagId, log);
		}


		[Theory]
		[InlineData("")]
		[InlineData("fd")]
		public void ExpectsArgumentWithPredicate_CorrectArgument_DoesNotLogOrThrowException(string argument)
		{
			FailOnErrors = true;

			uint? tagId = 1234;

			Code.ExpectsArgument(argument, ArgumentName, (s) => string.Equals(argument, s, StringComparison.Ordinal), "strings should be equal", tagId);

			CheckLogCount(tagId, false);
		}


		[Theory]
		[InlineData(null, true)]
		[InlineData(null, false)]
		public void ExpectsArgumentWithPredicate_IncorrectArgument_LogsAndThrowsException(string argument, bool log)
		{
			FailOnErrors = !log;

			uint? tagId = 1234;

			Assert.Throws<ArgumentException>(() => Code.ExpectsArgument(argument, ArgumentName, (s) => !string.Equals(argument, s, StringComparison.Ordinal), "strings should not be equal", log ? tagId : null));

			CheckLogCount(tagId, log);
		}


		[Theory]
		[InlineData("argument value")]
		public void ExpectsNotNullOrWhiteSpaceArgument_CorrectArgument_DoesNotLogOrThrowException(string argument)
		{
			FailOnErrors = true;

			uint tagId = 1234;
			string validatedArgument = Code.ExpectsNotNullOrWhiteSpaceArgument(argument, ArgumentName, tagId);
			Assert.Equal(argument, validatedArgument);

			CheckLogCount(tagId, false);
		}


		[Theory]
		[InlineData(null, true)]
		[InlineData(null, false)]
		[InlineData("", true)]
		[InlineData("", false)]
		[InlineData(" ", true)]
		[InlineData(" ", false)]
		public void ExpectsNotNullOrWhiteSpaceArgument_IncorrectArgument_LogsAndThrowsException(string argument, bool log)
		{
			FailOnErrors = !log;

			uint? tagId = 1234;

			if (argument == null)
			{
				Assert.Throws<ArgumentNullException>(() => Code.ExpectsNotNullOrWhiteSpaceArgument(argument, ArgumentName, log ? tagId : null));
			}
			else
			{
				Assert.Throws<ArgumentException>(() => Code.ExpectsNotNullOrWhiteSpaceArgument(argument, ArgumentName, log ? tagId : null));
			}

			CheckLogCount(tagId, log);
		}


		[Theory]
		[InlineData("")]
		[InlineData("fd")]
		public void ExpectsObject_CorrectArgument_DoesNotLogOrThrowException(string argument)
		{
			FailOnErrors = true;

			uint? tagId = 1234;

			Code.ExpectsObject(argument, ArgumentName, tagId);

			CheckLogCount(tagId, false);
		}


		[Theory]
		[InlineData(null, true)]
		[InlineData(null, false)]
		public void ExpectsObject_IncorrectArgument_LogsAndThrowsException(string argument, bool log)
		{
			FailOnErrors = !log;

			uint? tagId = 1234;

			Assert.Throws<ArgumentNullException>(() => Code.ExpectsObject(argument, ArgumentName, log ? tagId : null));

			CheckLogCount(tagId, log);
		}


		[Fact]
		public void Validate_CorrectArgument_DoesNotLogAndReturnsTrue()
		{
			FailOnErrors = true;

			uint? tagId = 1234;

			Assert.True(Code.Validate(true, tagId, ArgumentName));

			CheckLogCount(tagId, false);
		}


		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void Validate_IncorrectArgument_LogsAndReturnsFalse(bool log)
		{
			FailOnErrors = !log;

			uint? tagId = 1234;

			Assert.False(Code.Validate(false, log ? tagId : null, ArgumentName));

			CheckLogCount(tagId, log);
		}


		[Theory]
		[InlineData(new string[0], true)]
		[InlineData(new string[0], false)]
		[InlineData(new string[] { null }, true)]
		[InlineData(new string[] { null }, false)]
		[InlineData(new string[] { "", null }, true)]
		[InlineData(new string[] { "", null }, false)]
		[InlineData(null, true)]
		[InlineData(null, false)]
		public void ValidateAnyAndAllNotNull_IncorrectArgument_LogsAndReturnsFalse(string[] argumentValue, bool log)
		{
			FailOnErrors = !log;

			uint? tagId = 1234;

			Assert.False(Code.ValidateAnyAndAllNotNull(argumentValue, ArgumentName, log ? tagId : null));

			CheckLogCount(tagId, log);
		}


		[Fact]
		public void ValidateAnyAndAllNotNull_CorrectArgument_DoesNotLogAndReturnsTrue()
		{
			FailOnErrors = true;

			uint? tagId = 1234;

			Assert.True(Code.ValidateAnyAndAllNotNull(new[] { "" }, ArgumentName, tagId));

			CheckLogCount(tagId, false);
		}


		[Theory]
		[InlineData("")]
		[InlineData()]
		public void ValidateAllNotNull_CorrectArgument_DoesNotLogAndReturnsTrue(params string[] argumentValue)
		{
			FailOnErrors = true;

			uint? tagId = 1234;

			Assert.True(Code.ValidateAllNotNull(argumentValue, ArgumentName, tagId));

			CheckLogCount(tagId, false);
		}


		[Theory]
		[InlineData(0, true)]
		[InlineData(0, false)]
		[InlineData(1, true)]
		[InlineData(1, false)]
		[InlineData(2, true)]
		[InlineData(2, false)]
		public void ValidateAllNotNull_IncorrectArgument_LogsAndReturnsFalse(int count, bool log)
		{
			FailOnErrors = !log;

			uint? tagId = 1234;

			string[] argumentValues = count > 0 ? new string[count] : null;
			for (int i = 0; i < count; i++)
			{
				if (i == count - 1)
				{
					argumentValues[i] = null;
				}
				else
				{
					argumentValues[i] = "";
				}
			}

			Assert.False(Code.ValidateAllNotNull(argumentValues, ArgumentName, log ? tagId : null));

			CheckLogCount(tagId, log);
		}


		[Theory]
		[InlineData("")]
		[InlineData("", null)]
		public void ValidateAny_CorrectArgument_DoesNotLogAndReturnsTrue(params string[] argumentValue)
		{
			FailOnErrors = true;

			uint? tagId = 1234;

			Assert.True(Code.ValidateAny(argumentValue, ArgumentName, tagId));

			CheckLogCount(tagId, false);
		}


		[Theory]
		[InlineData(true, true)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(false, false)]
		public void ValidateAny_IncorrectArgument_LogsAndReturnsFalse(bool emptyArray, bool log)
		{
			FailOnErrors = !log;

			uint? tagId = 1234;

			string[] argumentValues = emptyArray ? new string[0] : null;

			if (!emptyArray)
			{
				Assert.False(Code.ValidateAny(argumentValues, ArgumentName, log ? tagId : null));
			}
			else
			{
				Assert.False(Code.ValidateAny(argumentValues, ArgumentName, log ? tagId : null));
			}

			CheckLogCount(tagId, log);
		}


		[Fact]
		public void ValidateArgument_CorrectArgument_DoesNotLogAndReturnsTrue()
		{
			FailOnErrors = true;

			uint? tagId = 1234;

			Assert.True(Code.ValidateArgument(Guid.NewGuid(), ArgumentName, tagId));

			CheckLogCount(tagId, false);
		}


		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void ValidateArgument_IncorrectArgument_LogsAndReturnsFalse(bool log)
		{
			FailOnErrors = !log;

			uint? tagId = 1234;

			Assert.False(Code.ValidateArgument(Guid.Empty, ArgumentName, log ? tagId : null));

			CheckLogCount(tagId, log);
		}



		private void CheckLogCount(uint? tagId, bool log)
		{
			IList<LogEventArgs> events = LoggedEvents;
			if (!log)
			{
				Assert.Equal(0, events != null ? events.Count() : 0);
			}
			else
			{
				Assert.Equal(1, events != null ? events.Count() : 0);

				LogEventArgs eventArg = events.Single();
				Assert.Equal(tagId, eventArg.TagId);
				Assert.Equal(Levels.Error, eventArg.Level);
				Assert.Equal(Categories.ArgumentValidation, eventArg.CategoryId);
				Assert.Contains(ArgumentName, eventArg.FullMessage);
			}
		}


		/// <summary>
		/// Argument's name
		/// </summary>
		private const string ArgumentName = "testArgument";
	}
}