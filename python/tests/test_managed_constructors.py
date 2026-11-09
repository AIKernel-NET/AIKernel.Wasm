from aikernel_wasm import WasmRuntime, WasmRuntimeContext, WebGpuNativeBackend, WebGpuWasmBackend


def test_wasm_runtime_create_uses_managed_defaults():
    runtime = WasmRuntime.create().managed
    try:
        context = runtime.CreateContext(256)
        assert context.MemorySize == 256
        context.WriteByte(0, 37)
        assert context.ReadByte(0) == 37
    finally:
        runtime.Dispose()


def test_wasm_runtime_context_create_uses_default_memory_size():
    context = WasmRuntimeContext.create().managed
    try:
        assert context.MemorySize == 65536
        context.WriteByte(65535, 91)
        assert context.ReadByte(65535) == 91
    finally:
        context.Dispose()


def test_webgpu_wasm_backend_create_uses_default_interop():
    backend = WebGpuWasmBackend.create().managed
    from System.Threading import CancellationToken  # type: ignore[import-not-found]

    backend.InitializeAsync(getattr(CancellationToken, "None")).GetAwaiter().GetResult()
    assert not backend.IsAvailable


def test_webgpu_native_backend_parameterless_create_still_works():
    backend = WebGpuNativeBackend.create().managed
    assert str(backend.GetType().FullName) == "AIKernel.Wasm.Compute.WebGpuNativeBackend"
