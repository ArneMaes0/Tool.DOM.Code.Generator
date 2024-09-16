namespace Skyline.DataMiner.Tools.DOM.Code.Generator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    using Skyline.DataMiner.Generator.DOM.Builders;
    using Skyline.DataMiner.Generator.DOM.Contexts;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Apps.Modules;
    using Skyline.DataMiner.Net.LogHelpers;
    using Skyline.DataMiner.Net.ManagerStore;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.Net.Sections;

    public class DomImporter
    {
        private static readonly JsonSerializer JsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ContractResolver = new DefaultContractResolver { IgnoreSerializableInterface = true }
        });

        private readonly List<ModuleContext> modules = new List<ModuleContext>();
        private JsonTextReader jsonTextReader;

        public DomImporter()
        {
        }

        public List<ModuleContext> Import(string path)
        {
            try
            {
                using (var reader = GetReader(path))
                {
                    jsonTextReader = reader.JsonTextReader;
                    jsonTextReader.Read(); // start array
                    while (jsonTextReader.Read() && jsonTextReader.TokenType == JsonToken.StartObject)
                    {
                        ImportModule();
                        jsonTextReader.Read(); // end object
                    }
                }

                return modules;
            }
            catch (IOException e)
            {
                throw new IOException(e.Message, e);
            }
            catch (JsonException e)
            {
                throw new JsonException("File has an invalid structure.", e);
            }
        }

        private List<CustomSectionDefinition> ImportSectionDefinitions()
        {
            var sections = new List<CustomSectionDefinition>();

            jsonTextReader.Read();
            jsonTextReader.Read();
            while (jsonTextReader.Read() && jsonTextReader.TokenType == JsonToken.StartObject)
            {
                var sectionDefinition = JsonSerializer.Deserialize<CustomSectionDefinition>(jsonTextReader);
                sections.Add(sectionDefinition);
            }

            return sections;
        }

        private List<DomBehaviorDefinition> ImportDomBehaviorDefinitions()
        {
            var behaviors = new List<DomBehaviorDefinition>();

            jsonTextReader.Read();
            jsonTextReader.Read();
            while (jsonTextReader.Read() && jsonTextReader.TokenType == JsonToken.StartObject)
            {
                var behaviorDefinition = JsonSerializer.Deserialize<DomBehaviorDefinition>(jsonTextReader);
                behaviors.Add(behaviorDefinition);
            }

            return behaviors;
        }

        private List<DomDefinition> ImportDomDefinitions()
        {
            var definitions = new List<DomDefinition>();

            jsonTextReader.Read();
            jsonTextReader.Read();
            while (jsonTextReader.Read() && jsonTextReader.TokenType == JsonToken.StartObject)
            {
                var domDefinition = JsonSerializer.Deserialize<DomDefinition>(jsonTextReader);
                definitions.Add(domDefinition);
            }

            return definitions;
        }

        private List<DomTemplate> ImportDomTemplates()
        {
            var domTemplates = new List<DomTemplate>();

            jsonTextReader.Read();
            jsonTextReader.Read();
            while (jsonTextReader.Read() && jsonTextReader.TokenType == JsonToken.StartObject)
            {
                var domTemplate = JsonSerializer.Deserialize<DomTemplate>(jsonTextReader);
                domTemplates.Add(domTemplate);
            }

            return domTemplates;
        }

        private List<DomInstance> ImportDomInstances()
        {
            var domInstances = new List<DomInstance>();

            jsonTextReader.Read();
            jsonTextReader.Read();
            while (jsonTextReader.Read() && jsonTextReader.TokenType == JsonToken.StartObject)
            {
                var domInstance = JsonSerializer.Deserialize<DomInstance>(jsonTextReader);
                domInstances.Add(domInstance);
            }

            return domInstances;
        }

        private void ImportModule()
        {
            jsonTextReader.Read(); // property name
            jsonTextReader.Read(); // start object
            var moduleSettings = JsonSerializer.Deserialize<ModuleSettings>(jsonTextReader);

            var module = BuilderFactory.CreateModuleBuilder()
                .AddModuleId(moduleSettings.ModuleId)
                .AddSections(ImportSectionDefinitions())
                .AddBehaviors(ImportDomBehaviorDefinitions())
                .AddDefinitions(ImportDomDefinitions());

            _ = ImportDomTemplates();
            _ = ImportDomInstances();

            modules.Add(module.Build());
        }

        private IModuleReader GetReader(string path)
        {
            if (path.EndsWith(".json"))
            {
                return new ModuleReader(path);
            }
            else if (path.EndsWith(".zip"))
            {
                return new ZipReader(path);
            }
            else
            {
                throw new NotSupportedException("The given filetype is not supported");
            }
        }

        private interface IModuleReader : IDisposable
        {
            JsonTextReader JsonTextReader { get; }
        }

        private sealed class ZipReader : IModuleReader
        {
            private readonly FileStream fileStream;
            private readonly ZipArchive zipArchive;
            private readonly Stream stream;
            private readonly StreamReader streamReader;

            public ZipReader(string path)
            {
                try
                {
                    fileStream = new FileStream(path, FileMode.Open);
                    zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read);
                    stream = zipArchive.Entries.Single().Open();
                    streamReader = new StreamReader(stream, Encoding.UTF8);
                    JsonTextReader = new JsonTextReader(streamReader);
                    JsonTextReader.SupportMultipleContent = true;
                }
                catch
                {
                    Dispose();
                    throw;
                }
            }

            public JsonTextReader JsonTextReader { get; }

            public void Dispose()
            {
                ((IDisposable)JsonTextReader)?.Dispose();
                streamReader?.Dispose();
                stream?.Dispose();
                zipArchive?.Dispose();
                fileStream?.Dispose();
            }
        }

        private sealed class ModuleReader : IModuleReader
        {
            private readonly FileStream fileStream;
            private readonly Stream stream;
            private readonly StreamReader streamReader;

            public ModuleReader(string path)
            {
                try
                {
                    fileStream = new FileStream(path, FileMode.Open);
                    streamReader = new StreamReader(fileStream, Encoding.UTF8);
                    JsonTextReader = new JsonTextReader(streamReader);
                    JsonTextReader.SupportMultipleContent = true;
                }
                catch
                {
                    Dispose();
                    throw;
                }
            }

            public JsonTextReader JsonTextReader { get; }

            public void Dispose()
            {
                ((IDisposable)JsonTextReader)?.Dispose();
                streamReader?.Dispose();
                stream?.Dispose();
                fileStream?.Dispose();
            }
        }
    }
}
