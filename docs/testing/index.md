# Testing

[日本語](index-ja.md)

AIKernel.Wasm tests focus on deterministic behavior that can run without a
browser GPU.

## C# Test Suites

```bash
dotnet build AIKernel.Wasm.slnx -c Release
dotnet test AIKernel.Wasm.slnx -c Release --no-build
```

Test projects:

- `tests/AIKernel.Wasm.Tests`
- `tests/WebGpuComputeProvider.Tests`

Covered checks include:

- WASM process start/stop lifecycle
- runtime context state and fail-closed memory operations
- EventBus integration
- WebGPU capability descriptor contract
- vector-add behavior
- CPU fallback behavior

## Python Tests

```bash
py -c "import sys, pytest; sys.path[:0]=['python/src']; raise SystemExit(pytest.main(['python/tests']))"
```

Python tests verify import surface, provider descriptors, and default WebGPU
capability metadata.

## Browser E2E

Real browser WebGPU validation is intentionally outside the default automated
test set. The default tests verify CPU fallback and contract behavior so CI can
run consistently on Windows and Linux.

## Documentation Checks

Documentation should remain bilingual for public package material:

- English: `index.md`, `README.md`
- Japanese: `index-ja.md`, `README-ja.md`

Public C# members are checked through XML documentation warnings during Release
builds.
