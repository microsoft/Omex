// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;

namespace Microsoft.Omex.Extensions.Abstractions.Wrappers
{
	/// <summary>
	/// Wrapper for late object construction
	/// </summary>
	public class Wrapper<T> : IWrapper<T>
	{
		private T? m_instance;

		private Func<Task<T>> m_factory;

		/// <summary>
		/// Constructor with factory responsible for late construction
		/// </summary>
		public Wrapper(Func<Task<T>> factory)
		{
			m_factory = factory;
		}

		/// <summary>
		/// Returns along with constructing instance if not existed before 
		/// </summary>
		public T Get()
		{
			if (m_instance == null)
			{
				m_instance = m_factory.Invoke().Result;
			}

			return m_instance;
		}
	}
}
