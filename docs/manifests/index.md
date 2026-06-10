# Manifests and Metadata

[日本語](index-ja.md)

AIKernel.Wasm uses provider manifests and deterministic descriptor metadata so
CLI, hosts, and Python wrappers can understand package surfaces without loading
browser implementation objects.

## Runtime Manifest

`src/Runtime/AIKernel.Wasm.Runtime/wasm.provider.json` describes the runtime
provider family.

Expected metadata includes:

- provider ID
- package version
- managed assembly name
- runtime capability names
- provider and invoker type names where applicable

## WebGPU Manifest

`src/Compute/WebGpuComputeProvider/webgpu.provider.json` describes the WebGPU
compute provider.

The manifest identifies:

- provider ID: `webgpu.compute`
- package: `AIKernel.Wasm.WebGpuComputeProvider`
- operations: `compute.dispatch`, `compute.vector_add`
- managed provider and invoker types
- backend metadata such as adapter profile

## Capability Descriptor Metadata

Capability descriptors should remain deterministic. Recommended keys:

- `provider`
- `adapter_profile`
- `backend`
- `fallback`
- `entry_point`
- `operations`

Metadata must not contain secrets, API keys, browser object references, or
host-specific absolute paths.

## CLI Compatibility

WASM provider manifests are intended for host and CLI discovery, but actual
browser WebGPU calls remain behind runtime-specific bindings. CLI checks should
prefer descriptor validation and CPU fallback behavior unless a browser E2E
environment is explicitly supplied.
