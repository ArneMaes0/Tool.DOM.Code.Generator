namespace DOM_Code_Generator
{
	using System;
	using System.Collections.Generic;
	using System.CommandLine;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.DataMiner.Generator.DOM;

	public static class Program
	{
		public static void Main(string[] args)
		{
			var isDebug = new Option<bool>(
			name: "--debug",
			description: "Indicates the tool should write out debug logging.")
			{
				IsRequired = false,
			};

			isDebug.SetDefaultValue(true);

			var fileArgument = new Option<FileInfo>(
				name: "--file",
				description: "The path to the file containing the DOM modules.")
			{
				IsRequired = true
			};

			var rootCommand = new RootCommand("")
			{
				isDebug,
				fileArgument,
			};

			rootCommand.SetHandler(Process, isDebug, fileArgument);
			_ = rootCommand.InvokeAsync(args);
		}

		private static void Process(bool isDebug, FileInfo file)
		{
			try
			{
				try
				{
					//Main Code for program here
					var importer = new DomImporter();
					var code = CodeGenerator.Generate(importer.Import(file.FullName));
					Console.WriteLine(code);
				}
				catch (Exception e)
				{
					Console.WriteLine($"Exception during Process Run: {e}");
				}
			}
			catch (Exception e)
			{
				Console.WriteLine($"Exception on Logger Creation: {e}");
			}
		}
	}
}
