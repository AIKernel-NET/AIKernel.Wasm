# Testing

[English](index.md)

AIKernel.Wasm の test は、browser GPU が無くても実行できる deterministic behavior
を中心にします。

## C# Test Suites

```bash
dotnet build AIKernel.Wasm.slnx -c Release
dotnet test AIKernel.Wasm.slnx -c Release --no-build
```

Test project:

- `tests/AIKernel.Wasm.Tests`
- `tests/WebGpuComputeProvider.Tests`

確認範囲:

- WASM process start/stop lifecycle
- runtime context state と fail-closed memory operation
- EventBus integration
- WebGPU capability descriptor contract
- vector-add behavior
- CPU fallback behavior

## Python Tests

```bash
py -c "import sys, pytest; sys.path[:0]=['python/src']; raise SystemExit(pytest.main(['python/tests']))"
```

Python test は import surface、provider descriptor、default WebGPU capability
metadata を確認します。

## Browser E2E

実 browser WebGPU validation は default automated test set の外に置きます。
default test は CPU fallback と contract behavior を検証し、Windows / Linux の CI
で安定して実行できるようにします。

## Documentation Checks

公開 package material は bilingual に保ちます。

- English: `index.md`, `README.md`
- Japanese: `index-ja.md`, `README-ja.md`

Public C# member は Release build 時の XML documentation warning で確認します。
