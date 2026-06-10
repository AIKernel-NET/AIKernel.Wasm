# AIKernel.Wasm Release Notes

[日本語](RELEASE_NOTES-ja.md)

## 0.1.1

**June 10th, 2026 - Activating the WASM runtime surface.**
**2026年6月10日--WASM ランタイム面を活性化する。**

Activating the WASM runtime surface: memory, FS, events, audio, and WebGPU
converge into a unified execution layer. WASM ランタイム面の活性化--メモリ・FS・
イベント・オーディオ・WebGPU が統一実行層へ収束する。

AIKernel.Wasm 0.1.1 introduces the browser/WebAssembly runtime package family
for the AIKernel Semantic OS.

- Publish `AIKernel.Wasm.Runtime` for WASM process, memory, stdin, file system,
  event, audio, screenshot, save-state, and time provider surfaces.
- Publish `AIKernel.Wasm.WebGpuComputeProvider` as the WebGPU compute boundary
  aligned with Core compute abstractions.
- Keep browser/WASM implementation concerns outside Core and host-side
  Providers.
- Provide the `aikernel-wasm` Python wrapper package for Python hosts that need
  the same public runtime surface.
- Keep real browser WebGPU validation separate from deterministic automated
  CPU-fallback tests.

Wasm 0.1.1 activates the sandboxed runtime layer while preserving Core and
Providers dependency boundaries.
