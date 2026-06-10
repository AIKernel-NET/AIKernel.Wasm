namespace AIKernel.Wasm.Runtime;

using AIKernel.Abstractions.Models;
using AIKernel.Abstractions.Providers;
using AIKernel.Dtos.Core;
using AIKernel.Dtos.Routing;

/// <summary>
/// [EN] Shared capability metadata for AIKernel WASM runtime providers.
/// [JA] AIKernel WASM runtime Provider 向けの共有 capability metadata です。
/// </summary>
public sealed class WasmProviderCapabilities : IProviderCapabilities
{
    private readonly IReadOnlyList<string> _operations;
    private readonly IReadOnlyList<string> _dataTypes;

    /// <summary>
    /// [EN] Initializes WASM provider capability metadata.
    /// [JA] WASM Provider capability metadata を初期化します。
    /// </summary>
    public WasmProviderCapabilities(
        IEnumerable<string> operations,
        IEnumerable<string> dataTypes)
    {
        ArgumentNullException.ThrowIfNull(operations);
        ArgumentNullException.ThrowIfNull(dataTypes);
        _operations = operations.Order(StringComparer.Ordinal).ToArray();
        _dataTypes = dataTypes.Order(StringComparer.Ordinal).ToArray();
    }

    /// <summary>[EN] Supported operations. [JA] 対応 operation です。</summary>
    public IReadOnlyList<string> SupportedOperations => _operations;

    /// <summary>[EN] Supported data types. [JA] 対応 data type です。</summary>
    public IReadOnlyList<string> SupportedDataTypes => _dataTypes;

    /// <summary>[EN] Maximum logical connection count. [JA] 最大 logical connection 数です。</summary>
    public int MaxConcurrentConnections => 1;

    /// <summary>[EN] Optional rate-limit information. [JA] 任意の rate-limit 情報です。</summary>
    public RateLimitInfo? RateLimit => null;

    /// <summary>[EN] Static capacity vector. [JA] 静的 capacity vector です。</summary>
    public ModelCapacityVector Vector => new();

    /// <summary>[EN] Returns no dynamic capacities for deterministic runtime providers. [JA] deterministic runtime Provider では dynamic capacity を返しません。</summary>
    public IDictionary<string, float>? GetDynamicCapacities(IExecutionConstraints constraints) => null;

    /// <summary>[EN] Returns no model capability profile for runtime providers. [JA] runtime Provider では model capability profile を返しません。</summary>
    public ICapabilityProfile? GetCapabilityProfile() => null;

    /// <summary>[EN] Checks operation support. [JA] operation support を確認します。</summary>
    public bool SupportsOperation(string operation)
        => _operations.Contains(operation, StringComparer.OrdinalIgnoreCase);

    /// <summary>[EN] Checks data-type support. [JA] data-type support を確認します。</summary>
    public bool SupportsDataType(string dataType)
        => _dataTypes.Contains(dataType, StringComparer.OrdinalIgnoreCase);

    /// <summary>[EN] Runtime providers do not expose quantization. [JA] runtime Provider は quantization を公開しません。</summary>
    public bool SupportsQuantization(string quantizationLevel) => false;

    /// <summary>[EN] Runtime providers do not augment queries. [JA] runtime Provider は query augmentation を行いません。</summary>
    public bool SupportsQueryAugmentation => false;

    /// <summary>[EN] Runtime providers do not decompose queries. [JA] runtime Provider は query decomposition を行いません。</summary>
    public bool SupportsQueryDecomposition => false;

    /// <summary>[EN] Runtime providers do not route queries. [JA] runtime Provider は query routing を行いません。</summary>
    public bool SupportsQueryRouting => false;

    /// <summary>[EN] Maximum query parts. [JA] 最大 query part 数です。</summary>
    public int MaxQueryParts => 0;

    /// <summary>[EN] Supported query-processing operations. [JA] 対応 query-processing operation です。</summary>
    public IReadOnlyList<string> SupportedQueryProcessingOperations => [];

    /// <summary>[EN] Checks query-processing support. [JA] query-processing support を確認します。</summary>
    public bool SupportsQueryProcessingOperation(string operation) => false;

    /// <summary>[EN] Runtime providers do not expose embeddings. [JA] runtime Provider は embedding を公開しません。</summary>
    public bool SupportsEmbedding => false;

    /// <summary>[EN] Embedding dimensions. [JA] embedding dimension です。</summary>
    public int? EmbeddingDimensions => null;

    /// <summary>[EN] Supported embedding model names. [JA] 対応 embedding model 名です。</summary>
    public IReadOnlyList<string> SupportedEmbeddingModels => [];
}
