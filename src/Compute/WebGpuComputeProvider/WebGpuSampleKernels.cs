namespace AIKernel.Wasm.Comput;

/// <summary>
/// [EN] Built-in sample WGSL kernels for WebGPU compute tests and examples.
/// [JA] WebGPU compute の test と example 用の組み込み sample WGSL kernel です。
/// </summary>
public static class WebGpuSampleKernels
{
    /// <summary>
    /// [EN] WGSL vector-add kernel using three storage buffers.
    /// [JA] 3 つの storage buffer を使用する WGSL vector-add kernel です。
    /// </summary>
    public const string VectorAdd =
        """
        @group(0) @binding(0) var<storage, read> a: array<f32>;
        @group(0) @binding(1) var<storage, read> b: array<f32>;
        @group(0) @binding(2) var<storage, read_write> out: array<f32>;

        @compute @workgroup_size(64)
        fn main(@builtin(global_invocation_id) gid: vec3<u32>) {
            let i = gid.x;
            out[i] = a[i] + b[i];
        }
        """;
}
