// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Azure.Documents;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.DocumentDb.Extensions
{
	/// <summary>
	/// DocumentClientException extensions class.
	/// </summary>
	public static class DocumentClientExceptionExtensions
	{
		/// <summary>
		/// Extracts error information from document client exception.
		/// </summary>
		/// <param name="exception">Document client exception.</param>
		/// <returns>Converted error string from document client exception information.</returns>
		public static string ToErrorMessage(this DocumentClientException exception)
		{
			Code.ExpectsArgument(exception, nameof(exception), TaggingUtilities.ReserveTag(0x2381b145 /* tag_961ff */));

			return $"Cost: {exception.RequestCharge}" +
				$" ContentLocation: {exception.ResponseHeaders["Content-Location"]}" +
				$"StatusCode: {(int)exception.StatusCode.GetValueOrDefault()} ActivityId: {exception.ActivityId}" +
				$" Error: {exception.Message}";
		}
	}
}