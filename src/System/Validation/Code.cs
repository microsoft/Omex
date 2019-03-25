// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Omex.System.Logging;
using UntaggedLogging = Microsoft.Omex.System.Logging.ULSLogging;

namespace Microsoft.Omex.System.Validation
{
	/// <summary>
	/// Code validation
	/// </summary>
	public static class Code
	{
		/// <summary>
		/// Check object's state, throws specified Exception type if state evaluates to false.
		/// </summary>
		/// <typeparam name="TException">Exception to throw</typeparam>
		/// <param name="state">Object's state</param>
		/// <param name="errorMessage">Error message</param>
		/// <param name="tagId">Tag Id to log, leave null if no logging is needed</param>
		public static void Expects<TException>(bool state, string errorMessage, uint? tagId)
			where TException : Exception
		{
			if (!Validate(state, errorMessage, tagId))
			{
				ReportError<TException>(errorMessage);
			}
		}


		/// <summary>
		/// Checks that the enumerable argument is not null and doesn't contain any nulls
		/// </summary>
		/// <remarks>Be careful to not pass enumerables that can be enumerated only once</remarks>
		/// <typeparam name="T">Type of the enumerable</typeparam>
		/// <param name="argumentValue">The argument value.</param>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="tagId">Tag Id to log, leave null if no logging is needed</param>
		/// <exception cref="ArgumentException">Thrown if any argument  <paramref name="argumentValue"/> element is null.</exception>
		/// <exception cref="ArgumentNullException">Thrown if the supplied argument <paramref name="argumentValue"/> is null.</exception>
		public static IEnumerable<T> ExpectsAllNotNull<T>(IEnumerable<T> argumentValue, string argumentName, uint? tagId)
			where T : class
		{
			argumentValue = ExpectsArgument(argumentValue, argumentName, tagId);

			if (!ValidateAllNotNull(argumentValue, argumentName, tagId))
			{
				ReportArgumentError(argumentName, AllErrorMessage);
			}

			return argumentValue;
		}


		/// <summary>
		/// Checks the argument value and throws an exception if it is null or contains no values.
		/// </summary>
		/// <param name="argumentValue">The argument value.</param>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="tagId">Tag Id to log, leave null if no logging is needed</param>
		/// <exception cref="ArgumentException">Thrown if the argument <paramref name="argumentValue"/> is empty.</exception>
		/// <exception cref="ArgumentNullException">Thrown if the supplied argument <paramref name="argumentValue"/> is null.</exception>
		public static IEnumerable<T> ExpectsAny<T>(IEnumerable<T> argumentValue, string argumentName, uint? tagId)
		{
			argumentValue = ExpectsArgument(argumentValue, argumentName, tagId);

			if (!ValidateAny(argumentValue, argumentName, tagId))
			{
				ReportArgumentError(argumentName, HasAnyErrorMessage);
			}

			return argumentValue;
		}


		/// <summary>
		/// Check object argument, throws exception if it is NULL
		/// </summary>
		/// <param name="argumentValue">argument value</param>
		/// <param name="argumentName">argument name</param>
		/// <param name="tagId">Tag Id to log, leave null if no logging is needed</param>
		/// <typeparam name="T">Type of argument to validate</typeparam>
		/// <exception cref="ArgumentNullException">Thrown if the supplied argument <paramref name="argumentValue"/> is null.</exception>
		public static T ExpectsArgument<T>(T argumentValue, string argumentName, uint? tagId)
		{
			if (!ValidateArgument(argumentValue, argumentName, tagId))
			{
				ReportArgumentNull(argumentName);
			}

			return argumentValue;
		}


		/// <summary>
		/// Check object argument against a predicate, throws if condition not met
		/// </summary>
		/// <param name="argumentValue">argument value</param>
		/// <param name="argumentName">argument name</param>
		/// <param name="predicate">predicate to evaluate</param>
		/// <param name="errorMessage">Error message for failed assertion</param>
		/// <param name="tagId">Tag Id to log, leave null if no logging is needed</param>
		/// <typeparam name="T">Type of argument to validate</typeparam>
		/// <exception cref="ArgumentException">Thrown if the supplied argument <paramref name="argumentValue"/> does not meet the predicate <paramref name="predicate"/>.</exception>
		public static T ExpectsArgument<T>(T argumentValue, string argumentName, Func<T, bool> predicate, string errorMessage, uint? tagId)
		{
			if (!ValidateArgument(argumentValue, argumentName, predicate, errorMessage, tagId))
			{
				ReportArgumentError(argumentName, errorMessage);
			}

			return argumentValue;
		}


