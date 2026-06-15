namespace AIKernel.Wasm.Models;

/// <summary>
/// [EN] Describes a descriptor-driven model that can remain resident behind the WebGPU boundary.
/// [JA] WebGPU 境界の背後に resident として保持できる descriptor-driven model を記述します。
/// </summary>
public sealed record WebGpuResidentModelDescriptor
{
    /// <summary>[EN] Gets the model identifier. [JA] model 識別子を取得します。</summary>
    public string ModelId { get; init; } = string.Empty;

    /// <summary>[EN] Gets the model family as metadata, not as a concrete type name. [JA] concrete 型名ではなく metadata として model family を取得します。</summary>
    public string ModelFamily { get; init; } = string.Empty;

    /// <summary>[EN] Gets the manifest URI or package-relative path. [JA] manifest URI または package-relative path を取得します。</summary>
    public string ManifestRef { get; init; } = string.Empty;

    /// <summary>[EN] Gets the default WGSL entry point identifier. [JA] default WGSL entry point 識別子を取得します。</summary>
    public string EntryPointId { get; init; } = string.Empty;

    /// <summary>[EN] Gets whether zero-copy surface references may be used when the backend supports them. [JA] backend が対応する場合に zero-copy surface 参照を利用できるかどうかを取得します。</summary>
    public bool PreferZeroCopy { get; init; }

    /// <summary>[EN] Gets descriptor metadata. [JA] descriptor metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>
/// [EN] Describes one WebGPU model buffer binding without exposing browser objects.
/// [JA] browser object を公開せず 1 つの WebGPU model buffer binding を記述します。
/// </summary>
public sealed record WebGpuResidentModelBufferBinding
{
    /// <summary>[EN] Gets the logical binding name. [JA] logical binding 名を取得します。</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>[EN] Gets the numeric binding index. [JA] numeric binding index を取得します。</summary>
    public int Binding { get; init; }

    /// <summary>[EN] Gets the payload bytes copied into the model buffer when provided. [JA] 指定された場合に model buffer へ copy される payload byte を取得します。</summary>
    public IReadOnlyList<byte> Payload { get; init; } = [];

    /// <summary>[EN] Gets the buffer byte length. [JA] buffer byte length を取得します。</summary>
    public int ByteLength { get; init; }

    /// <summary>[EN] Gets whether the binding should be read back after dispatch. [JA] dispatch 後に binding を read back するかどうかを取得します。</summary>
    public bool ReadBack { get; init; }

    /// <summary>[EN] Gets binding metadata. [JA] binding metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>
/// [EN] Carries a WebGPU resident model execution request.
/// [JA] WebGPU resident model execution request を保持します。
/// </summary>
public sealed record WebGpuResidentModelExecutionRequest
{
    /// <summary>[EN] Gets the model descriptor. [JA] model descriptor を取得します。</summary>
    public WebGpuResidentModelDescriptor Descriptor { get; init; } = new();

    /// <summary>[EN] Gets WGSL source selected by the model descriptor or caller. [JA] model descriptor または caller が選択した WGSL source を取得します。</summary>
    public string KernelSource { get; init; } = string.Empty;

    /// <summary>[EN] Gets dispatch group count in X. [JA] X 方向の dispatch group 数を取得します。</summary>
    public int DispatchX { get; init; } = 1;

    /// <summary>[EN] Gets dispatch group count in Y. [JA] Y 方向の dispatch group 数を取得します。</summary>
    public int DispatchY { get; init; } = 1;

    /// <summary>[EN] Gets dispatch group count in Z. [JA] Z 方向の dispatch group 数を取得します。</summary>
    public int DispatchZ { get; init; } = 1;

    /// <summary>[EN] Gets buffer bindings for the model dispatch. [JA] model dispatch 用の buffer binding を取得します。</summary>
    public IReadOnlyList<WebGpuResidentModelBufferBinding> Bindings { get; init; } = [];

    /// <summary>[EN] Gets request metadata. [JA] request metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>
/// [EN] Carries WebGPU resident model execution output.
/// [JA] WebGPU resident model execution output を保持します。
/// </summary>
public sealed record WebGpuResidentModelExecutionResult
{
    /// <summary>[EN] Gets whether execution succeeded. [JA] execution が成功したかを取得します。</summary>
    public bool Succeeded { get; init; }

    /// <summary>[EN] Gets a stable failure code when execution failed. [JA] execution が失敗した場合の stable failure code を取得します。</summary>
    public string? ErrorCode { get; init; }

    /// <summary>[EN] Gets a human-readable failure message when execution failed. [JA] execution が失敗した場合の人間可読 message を取得します。</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>[EN] Gets read-back payloads keyed by binding name. [JA] binding 名で keyed された read-back payload を取得します。</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<byte>> Outputs { get; init; } =
        new Dictionary<string, IReadOnlyList<byte>>(StringComparer.Ordinal);

    /// <summary>[EN] Gets whether the dispatch used a zero-copy-capable backend path. [JA] dispatch が zero-copy-capable backend path を使用したかどうかを取得します。</summary>
    public bool ZeroCopyHint { get; init; }

    /// <summary>[EN] Gets deterministic diagnostics. [JA] deterministic diagnostics を取得します。</summary>
    public IReadOnlyList<string> Diagnostics { get; init; } = [];

    /// <summary>[EN] Gets execution metadata. [JA] execution metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}
