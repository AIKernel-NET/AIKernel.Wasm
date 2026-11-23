using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using AIKernel.Kernel.Memory;

namespace AIKernel.Wasm.Compute;

/// <summary>Connects read-only OPFS model pages to AIKernel's bounded RAM cache in a browser worker.</summary>
[SupportedOSPlatform("browser")]
public sealed partial class BrowserWeightMemory : IDisposable
{
    private const string Module = "aikernel-opfs-weight-memory";
    private readonly VirtualMemorySpace space;
    private readonly JSObject file;
    private bool disposed;

    private BrowserWeightMemory(JSObject file, long byteLength, int pageSize, int ramPages)
    {
        this.file = file;
        ByteLength = byteLength;
        PageSize = pageSize;
        PageCount = (byteLength - 1) / pageSize + 1;
        space = new VirtualMemorySpace(PageCount, PageSize, ramPages, "/", 0, null, new OpfsSource(file, pageSize));
    }

    /// <summary>Length of the immutable backing file in bytes.</summary>
    public long ByteLength { get; }

    /// <summary>Size of each cache page, including any zero padding on the final page.</summary>
    public int PageSize { get; }

    /// <summary>Number of pages in the backing file.</summary>
    public long PageCount { get; }

    /// <summary>Opens a file at the origin's OPFS root using a dedicated worker and a trusted JavaScript module URL.</summary>
    /// <param name="moduleUrl">URL of the packaged opfs-weight-memory.mjs asset.</param>
    /// <param name="fileName">Name of the already populated and verified weight file.</param>
    /// <param name="pageSize">Page size in bytes, a positive multiple of four.</param>
    /// <param name="ramPages">Maximum number of resident RAM pages.</param>
    public static async Task<BrowserWeightMemory> OpenAsync(string moduleUrl, string fileName, int pageSize, int ramPages)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ramPages);
        if (pageSize % sizeof(uint) != 0)
            throw new ArgumentException("The page size must be a multiple of four bytes.", nameof(pageSize));

        await JSHost.ImportAsync(Module, moduleUrl);
        var file = await OpenFile(fileName, pageSize);
        try
        {
            return new BrowserWeightMemory(file, (long)GetFileSize(file), pageSize, ramPages);
        }
        catch
        {
            CloseFile(file);
            throw;
        }
    }

    /// <summary>Copies a page to a page-sized host buffer.</summary>
    public void ReadPage(long pageIndex, Span<byte> destination) => space.ReadBlock(pageIndex, destination);

    /// <summary>Enqueues a page upload to a caller-owned COPY_DST buffer on the same WebGPU device.</summary>
    /// <remarks>The caller controls buffer lifetime, submission order and WebGPU error scopes.</remarks>
    public void UploadPage(long pageIndex, JSObject device, JSObject buffer, long bufferOffset = 0)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfNegative(bufferOffset);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bufferOffset, 9_007_199_254_740_991L);
        if (bufferOffset % sizeof(uint) != 0)
            throw new ArgumentException("The buffer offset must be aligned to four bytes.", nameof(bufferOffset));

        space.UseComputeBlock(pageIndex, (bytes, _) =>
        {
            // The imported method copies synchronously and only reads the borrowed page.
            var borrowed = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(bytes), bytes.Length);
            WritePage(file, device, buffer, bufferOffset, borrowed);
        });
    }

    /// <summary>Returns cache residency and OPFS page reads from the shared virtual-memory layer.</summary>
    /// <remarks>SwapBlocks includes nonresident OPFS-backed pages. This read-only cache does not create a swap file.</remarks>
    public VirtualMemoryStatistics GetStatistics() => space.GetStatistics();

    /// <summary>Returns the shared virtual-memory layer's current and peak RAM allocation.</summary>
    public VirtualMemoryRamUsage GetRamUsage() => space.GetRamUsage();

    /// <inheritdoc />
    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        try { space.Dispose(); }
        finally { CloseFile(file); }
    }

    private static void CloseFile(JSObject file)
    {
        try { Close(file); }
        finally { file.Dispose(); }
    }

    private sealed class OpfsSource(JSObject file, int pageSize) : IVirtualMemorySource
    {
        public void ReadBlock(long blockIndex, Span<byte> destination) =>
            ReadFile(file, checked(blockIndex * pageSize), destination);
    }

    [JSImport("openWeightFile", Module)]
    [return: JSMarshalAs<JSType.Promise<JSType.Object>>]
    private static partial Task<JSObject> OpenFile(string fileName, int pageSize);

    [JSImport("weightFileSize", Module)]
    private static partial double GetFileSize(JSObject file);

    [JSImport("readWeightPage", Module)]
    private static partial void ReadFile(JSObject file, [JSMarshalAs<JSType.Number>] long offset,
        [JSMarshalAs<JSType.MemoryView>] Span<byte> destination);

    [JSImport("writeGpuPage", Module)]
    private static partial void WritePage(JSObject file, JSObject device, JSObject buffer,
        [JSMarshalAs<JSType.Number>] long offset, [JSMarshalAs<JSType.MemoryView>] Span<byte> source);

    [JSImport("closeWeightFile", Module)]
    private static partial void Close(JSObject file);
}