		/// <summary>
		/// Checks the string argument and throws an exception if it is null, empty or whitespace.
		/// </summary>
		/// <param name="argumentValue">The argument value.</param>
		/// <param name="argumentName">The argument name.</param>
		/// <param name="tagId">Tag Id to log, leave null if no logging is needed</param>
		/// <exception cref="ArgumentException">Thrown if the argument <paramref name="argumentValue"/> is empty or contains only whitespace.</exception>
		/// <exception cref="ArgumentNullException">Thrown if the supplied argument <paramref name="argumentValue"/> is null.</exception>
		public static string ExpectsNotNullOrWhiteSpaceArgument(string argumentValue, string argumentName, uint? tagId)
		{
			argumentValue = ExpectsArgument(argumentValue, argumentName, tagId);

			if (!ValidateNotNullOrWhiteSpaceArgument(argumentValue, argumentName, tagId))
			{
				string errorMessage = argumentValue.Length == 0 ?
					ArgumentContainsEmptyString : ArgumentContainsWhiteSpaceString;

				ReportArgumentError(argumentName, errorMessage);
			}

			return argumentValue;
		}


		/// <summary>
		/// Checks the supplied argument and returns it if not null, otherwise throws.
		/// </summary>
		/// <typeparam name="T">The type of the reference parameter to be checked.</typeparam>
		/// <param name="argumentValue">The object to to checked.</param>
		/// <param name="argumentName">The local name of the argument.</param>
		/// <param name="tagId">Tag identifier to log; leave null if no logging is required.</param>
		/// <returns>Returns the argument value as is if validation succeeds, otherwise an exception is thrown.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the supplied argument <paramref name="argumentValue"/> is null.</exception>
		public static T ExpectsObject<T>(T argumentValue, string argumentName, uint? tagId) where T : class
		{
			return ExpectsArgument<T>(argumentValue, argumentName, tagId);
		}


		/// <summary>
		/// Validate object's state
		/// </summary>
		/// <param name="state">Object's state</param>
		/// <param name="errorMessage">Error message</param>
		/// <param name="tagId">Tag Id to log, leave null if no logging is needed</param>
		/// <returns>True if state evaluates to true; false otherwise</returns>
		public static bool Validate(bool state, string errorMessage, uint? tagId)
		{
			if (!state)
			{
				if (tagId != null)
				{
					UntaggedLogging.LogTraceTag(tagId.Value, Categories.ArgumentValidation, Levels.Error, errorMessage);
				}
			}

			return state;
		}


		/// <summary>
		/// Validate object's state
		/// </summary>
		/// <param name="state">Object's state</param>
		/// <param name="errorMessage">Error message</param>
		/// <param name="tagId">Tag Id to log, leave null if no logging is needed</param>
		/// <param name="parameters">logging parameters</param>
		/// <returns>True if state evaluates to true; false otherwise</returns>
		public static bool Validate(bool state, uint? tagId, string errorMessage, params object[] parameters)
		{
			if (!state)
			{
				if (tagId != null)
				{
					UntaggedLogging.LogTraceTag(tagId.Value, Categories.ArgumentValidation, Levels.Error, errorMessage, parameters);
				}
			}

			return state;
		}


		/// <summary>
		/// Checks that the enumerable argument is not null and doesn't contain any nulls
		/// </summary>
		/// <remarks>Be careful to not pass enumerables that can be enumerated only once</remarks>
		/// <typeparam name="T">Type of the enumerable</typeparam>
		/// <param name="argumentValue">The argument value.</param>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="tagId">Tag Id to log, leave null if no logging is needed</param>
		/// <returns>True if the argument <paramref name="argumentValue"/> is not null and contains only non-null elements; false otherwise.</returns>
		public static bool ValidateAllNotNull<T>(IEnumerable<T> argumentValue, string argumentName, uint? tagId)
			where T : class
		{
			if (!ValidateArgument(argumentValue, argumentName, tagId))
			{
				return false;
			}

			if (argumentValue.Any(x => x == null))
			{
				if (tagId != null)
				{
					UntaggedLogging.LogTraceTag(tagId.Value, Categories.ArgumentValidation, Levels.Error,
						ValidationFailed, AllErrorMessage, argumentName);
				}

				return false;
			}

			return true;
		}


		/// <summary>
		/// Validates that the argument value is not null and contains values.
		/// </summary>
		/// <param name="argumentValue">The argument value.</param>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="tagId">Tag Id to log, leave null if no logging is needed</param>
		/// <returns>True if the argument <paramref name="argumentValue"/> is not null and contains at least one element; false otherwise.</returns>
		/// <typeparam name="T">The element type.</typeparam>
		public static bool ValidateAny<T>(IEnumerable<T> argumentValue, string argumentName, uint? tagId)
		{
			if (!ValidateArgument(argumentValue, argumentName, tagId))
			{
				return false;
			}

			if (!argumentValue.Any())
			{
				if (tagId != null)
				{
					UntaggedLogging.LogTraceTag(tagId.Value, Categories.ArgumentValidation, Levels.Error,
						ValidationFailed, HasAnyErrorMessage, argumentName);
				}

				return false;
			}

			return true;
		}


		/// <summary>
		/// Validates the argument isn't null.
		/// </summary>
		/// <param name="argumentValue">The argument value.</param>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="tagId">Tag Id to log, leave null if no logging is needed</param>
		/// <returns>True if the argument <paramref name="argumentValue"/> is not null; false otherwise.</returns>
		public static bool ValidateArgument(object argumentValue, string argumentName, uint? tagId)
		{
			if (argumentValue == null)
			{
				if (tagId != null)
				{
					UntaggedLogging.LogTraceTag(tagId.Value, Categories.ArgumentValidation, Levels.Error,
						ValidationFailed, ArgumentIsNull, argumentName);
				}

				return false;
			}

			return true;
		}


