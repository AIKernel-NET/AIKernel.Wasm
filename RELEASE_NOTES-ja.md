# AIKernel.Wasm リリースノート

[English](RELEASE_NOTES.md)

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
