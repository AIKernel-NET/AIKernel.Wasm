from aikernel_wasm import (
    WasmAudioProvider,
    WasmEventProvider,
    WasmFileSystemProvider,
    WasmMemoryProvider,
    WasmProcessProvider,
    WasmRuntime,
    WasmRuntimeContext,
    WasmSaveStateProvider,
    WasmScreenshotProvider,
    WasmStdinProvider,
    WasmTimeProvider,
    managed_api_summary,
    managed_type_names,
    WebGpuComputeCapability,
    WebGpuComputeInvoker,
    WebGpuComputeProvider,
    WebGpuNativeBackend,
    WebGpuWasmBackend,
    wasm_provider_contracts,
)


def test_public_import_surface():
    assert WasmRuntime
    assert WasmRuntimeContext
    assert WasmProcessProvider
    assert WasmMemoryProvider
    assert WasmStdinProvider
    assert WasmFileSystemProvider
    assert WasmEventProvider
    assert WasmAudioProvider
    assert WasmScreenshotProvider
    assert WasmSaveStateProvider
    assert WasmTimeProvider
    assert WebGpuComputeCapability
    assert WebGpuComputeProvider
    assert WebGpuComputeInvoker
    assert WebGpuNativeBackend
    assert WebGpuWasmBackend


def test_wasm_provider_contracts_cover_runtime_surface():
    provider_ids = {contract.provider_id for contract in wasm_provider_contracts()}

    assert provider_ids == {
        "wasm.runtime",
        "wasm.process",
        "wasm.memory",
        "wasm.stdin",
        "wasm.filesystem",
        "wasm.event",
        "wasm.audio",
        "wasm.screenshot",
        "wasm.savestate",
        "wasm.time",
    }


def test_webgpu_capability_defaults_match_public_contract():
    capability = WebGpuComputeCapability()

    assert capability.provider_id == "webgpu.compute"
    assert capability.adapter_profile == "wasm-webgpu"


def test_managed_api_catalog_covers_wasm_runtime_and_perception():
    names = set(managed_type_names())
    summary = managed_api_summary()

    assert "AIKernel.Wasm.Runtime.WasmRuntime" in names
    assert "AIKernel.Wasm.Perception.WasmResidentPerceptionAlgorithmLibrary" in names
    assert "AIKernel.Wasm.Spatial.WasmSpatialCognitionProvider" in names
    assert summary["AIKernel.Wasm.Runtime"] > 0
    assert summary["AIKernel.Wasm.Perception"] > 0
