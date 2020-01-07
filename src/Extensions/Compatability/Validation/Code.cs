// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Compatability.Validation
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
		[Obsolete(ObsoleteMessage, IsObsoleteError)]
		public static void Expects<TException>(bool state, string errorMessage, EventId? tagId)
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
		[Obsolete(ObsoleteMessage, IsObsoleteError)]
		public static IEnumerable<T> ExpectsAllNotNull<T>([ValidatedNotNull] IEnumerable<T>? argumentValue, string argumentName, EventId? tagId)
			where T : class
		{
			argumentValue = ExpectsArgumentNotNull(argumentValue, argumentName, tagId);

			if (!ValidateAllNotNull(argumentValue, argumentName, tagId))
			{
				ReportArgumentError(argumentName, AllErrorMessage);
			}

			return argumentValue;
		}


		/// <summary>
		/// Checks the collection and throws an exception if it is null, contains no values, or contains any null values
		/// </summary>
		/// <typeparam name="T">Type of the collection</typeparam>
		/// <param name="argumentValue">The argument value.</param>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="tagId">Tag Id to log, leave null if no logging is needed.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="argumentValue"/> is empty or if any element is null.</exception>
		/// <exception cref="ArgumentNullException">Thrown if the supplied argument <paramref name="argumentValue"/> is null.</exception>
		[Obsolete(ObsoleteMessage, IsObsoleteError)]
		public static IEnumerable<T> ExpectsNotEmptyAndAllNotNull<T>([ValidatedNotNull] ICollection<T>? argumentValue, string argumentName, EventId? tagId) where T : class
		{
			argumentValue = ExpectsArgumentNotNull(argumentValue, argumentName, tagId);

			if (!ValidateNotEmptyAndAllNotNull(argumentValue, argumentName, tagId))
			{
				ReportArgumentError(argumentName, HasAnyErrorMessage);
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
		[Obsolete(ObsoleteMessage, IsObsoleteError)]
		public static IEnumerable<T> ExpectsAny<T>([ValidatedNotNull] IEnumerable<T>? argumentValue, string argumentName, EventId? tagId)
		{
			argumentValue = ExpectsArgumentNotNull(argumentValue, argumentName, tagId);

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
		[Obsolete(ObsoleteMessage, IsObsoleteError)]
		public static T ExpectsArgument<T>([ValidatedNotNull] T argumentValue, string argumentName, EventId? tagId)
		{
			return (T)ExpectsArgumentNotNull((object?)argumentValue, argumentName, tagId);
		}


		/// <summary>
		/// Check object argument, throws exception if it is NULL
		/// Unlike ExpectsArgument it will return not nullable type
		/// </summary>
		/// <param name="argumentValue">argument value</param>
		/// <param name="argumentName">argument name</param>
		/// <param name="tagId">Tag Id to log, leave null if no logging is needed</param>
		/// <typeparam name="T">Type of argument to validate</typeparam>
		/// <exception cref="ArgumentNullException">Thrown if the supplied argument <paramref name="argumentValue"/> is null.</exception>
		[Obsolete(ObsoleteMessage, IsObsoleteError)]
		public static T ExpectsArgumentNotNull<T>([ValidatedNotNull] T? argumentValue, string argumentName, EventId? tagId)
			where T : class
		{
			if (!ValidateArgument(argumentValue, argumentName, tagId))
			{
				return ReportArgumentNull<T>(argumentName);
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
		[Obsolete(ObsoleteMessage, IsObsoleteError)]
		public static T ExpectsArgument<T>(T argumentValue, string argumentName, Func<T, bool> predicate, string errorMessage, EventId? tagId)
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
		[Obsolete(ObsoleteMessage, IsObsoleteError)]
		public static string ExpectsNotNullOrWhiteSpaceArgument([ValidatedNotNull] string? argumentValue, string argumentName, EventId? tagId)
		{
			argumentValue = ExpectsArgumentNotNull(argumentValue, argumentName, tagId);

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
		[Obsolete(ObsoleteMessage, IsObsoleteError)]
		public static T ExpectsObject<T>([ValidatedNotNull] T? argumentValue, string argumentName, EventId? tagId) where T : class
		{
			return ExpectsArgumentNotNull(argumentValue, argumentName, tagId);
		}


		/// <summary>
		/// Validate object's state
		/// </summary>
		/// <param name="state">Object's state</param>
		/// <param name="errorMessage">Error message</param>
		/// <param name="tagId">Tag Id to log, leave null if no logging is needed</param>
		/// <returns>True if state evaluates to true; false otherwise</returns>
		[Obsolete(ObsoleteMessage, IsObsoleteError)]
		public static bool Validate(bool state, string errorMessage, EventId? tagId)
		{
			if (!state)
			{
				LogError(tagId, errorMessage);
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
		[Obsolete(ObsoleteMessage, IsObsoleteError)]
		public static bool Validate(bool state, EventId? tagId, string errorMessage, params object[] parameters)
		{
			if (!state)
			{
				LogError(tagId, errorMessage, parameters);
			}

			return state;
		}


		/// <summary>
		/// Checks that the collection is not null, is not empty, and does not contain nulls
		/// </summary>
		/// <typeparam name="T">The type of the collection</typeparam>
		/// <param name="argumentValue">The argument value.</param>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="tagId">Tag Id to log, leave null if no logging is needed</param>
		/// <returns>True if the argument <paramref name="argumentValue"/> is not null, is not empty, and does not contain nulls; false otherwise.</returns>
		[Obsolete(ObsoleteMessage, IsObsoleteError)]
		public static bool ValidateNotEmptyAndAllNotNull<T>([NotNullWhen(true)]ICollection<T>? argumentValue, string argumentName, EventId? tagId = null)
			where T : class
		{
			if (!ValidateArgument(argumentValue, argumentName, tagId))
			{
				return false;
			}

			if (!argumentValue.Any() || argumentValue.Any(x => x == null))
			{
				LogError(tagId, ValidationFailed, AllErrorMessage, argumentName);
				return false;
			}

			return true;
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
		[Obsolete(ObsoleteMessage, IsObsoleteError)]
		public static bool ValidateAllNotNull<T>([NotNullWhen(true)]IEnumerable<T>? argumentValue, string argumentName, EventId? tagId = null)
			where T : class
		{
			if (!ValidateArgument(argumentValue, argumentName, tagId))
			{
				return false;
			}

			if (argumentValue.Any(x => x == null))
			{
				LogError(tagId, ValidationFailed, AllErrorMessage, argumentName);
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
		[Obsolete(ObsoleteMessage, IsObsoleteError)]
		public static bool ValidateAny<T>([NotNullWhen(true)]IEnumerable<T>? argumentValue, string argumentName, EventId? tagId = null)
		{
			if (!ValidateArgument(argumentValue, argumentName, tagId))
			{
				return false;
			}

			if (!argumentValue.Any())
			{
				LogError(tagId, ValidationFailed, HasAnyErrorMessage, argumentName);
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
		[Obsolete(ObsoleteMessage, IsObsoleteError)]
		public static bool ValidateArgument([NotNullWhen(true)]object? argumentValue, string argumentName, EventId? tagId = null)
		{
			if (argumentValue == null)
			{
				LogError(tagId, ValidationFailed, ArgumentIsNull, argumentName);
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
		[Obsolete(ObsoleteMessage, IsObsoleteError)]
		public static bool ValidateArgument<T>(T argumentValue, string argumentName, Func<T, bool> predicate, string errorMessage, EventId? tagId = null)
		{
			if (!predicate(argumentValue))
			{
				LogError(tagId, ValidationFailed, errorMessage, argumentName);
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
		[Obsolete(ObsoleteMessage, IsObsoleteError)]
		public static bool ValidateArgument(Guid argumentValue, string argumentName, EventId? tagId = null)
		{
			if (argumentValue.Equals(Guid.Empty))
			{
				LogError(tagId, ValidationFailed, ArgumentIsEmptyGuid, argumentName);
				return false;
			}

			return true;
		}


		/// <summary>
		/// Validates the string argument and returns false if it is null, empty or only whitespace.
		/// </summary>
		/// <param name="argumentValue">The argument value.</param>
		/// <param name="argumentName">The argument name.</param>
		/// <param name="eventId">Tag Id to log, leave null if no logging is needed</param>
		/// <returns>True if the argument <paramref name="argumentValue"/> is not null, empty or only whitespace; false otherwise.</returns>
		[Obsolete(ObsoleteMessage, IsObsoleteError)]
		public static bool ValidateNotNullOrWhiteSpaceArgument([NotNullWhen(true)]string? argumentValue, string argumentName, EventId? eventId = null)
		{
			if (string.IsNullOrWhiteSpace(argumentValue))
			{
				if (ValidateArgument(argumentValue, argumentName, eventId))
				{
					LogError(
						eventId,
						ValidationFailed,
						argumentName.Length == 0 ? ArgumentContainsEmptyString : ArgumentContainsWhiteSpaceString,
						argumentName);
				}

				return false;
			}

			return true;
		}


		internal static void Initialize(ILoggerFactory loggerFactory) => s_logger = loggerFactory.CreateLogger("ArgumentValidation");


		private static ILogger? s_logger;


		private static void LogError(EventId? eventId, string message, params object[] args)
		{
			if (s_logger == null || !eventId.HasValue)
			{
				return;
			}

			s_logger.LogError(eventId.Value, message, args);
		}


		/// <summary>
		/// Report a null argument
		/// </summary>
		/// <param name="argumentName">Argument name</param>
		/// <returns>The function has a return type in order to be used in the return statement, to explain compiler that it will end method execution</returns>
		private static T ReportArgumentNull<T>(string argumentName) => throw new ArgumentNullException(argumentName);


		/// <summary>
		/// Report an incorrect argument
		/// </summary>
		/// <param name="argumentName">Argument name</param>
		/// <param name="errorDescription">Error description message</param>
		private static void ReportArgumentError(string argumentName, string errorDescription) => throw new ArgumentException(errorDescription, argumentName);


		/// <summary>
		/// Report an error
		/// </summary>
		/// <typeparam name="TException">Exception type</typeparam>
		/// <param name="message">Message to report</param>
		private static void ReportError<TException>(string message) where TException : Exception
		{
			Exception? exception = (Exception?)Activator.CreateInstance(typeof(TException), message);
			if (exception != null)
			{
				throw exception;
			}
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


		private const string ObsoleteMessage = "Please consider using non-nullable reference types instead";
		private const bool IsObsoleteError = false;
	}
}
