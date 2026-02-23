#!/usr/bin/env dotnet

#:sdk Microsoft.NET.Sdk

#:property TargetFramework=net10.0
#:property LangVersion=latest
#:property Nullable=enable
#:property ImplicitUsings=enable
#:property PublishAot=false

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

const string LibraryName = "Ray.Automerge.FFI";
const string NativeCrateName = "ray_automerge";

var options = BuildOptions.Parse(args);
var projectDir = ResolveProjectDirectory(options.Project);
if (!Directory.Exists(projectDir))
{
    Console.Error.WriteLine($"Project directory not found: {projectDir}");
    return 1;
}

var nativeDir = Path.Combine(projectDir, "native");
var codegenDir = Path.Combine(projectDir, "codegen");
var generatedDir = Path.Combine(projectDir, "generated");

var targetSuffix = DetectTargetSuffix();
var buildProfile = "release";

if (!options.Configuration.Equals("Release", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine($"Requested configuration '{options.Configuration}' overridden; forcing release build for native artifacts.");
}

var nativeTargetDir = Path.Combine(nativeDir, $"target-{targetSuffix}");
var codegenTargetDir = Path.Combine(codegenDir, $"target-{targetSuffix}");
var nativeCargo = Path.Combine(nativeDir, "Cargo.toml");
var codegenCargo = Path.Combine(codegenDir, "Cargo.toml");
var generatedFile = Path.Combine(generatedDir, "Interop.g.cs");
var nativeLibPath = Path.Combine(nativeTargetDir, buildProfile, GetNativeLibraryName(targetSuffix));
var buildInfoPath = Path.Combine(projectDir, "build-info.json");

Console.WriteLine($"=== Building {LibraryName} ({targetSuffix}, {buildProfile}) ===");

EnsureDirectoryExists(nativeTargetDir);
EnsureDirectoryExists(codegenTargetDir);

var buildInfo = BuildInfoStorage.Load(buildInfoPath);
var currentChecksums = ChecksumCollector.Collect(projectDir);

var nativeChanged = ChecksumCollector.HasDifferences(buildInfo.Checksums, currentChecksums, "native/");
var codegenChanged = ChecksumCollector.HasDifferences(buildInfo.Checksums, currentChecksums, "codegen/");
var platformChanged = !string.Equals(buildInfo.TargetSuffix, targetSuffix, StringComparison.OrdinalIgnoreCase);
var profileChanged = !string.Equals(buildInfo.Profile, buildProfile, StringComparison.OrdinalIgnoreCase);

var needsNativeBuild = options.Force || nativeChanged || platformChanged || profileChanged || !File.Exists(nativeLibPath);
var needsCodegen = options.Force || codegenChanged || needsNativeBuild || !File.Exists(generatedFile);

if (needsNativeBuild)
{
    Console.WriteLine("-> Compiling native library with cargo.");
    var buildArgs = $"build --release --manifest-path \"{nativeCargo}\" --target-dir \"{nativeTargetDir}\"";

    var result = RunProcess("cargo", buildArgs, nativeDir, options.Verbose);

    if (result != 0)
    {
        Console.Error.WriteLine("Failed to build native library.");
        return result;
    }

    if (!File.Exists(nativeLibPath))
    {
        Console.Error.WriteLine($"Expected native artifact not found: {nativeLibPath}");
        return 1;
    }
}
else
{
    Console.WriteLine("-> Native library up-to-date, skipping rebuild.");
}

if (needsCodegen)
{
    Console.WriteLine("-> Running Interoptopus code generator.");
    var runArgs = $"run --release --manifest-path \"{codegenCargo}\" --target-dir \"{codegenTargetDir}\"";
    var result = RunProcess("cargo", runArgs, codegenDir, options.Verbose);
    if (result != 0)
    {
        Console.Error.WriteLine("Code generation failed.");
        return result;
    }

    if (!File.Exists(generatedFile))
    {
        Console.Error.WriteLine($"Expected generated file not found: {generatedFile}");
        return 1;
    }
}
else
{
    Console.WriteLine("-> Code generation sources unchanged, skipping.");
}

if (!string.IsNullOrWhiteSpace(options.OutputDirectory))
{
    CopyNativeOutputs(nativeTargetDir, buildProfile, options.OutputDirectory);
}

buildInfo.TargetSuffix = targetSuffix;
buildInfo.Profile = buildProfile;
buildInfo.Checksums = currentChecksums;
BuildInfoStorage.Save(buildInfoPath, buildInfo);

Console.WriteLine("Build script completed successfully.");
return 0;

static string ResolveProjectDirectory(string? providedPath)
{
    if (!string.IsNullOrWhiteSpace(providedPath))
    {
        return Path.GetFullPath(providedPath);
    }

    var cwd = Directory.GetCurrentDirectory();
    var probe = Path.Combine(cwd, "libs", LibraryName);
    if (Directory.Exists(probe))
    {
        return Path.GetFullPath(probe);
    }

    probe = Path.Combine(cwd, LibraryName);
    if (Directory.Exists(probe))
    {
        return Path.GetFullPath(probe);
    }

    return cwd;
}

static string DetectTargetSuffix()
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        return "windows";
    }

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        return "linux";
    }

    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
        return "macos";
    }

    return "unix";
}

static string GetNativeLibraryName(string targetSuffix)
{
    return targetSuffix switch
    {
        "windows" => $"{NativeCrateName}.dll",
        "macos" => $"lib{NativeCrateName}.dylib",
        _ => $"lib{NativeCrateName}.so"
    };
}

