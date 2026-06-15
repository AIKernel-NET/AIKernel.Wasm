# Concept Elevation Notes / 概念昇格ノート

## Canonical Reference / 正典参照

Common naming rules are maintained in AIKernel.NET:

- `AIKernel.NET/docs/canonical-language/index.md`
- `AIKernel.NET/docs/design/concept-elevation-refactoring-design.md`
- `AIKernel.NET/docs/guidelines/concept-elevation-guidelines.md`
- `AIKernel.NET/docs/migration/concept-elevation-v0.1.1.1.md`
- `AIKernel.NET/docs/todo/concept-elevation-refactoring-todo.md`

## WASM Scope / WASM の対象範囲

AIKernel.Wasm owns sandboxed runtime, browser/WebAssembly boundaries, and
WebGPU-facing surfaces. Concept facades are limited to perception, scene, and
time vocabulary above the interop layer.

AIKernel.Wasm は sandboxed runtime、browser/WebAssembly boundary、WebGPU-facing
surface を担当します。concept facade は interop layer より上位の perception /
scene / time 語彙に限定します。

Added concept surfaces:

- `AIKernel.Wasm.Runtime.Concepts.AisthesisFrameSource`
- `AIKernel.Wasm.Runtime.Concepts.AisthesisObservationSurface`
- `AIKernel.Wasm.Runtime.Concepts.PhantasiaSceneSurface`
- `AIKernel.Wasm.Runtime.Concepts.PhantasiaFrameModel`
- `AIKernel.Wasm.Runtime.Concepts.ChronosWindow`
- `AIKernel.Wasm.Runtime.Concepts.ChronosBuffer`
- `AIKernel.Wasm.Runtime.Concepts.ChronosReplayWindow`
- `AIKernel.Wasm.Runtime.Concepts.ChronosAudioWindow`
- `AIKernel.Wasm.Runtime.Concepts.ChronosAudioBuffer`
- `AIKernel.Wasm.Runtime.Concepts.ChronosPlaybackTimeline`
- `AIKernel.Wasm.Runtime.Concepts.KairosTrigger`
- `AIKernel.Wasm.Runtime.Concepts.KairosFrameSignal`
- `AIKernel.Wasm.Runtime.Concepts.KairosActionTiming`
- `AIKernel.Wasm.Runtime.Concepts.KairosInputTrigger`

Browser boundary packages:

- `AIKernel.Wasm.Audio`
- `AIKernel.Wasm.Display`
- `AIKernel.Wasm.Input`

## Guardrails / 境界ルール

- `WasmRuntime`, `WasmRuntimeContext`, WebGPU providers, audio providers, and
  JS interop classes keep technical names.
- Concept names do not appear on DTO, JSInterop, NativeBridge, mapper, adapter,
  or provider implementation names.
- Gate logic remains outside AIKernel.Wasm.
- `IWebAudioJsInterop` stays inside `AIKernel.Wasm.Audio`.
- WebGPU code uses `AIKernel.Wasm.Compute` as canonical namespace and preserves
  `AIKernel.Wasm.Comput` only as compatibility surface.

`WasmRuntime` / `WasmRuntimeContext` / WebGPU provider / audio provider /
JS interop class は通常の技術名を維持します。Gate logic は AIKernel.Wasm に置きません。

## Tests / テスト

`tests/AIKernel.Wasm.Tests/ConceptElevationArchitectureTests.cs` guards the
naming boundary and verifies deterministic concept helper behavior.