		/// <summary>
		/// Validates argument against a predicate
		/// </summary>
		/// <param name="argumentValue">argument value</param>
		/// <param name="argumentName">argument name</param>
		/// <param name="predicate">predicate to evaluate</param>
		/// <param name="errorMessage">Error message for failed assertion</param>
		/// <param name="tagId">Tag Id to log, leave null if no logging is needed</param>
		/// <typeparam name="T">Type of argument to validate</typeparam>
		/// <returns>True if the argument <paramref name="argumentValue"/> passes predicate <paramref name="predicate"/>; false otherwise.</returns>
		public static bool ValidateArgument<T>(T argumentValue, string argumentName, Func<T, bool> predicate, string errorMessage, uint? tagId)
		{
			if (!predicate(argumentValue))
			{
				if (tagId != null)
				{
					UntaggedLogging.LogTraceTag(tagId.Value, Categories.ArgumentValidation, Levels.Error,
						ValidationFailed, errorMessage, argumentName);
				}

				return false;
			}

			return true;
		}


		/// <summary>
		/// Validates the guid argument and returns false if it is empty.
		/// </summary>
		/// <param name="argumentValue">The argument value.</param>
		/// <param name="argumentName">The argument name.</param>
		/// <param name="tagId">Tag Id to log, leave null if no logging is needed</param>
		/// <returns>True if the argument <paramref name="argumentValue"/> is not an empty guid; false otherwise.</returns>
		public static bool ValidateArgument(Guid argumentValue, string argumentName, uint? tagId = null)
		{
			if (argumentValue.Equals(Guid.Empty))
			{
				if (tagId != null)
				{
					UntaggedLogging.LogTraceTag(tagId.Value, Categories.ArgumentValidation, Levels.Error,
						ValidationFailed, ArgumentIsEmptyGuid, argumentName);
				}

				return false;
			}

			return true;
		}


		/// <summary>
		/// Validates the string argument and returns false if it is null, empty or only whitespace.
		/// </summary>
		/// <param name="argumentValue">The argument value.</param>
		/// <param name="argumentName">The argument name.</param>
		/// <param name="tagId">Tag Id to log, leave null if no logging is needed</param>
		/// <returns>True if the argument <paramref name="argumentValue"/> is not null, empty or only whitespace; false otherwise.</returns>
		public static bool ValidateNotNullOrWhiteSpaceArgument(string argumentValue, string argumentName, uint? tagId = null)
		{
			if (string.IsNullOrWhiteSpace(argumentValue))
			{
				if (ValidateArgument(argumentValue, argumentName, tagId))
				{
					if (tagId != null)
					{
						UntaggedLogging.LogTraceTag(tagId.Value, Categories.ArgumentValidation, Levels.Error,
							ValidationFailed,
							argumentName.Length == 0 ? ArgumentContainsEmptyString : ArgumentContainsWhiteSpaceString,
							argumentName);
					}
				}

				return false;
			}

			return true;
		}


		/// <summary>
		/// Report a null argument
		/// </summary>
		/// <param name="argumentName">Argument name</param>
		private static void ReportArgumentNull(string argumentName)
		{
			throw new ArgumentNullException(argumentName);
		}


		/// <summary>
		/// Report an incorrect argument
		/// </summary>
		/// <param name="argumentName">Argument name</param>
		/// <param name="errorDescription">Error description message</param>
		private static void ReportArgumentError(string argumentName, string errorDescription)
		{
			throw new ArgumentException(errorDescription, argumentName);
		}


		/// <summary>
		/// Report an error
		/// </summary>
		/// <typeparam name="TException">Exception type</typeparam>
		/// <param name="message">Message to report</param>
		private static void ReportError<TException>(string message) where TException : Exception
		{
			throw (Exception)Activator.CreateInstance(typeof(TException), message);
		}


		/// <summary>
		/// The error message for all *Any methods.
		/// </summary>
		private const string HasAnyErrorMessage = "argument does not contain any values";


		/// <summary>
		/// The error message for all *All methods.
		/// </summary>
		private const string AllErrorMessage = "argument contains a null value";


		/// <summary>
		/// The error message for argument is null.
		/// </summary>
		private const string ArgumentIsNull = "argument is null";


		/// <summary>
		/// The error message for argument containing empty string
		/// </summary>
		private const string ArgumentContainsEmptyString = "argument contains an empty string";


		/// <summary>
		/// The error message for argument containing whitespace string
		/// </summary>
		private const string ArgumentContainsWhiteSpaceString = "argument contains a string with only white space characters";


		/// <summary>
		/// The error message when argument is an empty GUID
		/// </summary>
		private const string ArgumentIsEmptyGuid = "argument is an empty GUID";


		/// <summary>
		/// Logging message for failed validation
		/// </summary>
		/// <remarks>The '{0}' placeholder is replaced with one of the messages above. The '{1}' is replaced with the argument name.</remarks>
		private const string ValidationFailed = "Code validation failed, {0}: {1}";
	}
}