static void EnsureDirectoryExists(string path)
{
    if (!Directory.Exists(path))
    {
        Directory.CreateDirectory(path);
    }
}

static int RunProcess(string fileName, string arguments, string workingDirectory, bool verbose)
{
    Console.WriteLine($"> {fileName} {arguments}");

    var startInfo = new ProcessStartInfo
    {
        FileName = fileName,
        Arguments = arguments,
        WorkingDirectory = workingDirectory,
        RedirectStandardOutput = verbose,
        RedirectStandardError = verbose,
        UseShellExecute = !verbose
    };

    try
    {
        using var process = Process.Start(startInfo);
        if (process == null)
        {
            Console.Error.WriteLine($"Failed to start process: {fileName}");
            return 1;
        }

        if (verbose)
        {
            process.OutputDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Console.WriteLine(e.Data);
                }
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Console.Error.WriteLine(e.Data);
                }
            };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        process.WaitForExit();
        return process.ExitCode;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error running process: {ex.Message}");
        return 1;
    }
}

static void CopyNativeOutputs(string nativeTargetDir, string buildProfile, string outputDirectory)
{
    var releaseDir = Path.Combine(nativeTargetDir, buildProfile);
    if (!Directory.Exists(releaseDir))
    {
        Console.WriteLine($"Native output directory does not exist: {releaseDir}");
        return;
    }

    var targets = new[]
    {
        "ray_automerge.dll",
        "libray_automerge.so",
        "libray_automerge.dylib"
    };

    EnsureDirectoryExists(outputDirectory);

    foreach (var fileName in targets)
    {
        var source = Path.Combine(releaseDir, fileName);
        if (!File.Exists(source))
        {
            continue;
        }

        var destination = Path.Combine(outputDirectory, fileName);
        File.Copy(source, destination, overwrite: true);
        Console.WriteLine($"Copied {fileName} -> {destination}");
    }
}

sealed class BuildOptions
{
    public string? Project { get; set; }
    public string OutputDirectory { get; set; } = string.Empty;
    public string Configuration { get; set; } = "Release";
    public bool Force { get; set; }
    public bool Verbose { get; set; }

    public static BuildOptions Parse(string[] arguments)
    {
        var options = new BuildOptions();

        for (var i = 0; i < arguments.Length; i++)
        {
            var arg = arguments[i];

            switch (arg)
            {
                case "--project":
                    options.Project = GetValue(arguments, ref i);
                    break;
                case "--out-dir":
                    options.OutputDirectory = Path.GetFullPath(GetValue(arguments, ref i));
                    break;
                case "--configuration":
                    options.Configuration = GetValue(arguments, ref i);
                    break;
                case "--force":
                case "-f":
                    options.Force = true;
                    break;
                case "--verbose":
                case "-v":
                    options.Verbose = true;
                    break;
                default:
                    Console.WriteLine($"Unknown argument ignored: {arg}");
                    break;
            }
        }

        return options;
    }

    private static string GetValue(string[] args, ref int index)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"Missing value for {args[index]}");
        }

        index++;
        return args[index];
    }
}

sealed class BuildInfo
{
    public string TargetSuffix { get; set; } = string.Empty;
    public string Profile { get; set; } = string.Empty;
    public Dictionary<string, string> Checksums { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

static class BuildInfoStorage
{
    public static BuildInfo Load(string path)
    {
        if (!File.Exists(path))
        {
            return new BuildInfo();
        }

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<BuildInfo>(json) ?? new BuildInfo();
        }
        catch
        {
            return new BuildInfo();
        }
    }

    public static void Save(string path, BuildInfo buildInfo)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(buildInfo, options);
        File.WriteAllText(path, json);
    }
}

static class ChecksumCollector
{
    public static Dictionary<string, string> Collect(string projectDir)
    {
        var checksums = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var nativeDir = Path.Combine(projectDir, "native");
        var codegenDir = Path.Combine(projectDir, "codegen");

        AddFile(checksums, Path.Combine(nativeDir, "Cargo.toml"), Path.Combine("native", "Cargo.toml"));
        AddFile(checksums, Path.Combine(codegenDir, "Cargo.toml"), Path.Combine("codegen", "Cargo.toml"));

        AddDirectory(checksums, Path.Combine(nativeDir, "src"), "native/src");
        AddDirectory(checksums, Path.Combine(codegenDir, "src"), "codegen/src");

        return checksums;
    }

    public static bool HasDifferences(
        Dictionary<string, string> previous,
        Dictionary<string, string> current,
        string prefix)
    {
        foreach (var kvp in current)
        {
            if (!kvp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!previous.TryGetValue(kvp.Key, out var oldChecksum) || !oldChecksum.Equals(kvp.Value, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        foreach (var kvp in previous)
        {
            if (!kvp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!current.ContainsKey(kvp.Key))
            {
                return true;
            }
        }

        return false;
    }

    private static void AddDirectory(Dictionary<string, string> checksums, string directory, string prefix)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
        {
            if (!file.EndsWith(".rs", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var relative = Path.GetRelativePath(directory, file).Replace('\\', '/');
            var key = $"{prefix}/{relative}";
            checksums[key] = ComputeChecksum(file);
        }
    }

    private static void AddFile(Dictionary<string, string> checksums, string path, string key)
    {
        if (File.Exists(path))
        {
            checksums[key] = ComputeChecksum(path);
        }
    }

    private static string ComputeChecksum(string file)
    {
        using var stream = File.OpenRead(file);
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(stream);
        return Convert.ToHexString(hash);
    }
}
