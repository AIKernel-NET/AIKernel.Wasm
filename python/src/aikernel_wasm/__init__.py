"""[EN]
Python wrapper for AIKernel.Wasm runtime and WebGPU providers.

[JA]
AIKernel.Wasm runtime と WebGPU Provider の Python wrapper です。
"""

from .native import load_wasm_runtime, require_wasm_assemblies, wasm_assemblies
from .runtime import (
    WasmAudioProvider,
    WasmEventProvider,
    WasmFileSystemProvider,
    WasmMemoryProvider,
    WasmProcessProvider,
    WasmProviderContract,
    WasmRuntime,
    WasmRuntimeContext,
    WasmSaveStateProvider,
    WasmScreenshotProvider,
    WasmStdinProvider,
    WasmTimeProvider,
    wasm_provider_contracts,
)
from .webgpu import (
    CapabilityContract,
    WebGpuComputeCapability,
    WebGpuComputeInvoker,
    WebGpuComputeProvider,
    WebGpuNativeBackend,
    WebGpuWasmBackend,
)

__all__ = [
    "CapabilityContract",
    "WasmAudioProvider",
    "WasmEventProvider",
    "WasmFileSystemProvider",
    "WasmMemoryProvider",
    "WasmProcessProvider",
    "WasmProviderContract",
    "WasmRuntime",
    "WasmRuntimeContext",
    "WasmSaveStateProvider",
    "WasmScreenshotProvider",
    "WasmStdinProvider",
    "WasmTimeProvider",
    "WebGpuComputeCapability",
    "WebGpuComputeInvoker",
    "WebGpuComputeProvider",
    "WebGpuNativeBackend",
    "WebGpuWasmBackend",
    "load_wasm_runtime",
    "require_wasm_assemblies",
    "wasm_assemblies",
    "wasm_provider_contracts",
]
