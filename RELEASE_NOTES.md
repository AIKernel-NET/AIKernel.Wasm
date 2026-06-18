# AIKernel.Wasm Release Notes

[日本語](RELEASE_NOTES-ja.md)

## 0.1.2

**June 16th, 2026 - Browser runtime package line.**

AIKernel.Wasm 0.1.2 aligns the browser and WebAssembly runtime packages with the AIKernel 0.1.2 dependency chain.

- Publish runtime, audio, display, input, HUD, perception, spatial, model, and WebGPU compute provider surfaces.
- Keep Wasm execution and WebGPU/WebAudio concerns isolated from Providers and Control.
- Align documentation with the synchronized NuGet and Python wrapper release flow.
- Preserve thin surfaces for future canonical interface promotion.