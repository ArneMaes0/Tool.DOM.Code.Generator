namespace Skyline.DataMiner.Tools.DOM.Code.Generator
{
    using System;
    using System.CommandLine;
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Serilog;

    using Skyline.DataMiner.Generator.DOM;

    /// <summary>
    /// .
    /// </summary>
    public static class Program
    {
        /*
         * Design guidelines for command line tools: https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax#design-guidance
         */

        /// <summary>
        /// Code that will be called when running the tool.
        /// </summary>
        /// <param name="args">Extra arguments.</param>
        /// <returns>0 if successful.</returns>
        public static async Task<int> Main(string[] args)
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

            return await rootCommand.InvokeAsync(args);
        }

        private static async Task<int> Process(bool isDebug, FileInfo file)
        {
            try
            {
                var logConfig = new LoggerConfiguration().WriteTo.Console();
                logConfig.MinimumLevel.Is(isDebug ? Serilog.Events.LogEventLevel.Debug : Serilog.Events.LogEventLevel.Information);
                var seriLog = logConfig.CreateLogger();

                using(var loggerFactory = LoggerFactory.Create(builder => builder.AddSerilog(seriLog)))
                {
                    var logger = loggerFactory.CreateLogger("Skyline.DataMiner.Tools.DOM.Code.Generator");
                    try
                    {
                        //Main Code for program here
                        var importer = new DomImporter();
                        var code = CodeGenerator.Generate(importer.Import(file.FullName));
                        Console.WriteLine(code);
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"Exception during Process Run: {e}");
                        return 1;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception on Logger Creation: {e}");
                return 1;
            }

            return 0;
        }
    }
}