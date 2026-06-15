# AIKernel.Wasm リリースノート

[English](RELEASE_NOTES.md)

## 0.1.1.1

**June 2026 - Local development alignment.**

AIKernel.Wasm は AIKernel.Core / AIKernel.Control / AIKernel.Providers
0.1.1.1 と同じ開発方針へ揃えます。

- Wasm package family を `0.1.1.1` に設定しました。
- local development package versioning を `0.1.1.1-dev{build-number}` 形式に
  揃えました。
- AIKernel.NET contract package は `0.1.1.1` を参照します。
- AIKernel.Core は local `0.1.1.1-dev1` package family を参照します。
- AIKernel.Providers は local `0.1.1.1-dev2` package family を参照します。
- audio、display/frame capture、分解済み virtual input 向けに WASM owned の
  browser boundary package を追加しました。
- canonical WebGPU namespace を `AIKernel.Wasm.Compute` に補正しつつ、
  `AIKernel.Wasm.Comput` の互換 wrapper を維持しました。
- Aisthesis、Phantasia、Chronos、Kairos を facade vocabulary に限定する
  concept-elevation architecture guard を追加しました。
- この update line は NuGet-only です。Python wrapper 関連資料は
  reference-only とし、PyPI package として build / install / publish しません。

## 0.1.1

**June 10th, 2026 - Activating the WASM runtime surface.**
**2026年6月10日--WASM ランタイム面を活性化する。**

Activating the WASM runtime surface: memory, FS, events, audio, and WebGPU
converge into a unified execution layer. WASM ランタイム面の活性化--メモリ・FS・
イベント・オーディオ・WebGPU が統一実行層へ収束する。

AIKernel.Wasm 0.1.1 は、AIKernel Semantic OS 向けの browser / WebAssembly
runtime package family を導入します。

- WASM process、memory、stdin、file system、event、audio、screenshot、
  save-state、time provider surface 向けに `AIKernel.Wasm.Runtime` を公開します。
- Core compute abstraction と整合する WebGPU compute boundary として
  `AIKernel.Wasm.WebGpuComputeProvider` を公開します。
- browser / WASM 固有の実装責務を Core や host-side Providers へ戻しません。
- Python host 向けに、同じ public runtime surface を扱う `aikernel-wasm`
  Python wrapper package を提供します。
- 実 browser WebGPU validation は、deterministic automated CPU-fallback test とは
  分離して扱います。

Wasm 0.1.1 は、Core と Providers の dependency boundary を維持しながら
sandboxed runtime layer を活性化します。
