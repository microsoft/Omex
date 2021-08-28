// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;
using Microsoft.Omex.System.Data;
using Microsoft.Omex.System.Data.FileSystem;
using Microsoft.Omex.System.UnitTests.Shared;
using Microsoft.Omex.System.UnitTests.Shared.Data.FileSystem;
using Xunit;

namespace Microsoft.Omex.System.UnitTests.Data
{
	/// <summary>
	/// Unit Tests for FileResource
	/// </summary>
	public sealed class FileResourceUnitTests : UnitTestBase
	{
		[Fact]
		public void Constructor_SetsLocation()
		{
			FileResource fileResource = new FileResource(new IOFile(), FolderName, FileName);
			Assert.Equal(Path.Compine(FolderName, FileName), fileResource.Location);
		}

		[Fact]
		public void Constructor_LocationCannotBeSet_SetsLocationToNameAndLogs()
		{
			FailOnErrors = false;

			string folderWithIncorrectChars = "?folder<:";
			FileResource fileResource = new FileResource(new IOFile(), folderWithIncorrectChars, FileName);
			Assert.Equal(FileName, fileResource.Location);
		}

		[Fact]
		public void GetContent_FileDoesNotExist_ReturnsNotFoundAndLogs()
		{
			FailOnErrors = false;

			FileResource fileResource = new FileResource(new UnitTestTextFile(false, false, null), FolderName, FileName);
			Assert.Equal(ResourceReadStatus.NotFound, fileResource.GetContent(out byte[] content));
			Assert.Null(content);
		}

		[Fact]
		public void GetContent_FileCouldNotBeRead_ReturnsReadFailedAndLogs()
		{
			FailOnErrors = false;

			FileResource fileResource = new FileResource(new UnitTestTextFile(true, false, null), FolderName, FileName);
			Assert.Equal(ResourceReadStatus.ReadFailed, fileResource.GetContent(out byte[] content));
			Assert.Null(content);
		}

		[Fact]
		public void GetContent_FileRead_ReturnsFileContents()
		{
			string content = "file content";
			FileResource fileResource = new FileResource(
				new UnitTestTextFile(true, true, content), FolderName, FileName);
			Assert.Equal(ResourceReadStatus.Success, fileResource.GetContent(out byte[] fileData));
			Assert.NotNull(fileData);
		}

		/// <summary>
		/// Folder Name
		/// </summary>
		private const string FolderName = "c:\\mydir";

		/// <summary>
		/// File Name
		/// </summary>
		private const string FileName = "myfile.xml";
	}
}
