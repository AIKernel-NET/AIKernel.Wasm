using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using AIKernel.Wasm.Compute;

[assembly: SupportedOSPlatform("browser")]

await WeightMemorySmoke.RunAsync();

internal static partial class WeightMemorySmoke
{
    internal static async Task RunAsync()
    {
        const int pageSize = 256;
        const int byteLength = 2 * pageSize + 13;
        using var memory = await BrowserWeightMemory.OpenAsync(ModuleUrl(), FileName(), pageSize, ramPages: 1);
        using var device = Device();
        using var buffer = Buffer();
        if (memory.ByteLength != byteLength || memory.PageCount != 3)
            throw new InvalidOperationException("The partial OPFS page was not included in the address space.");

        // Reuse page zero before evicting it, then use it again after the eviction.
        foreach (var index in new[] { 0, 0, 1, 2, 0 })
        {
            memory.UploadPage(index, device, buffer, pageSize);
            await CheckGpuPage(index);
        }

        byte[] host = new byte[pageSize];
        memory.ReadPage(0, host);
        for (int index = 0; index < pageSize; index++)
        {
            if (host[index] != (byte)(index * 17 + 3))
                throw new InvalidOperationException($"Host cache byte {index} differs from the fixture.");
        }

        var stats = memory.GetStatistics();
        var ram = memory.GetRamUsage();
        if (stats.RamBlocks != 1 || stats.BackingReads != 4 || stats.SwapReads != 0 || stats.SwapBlocks != 2 ||
            ram.ResidentBytes != pageSize || ram.PeakAllocatedBytes != 2 * pageSize)
            throw new InvalidOperationException($"Unexpected cache counters. {stats} {ram}");
        if (Directory.EnumerateFiles("/", ".aikernel-swap-*").Any())
            throw new IOException("An immutable OPFS page created a swap file.");
        Report((int)stats.BackingReads, stats.RamBlocks, stats.SwapBlocks, (int)ram.PeakAllocatedBytes);
    }

    [JSImport("moduleUrl", "smoke")]
    private static partial string ModuleUrl();

    [JSImport("fileName", "smoke")]
    private static partial string FileName();

    [JSImport("device", "smoke")]
    private static partial JSObject Device();

    [JSImport("buffer", "smoke")]
    private static partial JSObject Buffer();

    [JSImport("checkGpuPage", "smoke")]
    private static partial Task CheckGpuPage(int index);

    [JSImport("report", "smoke")]
    private static partial void Report(int backingReads, int residentRamPages, int nonresidentPages, int peakRamPageBytes);
}
