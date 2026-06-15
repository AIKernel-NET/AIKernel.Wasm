# Concept Elevation Notes / 概念昇格ノート

[English](concept-elevation.md)

## Canonical Reference / 正典参照

共通の命名規約は AIKernel.NET 側で管理します。

- `AIKernel.NET/docs/canonical-language/index.md`
- `AIKernel.NET/docs/design/concept-elevation-refactoring-design.md`
- `AIKernel.NET/docs/guidelines/concept-elevation-guidelines.md`
- `AIKernel.NET/docs/migration/concept-elevation-v0.1.1.1.md`
- `AIKernel.NET/docs/todo/concept-elevation-refactoring-todo.md`

## WASM Scope / WASM の対象範囲

AIKernel.Wasm は sandboxed runtime、browser / WebAssembly boundary、WebGPU-facing
surface を担当します。concept facade は interop layer より上位の perception、
scene、time、timing 語彙に限定します。

追加された concept surface:

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

browser boundary package:

- `AIKernel.Wasm.Audio`
- `AIKernel.Wasm.Display`
- `AIKernel.Wasm.Input`

## Guardrails / 境界ルール

- `WasmRuntime`、`WasmRuntimeContext`、WebGPU provider、audio provider、JS interop
  class は技術名のまま維持します。
- concept name は DTO、JSInterop、NativeBridge、mapper、adapter、provider 実装名に
  使いません。
- Gate logic は AIKernel.Wasm に置きません。
- `IWebAudioJsInterop` は `AIKernel.Wasm.Audio` の内部境界に閉じます。
- WebGPU code は `AIKernel.Wasm.Compute` を canonical namespace とし、
  `AIKernel.Wasm.Comput` は互換 surface としてだけ維持します。

## Tests / テスト

`tests/AIKernel.Wasm.Tests/ConceptElevationArchitectureTests.cs` は naming boundary、
dependency boundary、WebAudio interop containment、concept helper behavior を検証します。
