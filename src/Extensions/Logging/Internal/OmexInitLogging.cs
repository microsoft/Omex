using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Microsoft.Omex.Extensions.Logging
{
	internal sealed class OmexStaticInitLogging
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="consoleLoggerProvider"></param>
		/// <param name="omexLoggerProvider"></param>
		public OmexStaticInitLogging(ConsoleLoggerProvider consoleLoggerProvider, OmexLoggerProvider omexLoggerProvider)
		{
			m_console = consoleLoggerProvider.CreateLogger("");
			m_omexLogger = omexLoggerProvider.CreateLogger("initialisation");
		}

		private ILogger? m_console;

		private ILogger m_omexLogger;
	}
}
