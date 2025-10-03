using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;


// Roslyn specific usings
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Indicators
{
    // NamesFilesFromFolder can remain as is if used by other parts,
    // or its logic can be integrated/simplified if only used here.
    // For this refactor, it's kept as per the original structure.
    public class NamesFilesFromFolder
    {
        public string Folder;
        public List<string> Files;

        public List<string> GetFilesCopy()
        {
            // Defensive copy
            return Files != null ? [.. Files] : [];
        }
    }

    public static class IndicatorsFactory // Made static to match CandleFactory and common factory patterns
    {
        public static readonly Dictionary<string, Type> IndicatorsWithAttribute = GetTypesWithIndicatorAttribute();

        private static Dictionary<string, Type> GetTypesWithIndicatorAttribute()
        {
            // Assumes BaseIndicator is in OsEngine.Entity or OsEngine.Indicators
            Assembly assembly = typeof(BaseIndicator).Assembly;
            Dictionary<string, Type> indicators = [];
            foreach (Type type in assembly.GetTypes())
            {
                // Ensure type is public, not abstract, and assignable to BaseIndicator
                if (type.IsPublic && !type.IsAbstract && typeof(BaseIndicator).IsAssignableFrom(type))
                {
                    object[] attributes = type.GetCustomAttributes(typeof(IndicatorAttribute), false);
                    if (attributes.Length > 0 && attributes[0] is IndicatorAttribute attr)
                    {
                        indicators[attr.Name] = type;
                    }
                }
            }
            return indicators;
        }

        public static List<string> GetIndicatorsNames()
        {
            const string customIndicatorsScriptPath = @"Custom\Indicators\Scripts";

            if (!Directory.Exists(@"Custom"))
            {
                Directory.CreateDirectory(@"Custom");
            }
            if (!Directory.Exists(@"Custom\Indicators"))
            {
                Directory.CreateDirectory(@"Custom\Indicators");
            }
            if (!Directory.Exists(customIndicatorsScriptPath))
            {
                Directory.CreateDirectory(customIndicatorsScriptPath);
            }

            List<string> scriptFileNames = [.. GetFullNamesFromFolder(customIndicatorsScriptPath)
                .Select(Path.GetFileNameWithoutExtension)
            ];

            // The original custom sorting logic was a bit complex.
            // Combining with attribute-based indicators and then sorting alphabetically.
            List<string> allIndicatorNames = [.. scriptFileNames, .. IndicatorsWithAttribute.Keys];

            // Remove duplicates that might arise if a script has the same name as an attribute-based indicator
            // and then sort alphabetically.
            return [.. allIndicatorNames.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(name => name)];
        }

        private static readonly List<NamesFilesFromFolder> _filesInDirCache = [];
        private static readonly Lock _filesInDirCacheLock = new();


        public static List<string> GetFullNamesFromFolder(string directory)
        {
            lock (_filesInDirCacheLock)
            {
                var cachedEntry = _filesInDirCache.FirstOrDefault(f => f != null && f.Folder.Equals(directory, StringComparison.OrdinalIgnoreCase));
                if (cachedEntry != null)
                {
                    return cachedEntry.GetFilesCopy();
                }
            }

            List<string> results = [];
            try
            {
                string[] subDirectories = Directory.GetDirectories(directory);
                foreach (string subDir in subDirectories)
                {
                    // If "Dlls" folders should be excluded from recursive search:
                    // if (Path.GetFileName(subDir).Equals("Dlls", StringComparison.OrdinalIgnoreCase)) continue;
                    results.AddRange(GetFullNamesFromFolder(subDir));
                }

                string[] files = Directory.GetFiles(directory);
                results.AddRange(files);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing directory {directory}: {ex.Message}");
                // Depending on requirements, either throw or return partial/empty results
            }

            lock (_filesInDirCacheLock)
            {
                // Double-check before adding to avoid race conditions if multiple threads hit this for the first time
                if (!_filesInDirCache.Any(f => f != null && f.Folder.Equals(directory, StringComparison.OrdinalIgnoreCase)))
                {
                    NamesFilesFromFolder dirEntry = new()
                    {
                        Folder = directory,
                        Files = [.. results] // Store a copy in the cache
                    };
                    _filesInDirCache.Add(dirEntry);
                }
            }
            return [.. results]; // Return a copy
        }

        public static BaseIndicator CreateIndicatorByName(string nameClass, string name, bool canDelete, StartProgram startProgram = StartProgram.IsOsTrader)
        {
            BaseIndicator indicator = null;

            try
            {
                if (IndicatorsWithAttribute.TryGetValue(nameClass, out Type precompiledType))
                {
                    indicator = (BaseIndicator)Activator.CreateInstance(precompiledType);
                }

                if (indicator == null)
                {
                    const string scriptFolderPath = @"Custom\Indicators\Scripts";
                    if (!Directory.Exists(scriptFolderPath))
                        Directory.CreateDirectory(scriptFolderPath); // Should already exist from GetIndicatorsNames

                    List<string> fullPaths = GetFullNamesFromFolder(scriptFolderPath);

                    // Scripts can be .cs or .txt
                    string scriptFileNameTxt = nameClass + ".txt";
                    string scriptFileNameCs = nameClass + ".cs";
                    string scriptPath = "";

                    foreach (string fullPath in fullPaths)
                    {
                        string fileName = Path.GetFileName(fullPath);
                        if (fileName.Equals(scriptFileNameTxt, StringComparison.OrdinalIgnoreCase) ||
                            fileName.Equals(scriptFileNameCs, StringComparison.OrdinalIgnoreCase))
                        {
                            scriptPath = fullPath;
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(scriptPath))
                    {
                        MessageBox.Show($"Error! Indicator script '{nameClass}' not found in {scriptFolderPath}");
                        return null;
                    }

                    indicator = CompileAndInstantiateIndicatorScript(scriptPath, nameClass);
                }

                if (indicator != null)
                {
                    indicator.Init(name, startProgram);
                    indicator.CanDelete = canDelete;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error creating indicator '{nameClass}':\n{e}");
                // Consider logging the exception or re-throwing specific exceptions
            }

            return indicator;
        }

        private static List<MetadataReference> _baseReferences;
        private static readonly Lock _referencesLock = new();
        // Cache for successfully compiled indicator types. Key: nameClass, Value: Type
        private static readonly Dictionary<string, Type> _compiledIndicatorTypesCache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Lock _compiledTypesCacheLock = new();


        // Comparer for MetadataReference based on Display path to avoid duplicates
        private class MetadataReferenceComparer : IEqualityComparer<MetadataReference>
        {
            public bool Equals(MetadataReference x, MetadataReference y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null || y is null) return false;
                return x.Display.Equals(y.Display, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(MetadataReference obj)
            {
                return obj?.Display?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0;
            }
        }


        private static void InitializeBaseReferences()
        {
            if (_baseReferences == null)
            {
                lock (_referencesLock)
                {
                    if (_baseReferences == null) // Double-check locking
                    {
                        var references = new HashSet<MetadataReference>(new MetadataReferenceComparer());
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            if (!assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
                            {
                                try
                                {
                                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Warning: Could not create metadata reference for {assembly.FullName} from {assembly.Location}. {ex.Message}");
                                }
                            }
                        }
                        _baseReferences = [.. references];
                    }
                }
            }
        }

        private static BaseIndicator CompileAndInstantiateIndicatorScript(string scriptPath, string nameClass)
        {
            Type indicatorType;
            lock (_compiledTypesCacheLock)
            {
                if (_compiledIndicatorTypesCache.TryGetValue(nameClass, out indicatorType))
                {
                    return (BaseIndicator)Activator.CreateInstance(indicatorType);
                }
            }

            InitializeBaseReferences();
            List<MetadataReference> currentCompilationReferences = [.. _baseReferences];

            List<string> dllsFromScriptFolder = GetDllsPathFromScriptFolder(scriptPath);
            if (dllsFromScriptFolder != null)
            {
                foreach (string dllPath in dllsFromScriptFolder)
                {
                    if (!currentCompilationReferences.Any(r => r.Display.Equals(dllPath, StringComparison.OrdinalIgnoreCase)))
                    {
                        try
                        {
                            currentCompilationReferences.Add(MetadataReference.CreateFromFile(dllPath));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Warning: Could not create metadata reference for custom DLL {dllPath}. {ex.Message}");
                        }
                    }
                }
            }

            string sourceCode = File.ReadAllText(scriptPath);
            if (string.IsNullOrWhiteSpace(sourceCode))
            {
                throw new InvalidOperationException($"Source code file is empty or could not be read: {scriptPath}");
            }

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
                sourceCode,
                CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest));

            string assemblyName = "Indicator_" + Path.GetFileNameWithoutExtension(scriptPath) + "_" + Guid.NewGuid().ToString("N");
            CSharpCompilationOptions compilationOptions = new(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Debug, // Or Release
                platform: Platform.AnyCpu);

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: [syntaxTree],
                references: currentCompilationReferences,
                options: compilationOptions);

            using var assemblyStream = new MemoryStream();
            using var pdbStream = new MemoryStream();
            EmitOptions emitOptions = new(debugInformationFormat: DebugInformationFormat.PortablePdb);
            EmitResult result = compilation.Emit(assemblyStream, pdbStream, options: emitOptions);

            if (!result.Success)
            {
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
                string errorString = $"Error! Indicator script compilation problem for: {scriptPath}\n";
                foreach (var diagnostic in failures)
                {
                    errorString += $"  {diagnostic.Id}: {diagnostic.GetMessage()} at {diagnostic.Location.GetLineSpan()}\n";
                }
                throw new Exception(errorString);
            }

            assemblyStream.Seek(0, SeekOrigin.Begin);
            pdbStream.Seek(0, SeekOrigin.Begin);
            Assembly compiledAssembly = Assembly.Load(assemblyStream.ToArray(), pdbStream.ToArray());

            // Try to find the type by nameClass, or the first public BaseIndicator derivative
            indicatorType = compiledAssembly.GetTypes().FirstOrDefault(t => t.Name.Equals(nameClass, StringComparison.OrdinalIgnoreCase) && typeof(BaseIndicator).IsAssignableFrom(t) && t.IsPublic && !t.IsAbstract)
                         ?? compiledAssembly.GetTypes().FirstOrDefault(t => typeof(BaseIndicator).IsAssignableFrom(t) && t.IsPublic && !t.IsAbstract);


            if (indicatorType == null)
            {
                throw new TypeLoadException($"Could not find a suitable public BaseIndicator type (ideally named '{nameClass}') in compiled script: {scriptPath}");
            }

            lock (_compiledTypesCacheLock)
            {
                // Add to cache if not already added by another thread
                if (_compiledIndicatorTypesCache.TryGetValue(nameClass, out Type value))
                {
                    indicatorType = value;
                }
                else // If another thread just compiled and cached it, use that type
                {
                    _compiledIndicatorTypesCache[nameClass] = indicatorType;
                }
            }
            return (BaseIndicator)Activator.CreateInstance(indicatorType);
        }

        private static List<string> GetDllsPathFromScriptFolder(string scriptFilePath)
        {
            string scriptFolder = Path.GetDirectoryName(scriptFilePath);
            if (string.IsNullOrEmpty(scriptFolder)) return null;

            string dllsFolder = Path.Combine(scriptFolder, "Dlls");
            if (!Directory.Exists(dllsFolder)) return null;

            return [.. Directory.GetFiles(dllsFolder, "*.dll")];
        }
    }
}
