# AIKernel.Wasm Release Notes

[日本語](RELEASE_NOTES-ja.md)

## 0.1.1.1

**June 2026 - Local development alignment.**

AIKernel.Wasm now follows the AIKernel.Core, AIKernel.Control, and
AIKernel.Providers 0.1.1.1 development policy.

- Set the Wasm package family to `0.1.1.1`.
- Add local development package versioning through
  `0.1.1.1-dev{build-number}`.
- Resolve AIKernel.NET contract packages from `0.1.1.1`.
- Resolve AIKernel.Core from the local `0.1.1.1-dev1` package family.
- Resolve AIKernel.Providers from the local `0.1.1.1-dev2` package family.
- Add WASM-owned browser boundary packages for audio, display/frame capture,
  and decomposed virtual input.
- Correct the canonical WebGPU namespace to `AIKernel.Wasm.Compute` while
  keeping `AIKernel.Wasm.Comput` compatibility wrappers.
- Add concept-elevation architecture guards that keep Aisthesis, Phantasia,
  Chronos, and Kairos as facade vocabulary only.
- Keep this update line NuGet-only. Python wrapper materials remain
  reference-only and are not built, installed, or published as PyPI packages.

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
