namespace AIKernel.Wasm.Tests;

using System.Text.RegularExpressions;
using AIKernel.Wasm.Runtime.Concepts;

/// <summary>
/// EN: Verifies WASM concept names stay above JS interop, native bridge, and low-level I/O layers.
/// JA: WASM の概念名が JS interop / native bridge / low-level I/O より上位に留まることを検証します。
/// </summary>
public sealed class ConceptElevationArchitectureTests
{
    private static readonly string[] PhilosophicalPrefixes =
    [
        "Ethos",
        "Pathos",
        "Logos",
        "Nomos",
        "Dike",
        "Kratos",
        "Aisthesis",
        "Phantasia",
        "Chronos",
        "Kairos",
        "Dynamis",
        "Energeia",
        "Nous",
        "Telos",
        "Apatheia",
        "Ataraxia",
        "Eidos",
    ];

    private static readonly string[] ForbiddenTechnicalSuffixes =
    [
        "Dto",
        "Request",
        "Result",
        "Mapper",
        "Adapter",
        "Serializer",
        "Converter",
        "HttpClient",
        "JSInterop",
        "JsInterop",
        "NativeBridge",
        "Provider",
    ];

    private static readonly Regex TypeDeclarationPattern = new(
        @"\b(?:public|internal|private|protected)?\s*(?:sealed\s+|abstract\s+|static\s+|partial\s+)*\b(?:class|record|interface|enum)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)",
        RegexOptions.Compiled);

    /// <summary>
    /// EN: Confirms WASM concept facades expose deterministic perception and temporal helpers only.
    /// JA: WASM concept facade が deterministic perception / temporal helper のみを公開することを確認します。
    /// </summary>
    [Fact]
    public void ConceptFacades_WhenUsed_ReturnStableRuntimeValues()
    {
        var frameSource = new AisthesisFrameSource();
        var observationSurface = new AisthesisObservationSurface();
        var sceneSurface = new PhantasiaSceneSurface();
        var frameModel = new PhantasiaFrameModel();
        var window = new ChronosWindow();
        var buffer = new ChronosBuffer();
        var replayWindow = new ChronosReplayWindow();
        var audioWindow = new ChronosAudioWindow();
        var audioBuffer = new ChronosAudioBuffer();
        var playbackTimeline = new ChronosPlaybackTimeline();
        var trigger = new KairosTrigger();
        var frameSignal = new KairosFrameSignal();
        var actionTiming = new KairosActionTiming();
        var inputTrigger = new KairosInputTrigger();
        var now = DateTimeOffset.Parse("2026-01-01T00:00:00Z", null, System.Globalization.DateTimeStyles.AssumeUniversal);

        Assert.Equal([1, 2, 3], frameSource.Capture(new byte[] { 1, 2, 3 }));
        Assert.Equal("aisthesis.surface.runtime", observationSurface.SurfaceId("runtime"));
        Assert.Equal("phantasia.scene.surface", sceneSurface.Label("surface"));
        Assert.Equal("source:7", frameModel.Key("source", 7));
        Assert.True(window.Contains(now, now.AddSeconds(-1), now.AddSeconds(1)));
        Assert.Equal([now.AddSeconds(-1), now], buffer.Order([now, now.AddSeconds(-1)]));
        Assert.Equal($"{now.AddSeconds(-1):O}/{now:O}", replayWindow.Label(now.AddSeconds(-1), now));
        Assert.True(audioWindow.Contains(TimeSpan.FromSeconds(1), TimeSpan.Zero, TimeSpan.FromSeconds(2)));
        Assert.Equal(TimeSpan.FromSeconds(1), audioBuffer.Duration(48000, 48000));
        Assert.Equal("audio:48000", playbackTimeline.Segment("audio", TimeSpan.FromTicks(48000)));
        Assert.True(trigger.IsReady(now, now.AddSeconds(-1), TimeSpan.Zero));
        Assert.Equal("frame-ready:1", frameSignal.Label("frame-ready", 1));
        Assert.True(actionTiming.IsWithinTolerance(now, now, TimeSpan.Zero));
        Assert.Equal($"input:{now:O}", inputTrigger.Label("input", now));
    }

