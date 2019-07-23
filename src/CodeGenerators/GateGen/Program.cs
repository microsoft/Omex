// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Omex.Gating;
using Microsoft.Omex.System.Data;

namespace Microsoft.Omex.CodeGenerators.GateGen
{
	/// <summary>
	/// Generates strongly typed classes for gates
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// Main entry point from command line.
		/// </summary>
		/// <param name="arguments">Command line arguments</param>
		/// <returns>0 if successfull; 1 otherwise</returns>
		public static int Main(string[] arguments)
		{
			ConsoleColor originalColor = Console.ForegroundColor;
			const string version = "2.0.0.0";
			try
			{
				if (arguments.Length < 3)
				{
					Console.WriteLine("Omex gate generator, version {0}", version);
					Console.WriteLine("Usage: gategen.exe omexgates.xml omextip.xml output.cs [namespace]");
					return 1;
				}

				GateDataSet gateDataSet = null;
				try
				{
					gateDataSet = LoadGateDataSet(arguments[0], arguments[1]);
				}
				catch (Exception ex)
				{
					Console.ForegroundColor = ConsoleColor.Red;

					Console.WriteLine("Unable to read file {0}.", arguments[0]);
					Console.WriteLine(ex);

					return 1;
				}
				finally
				{
					Console.ForegroundColor = originalColor;
				}

				GateItem root = new GateItem();
				BuildTree(root, gateDataSet);

				AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();

				try
				{
					string omexGatesNamespace = "Microsoft.Omex.Gating";
					if (arguments.Length >= 4 && !string.IsNullOrWhiteSpace(arguments[3]))
					{
						omexGatesNamespace = arguments[3];
					}

					using (StreamWriter writer = new StreamWriter(arguments[2]))
					{
						writer.Write(string.Format(CultureInfo.InvariantCulture, Properties.Resources.GatesClassTemplate, assemblyName.Name, version,
							OutputTree(root, "\t"), omexGatesNamespace, "OmexGates", "Microsoft.Omex.Gating.Gates"));
					}
				}
				catch (Exception ex)
				{
					Console.ForegroundColor = ConsoleColor.Red;

					Console.WriteLine("Unable to write file {0}.", arguments[1]);
					Console.WriteLine(ex);

					return 1;
				}
			}
			finally
			{
				Console.ForegroundColor = originalColor;
			}

			return 0;
		}


		/// <summary>
		/// Build up the tree structure of the gates from the gate cache
		/// </summary>
		/// <param name="root">root of the tree</param>
		/// <param name="cache">gate cache</param>
		private static void BuildTree(GateItem root, GateDataSet cache)
		{
			foreach (string name in cache.GateNames)
			{
				string[] sections = name.Split('.');
				GateItem parentItem = root;
				GateItem item = null;
				for (int i = 0; i < sections.Length; i++)
				{
					if (!parentItem.Gates.TryGetValue(sections[i], out item))
					{
                        item = new GateItem
                        {
                            GateGroupName = sections[i]
                        };
                        parentItem.Gates[sections[i]] = item;
					}
					parentItem = item;
				}
				if (item != null)
				{
					item.Name = name;
				}
			}
		}


		/// <summary>
		/// Output tree of gates to settings file
		/// </summary>
		/// <param name="root">the root of the (sub)tree</param>
		/// <param name="indent">string used for indentation</param>
		/// <returns>string representing the (sub)tree</returns>
		private static string OutputTree(GateItem root, string indent)
		{
			StringBuilder builder = new StringBuilder(2000);
			if (!string.IsNullOrWhiteSpace(root.Name))
			{
				// This is a gate to output
				builder.AppendFormat(CultureInfo.InvariantCulture, Properties.Resources.GatePropertyTemplate, root.GateGroupName, root.Name, indent);
			}

			int childCount = root.Gates.Count;
			if (childCount > 0)
			{
				StringBuilder childGates = new StringBuilder(childCount * 100);
				foreach (GateItem child in root.Gates.Values)
				{
					childGates.Append(OutputTree(child, indent + "\t"));
					childCount--;
					if (childCount > 0)
					{
						childGates.AppendLine();
						childGates.AppendLine();
					}
				}
				if (string.IsNullOrWhiteSpace(root.GateGroupName))
				{
					builder.Append(childGates.ToString());
				}
				else
				{
					builder.AppendFormat(CultureInfo.InvariantCulture, Properties.Resources.NestingTemplate, indent, root.GateGroupName, childGates.ToString());
				}
			}

			return builder.ToString();
		}


		/// <summary>
		/// Loads GateDataSet from file
		/// </summary>
		/// <param name="settingsXml">The settings XML.</param>
		/// <param name="tipXml">The TIP XML.</param>
		/// <returns>Loaded Gates DataSet</returns>
		private static GateDataSet LoadGateDataSet(string settingsXml, string tipXml)
		{
			FileInfo settingsFile = new FileInfo(settingsXml);
			FileInfo tipFile = new FileInfo(tipXml);
			IDictionary<string, IResourceDetails> resources =
				new Dictionary<string, IResourceDetails>(2, StringComparer.OrdinalIgnoreCase)
			{
				{  settingsFile.Name, new ResourceDetails(settingsFile.LastWriteTimeUtc , settingsFile.Length, File.ReadAllBytes(settingsFile.FullName)) },
				{ tipFile.Name, new ResourceDetails(tipFile.LastWriteTimeUtc , tipFile.Length, File.ReadAllBytes(tipFile.FullName)) }
			};
			GateDataSet dataSet = new GateDataSet(settingsFile.Name, tipFile.Name);
			dataSet.Load(resources);
			return dataSet;
		}
	}
}
