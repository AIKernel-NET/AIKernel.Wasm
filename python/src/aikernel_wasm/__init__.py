"""[EN]
Python wrapper for AIKernel.Wasm runtime and WebGPU providers.

[JA]
AIKernel.Wasm runtime と WebGPU Provider の Python wrapper です。
"""

from .api_catalog import (
    ManagedMemberDescriptor,
    ManagedTypeDescriptor,
    find_managed_type,
    managed_api_catalog,
    managed_api_summary,
    managed_type_names,
)
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
    "ManagedMemberDescriptor",
    "ManagedTypeDescriptor",
    "find_managed_type",
    "managed_api_catalog",
    "managed_api_summary",
    "managed_type_names",
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