    /// <summary>
    /// EN: Rejects philosophical prefixes on forbidden WASM infrastructure names.
    /// JA: 禁止された WASM infrastructure 名への哲学語 prefix を拒否します。
    /// </summary>
    [Fact]
    public void SourceTypes_WhenUsingPhilosophicalPrefix_DoNotUseForbiddenTechnicalSuffix()
    {
        var violations = FindViolations("AIKernel.Wasm.slnx");

        Assert.Empty(violations);
    }

    /// <summary>
    /// EN: Confirms forbidden platform and scenario dependencies stay out of Wasm projects.
    /// JA: 禁止された platform / scenario dependency が Wasm project に入らないことを確認します。
    /// </summary>
    [Fact]
    public void ProjectFiles_WhenReferencingDependencies_KeepWasmBoundaryClean()
    {
        var repositoryRoot = FindRepositoryRoot("AIKernel.Wasm.slnx");
        var projectFiles = Directory.EnumerateFiles(repositoryRoot, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToArray();
        var forbidden = new[]
        {
            "AIKernel.Doom",
            "AIKernel.Tools",
            "AIKernel.WindowsAI",
            "AIKernel.Audio.NAudio",
            "AIKernel.Audio.Sdl",
            "AIKernel.Cuda13.0",
            "AIKernel.Providers.Audio",
            "AIKernel.Providers.Compute",
            "AIKernel.Providers.Substrate"
        };

        var violations = projectFiles
            .SelectMany(path =>
            {
                var source = File.ReadAllText(path);
                return forbidden
                    .Where(item => source.Contains(item, StringComparison.Ordinal))
                    .Where(item => item != "AIKernel.Providers.Standard")
                    .Select(item => $"{Path.GetRelativePath(repositoryRoot, path)}: {item}");
            })
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(violations);
    }

    /// <summary>
    /// EN: Confirms WebAudio JS interop is contained in the WASM audio package.
    /// JA: WebAudio JS interop が WASM audio package に閉じていることを確認します。
    /// </summary>
    [Fact]
    public void WebAudioInterop_WhenDeclared_StaysInsideWasmAudioPackage()
    {
        var repositoryRoot = FindRepositoryRoot("AIKernel.Wasm.slnx");
        var sourceRoot = Path.Combine(repositoryRoot, "src");
        var violations = Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => File.ReadAllText(path).Contains("IWebAudioJsInterop", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}Audio{Path.DirectorySeparatorChar}AIKernel.Wasm.Audio{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(repositoryRoot, path))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(violations);
    }

    private static IReadOnlyList<string> FindViolations(string solutionFileName)
    {
        var repositoryRoot = FindRepositoryRoot(solutionFileName);
        var sourceRoot = Path.Combine(repositoryRoot, "src");

        return Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .SelectMany(path => FindViolationsInFile(repositoryRoot, path))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static IEnumerable<string> FindViolationsInFile(string repositoryRoot, string path)
    {
        var source = File.ReadAllText(path);
        foreach (Match match in TypeDeclarationPattern.Matches(source))
        {
            var typeName = match.Groups["name"].Value;
            var hasPhilosophicalPrefix = PhilosophicalPrefixes.Any(prefix => typeName.StartsWith(prefix, StringComparison.Ordinal));
            var hasForbiddenTechnicalSuffix = ForbiddenTechnicalSuffixes.Any(suffix => typeName.EndsWith(suffix, StringComparison.Ordinal));
            var isConceptSurface = path.Contains($"{Path.DirectorySeparatorChar}Concepts{Path.DirectorySeparatorChar}", StringComparison.Ordinal);

            if (hasPhilosophicalPrefix && (hasForbiddenTechnicalSuffix || !isConceptSurface))
            {
                yield return $"{Path.GetRelativePath(repositoryRoot, path)}: {typeName}";
            }
        }
    }

    private static string FindRepositoryRoot(string solutionFileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, solutionFileName)))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException($"Could not locate {solutionFileName}.");
    }
}
