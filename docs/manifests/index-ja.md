# Manifests and Metadata

[English](index.md)

AIKernel.Wasm は provider manifest と deterministic descriptor metadata を使い、
CLI、host、Python wrapper が browser 実装 object を load せず package surface を
理解できるようにします。

## Runtime Manifest

`src/Runtime/AIKernel.Wasm.Runtime/wasm.provider.json` は runtime Provider family を
記述します。

期待される metadata:

- provider ID
- package version
- managed assembly name
- runtime capability name
- 必要に応じた provider / invoker type name

## WebGPU Manifest

`src/Compute/WebGpuComputeProvider/webgpu.provider.json` は WebGPU compute Provider
を記述します。

manifest が識別する内容:

- provider ID: `webgpu.compute`
- package: `AIKernel.Wasm.WebGpuComputeProvider`
- operations: `compute.dispatch`, `compute.vector_add`
- managed provider / invoker type
- adapter profile など backend metadata

## Capability Descriptor Metadata

Capability descriptor は deterministic に保ちます。推奨 key:

- `provider`
- `adapter_profile`
- `backend`
- `fallback`
- `entry_point`
- `operations`

metadata には secret、API key、browser object reference、host 固有 absolute path を
含めません。

## CLI Compatibility

WASM provider manifest は host / CLI discovery を想定しますが、実 browser WebGPU
call は runtime 固有 binding の背後に残します。CLI check は、browser E2E 環境が
明示されない限り、descriptor validation と CPU fallback behavior を優先します。
