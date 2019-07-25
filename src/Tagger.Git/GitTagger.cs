// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using System.Collections.Generic;

namespace Microsoft.Omex.Log.Tagger.Git
{
	internal class GitTagger : ITagger
	{
		protected string m_repoRoot;

		protected string m_pathSeparator;

		protected string m_commitId;

		protected string m_urlTemplate;

		private readonly ITagCache m_cache;

		public GitTagger(string pathToClonedRepo, ITagCache cache, IEnumerable<IUrlTemplateProvider> templateProviders)
		{
			m_cache = cache;
			m_repoRoot = GetRepoRoot(pathToClonedRepo);
			if (m_repoRoot == null)
			{
				throw new ArgumentException("Filed to find repository root");
			}

			Repository repository = new Repository(m_repoRoot);
			m_commitId = repository.Head.Tip.Sha;
			Remote origin = repository.Network.Remotes.FirstOrDefault();
			if (origin == null)
			{
				throw new ArgumentException("Filed to find repository origin");
			}

			(m_urlTemplate, m_pathSeparator) = CreateUrlTemplate(origin.Url, templateProviders);
		}

		public string GetUrl(string filePath, int lineNumber) => m_cache.GetOrAdd(filePath, lineNumber, CreateUrl);

		private string GetRelativePath(string filePath) => filePath.Substring(m_repoRoot.Length).Replace("\\", m_pathSeparator);

		private string CreateUrl((string localPath, int lineNumber) position) => CreateUrl(GetRelativePath(position.localPath), position.lineNumber);

		private string CreateUrl(string relativePath, int lineNumber) => string.Format(m_urlTemplate, m_commitId, relativePath, lineNumber);

		private (string template, string separator) CreateUrlTemplate(string originUrl, IEnumerable<IUrlTemplateProvider> templateProviders)
		{
			const string gitUrlEnd = ".git";
			if (originUrl.EndsWith(gitUrlEnd))
			{
				originUrl = originUrl.Remove(originUrl.Length - gitUrlEnd.Length, gitUrlEnd.Length);
			}

			foreach (IUrlTemplateProvider provider in templateProviders)
			{
				if (provider.IsApplicable(originUrl))
				{
					return (provider.CreateUrlTemplate(originUrl), provider.PathSeparator);
				}
			}

			throw new ArgumentException("Filed to find applicable UrlTempalte provider");
		}

		private string GetRepoRoot(string path)
		{
			if (Repository.IsValid(path))
			{
				return path;
			}

			path = Path.GetDirectoryName(path);

			if (string.IsNullOrEmpty(path))
			{
				return null;
			}

			return GetRepoRoot(path);
		}
	}
}