# AIKernel.Wasm Release Notes

[日本語](RELEASE_NOTES-ja.md)

## 0.1.3 (Unreleased)

AIKernel.Wasm 0.1.3 is the canonical GPU integration release line for the
browser/WASM runtime family. It aligns the WebGPU provider with canonical GPU HUD,
GPU Aisthesis, Spatial Reasoning, zero-copy diagnostics, and deterministic CPU
fallback metadata.

- Package the canonical browser dispatch bridge at
  `runtime/browser/webgpu-envelope-bridge.js`.
- Package GPU resident WGSL assets for HUD composite, Aisthesis raw-frame
  feature masks, and Spatial Reasoning.
- Package canonical GPU layout metadata at `buffers/layouts/gpu-layouts.json`.
- Add `scripts/verify-webgpu-package.ps1` to smoke-test the NuGet package asset
  surface before release.
- Local `0.1.3-dev3` validation passed 64 Release tests and verified three
  packaged WebGPU passes in Chrome on an NVIDIA adapter.
- Python `WasmRuntime.create()`, `WasmRuntimeContext.create()`, and
  `WebGpuWasmBackend.create()` use the managed constructor defaults.
  The development bundle contains the `0.1.3-dev3` AIKernel DLLs and the
  resolved dependency injection DLL (10.0.8).
- Installed the local `0.1.3.dev3` wheel in a fresh virtual environment:
  all nine DLLs loaded from the wheel and all eight Python tests passed.
- Before publishing the stable `0.1.3` wheel, replace the development DLLs
  with the stable package graph and repeat the installed-wheel verification.

## 0.1.2.1

**June 19th, 2026 - WASM perception patch.**

AIKernel.Wasm 0.1.2.1 is a NuGet-only patch release for the AIKernel.Wasm
0.1.2 runtime family. It keeps AIKernel.NET, AIKernel.Core, and
AIKernel.Providers dependencies on the published 0.1.2 packages.

- Add synthetic sensor pipeline abstractions for Phainesis phenomena and Nous
  meaning vectors.
- Add `AIKernel.Common` Result / Option / LINQ pipeline support to
  `AIKernel.Wasm.Perception`.
- Extract reusable WebGPU resident kernel planning through
  `IWasmSyntheticSensorKernelPlanner`.
- Align WASM provider manifests with the 0.1.2.1 NuGet patch version.

## 0.1.2

**June 16th, 2026 - Browser runtime package line.**

AIKernel.Wasm 0.1.2 aligns the browser and WebAssembly runtime packages with the AIKernel 0.1.2 dependency chain.

- Publish runtime, audio, display, input, HUD, perception, spatial, model, and WebGPU compute provider surfaces.
- Keep Wasm execution and WebGPU/WebAudio concerns isolated from Providers and Control.
- Align documentation with the synchronized NuGet and Python wrapper release flow.
- Preserve thin surfaces for future canonical interface promotion.
