# WASM Perception and Spatial Cognition

`AIKernel.Wasm.Perception`, `AIKernel.Wasm.Spatial`, `AIKernel.Wasm.Hud`, and `AIKernel.Wasm.Models` provide browser/WASM-owned execution surfaces for perception and model dispatch.

These packages are intentionally implementation-side surfaces for v0.1.1.1. They are shaped so the next canonical interface update can promote or consolidate auditory perception, spatial cognition, and resident model execution contracts in AIKernel.NET.

## Boundaries

- WebGPU resident model execution is descriptor-driven and does not hard-code a model family.
- Frame perception is scenario-independent and emits bounded frame signals from `FrameSnapshot`.
- Auditory perception is WebAudio PCM based and does not expose backend-specific audio types.
- Auditory perception emits left/right energy, stereo balance, dominant frequency,
  and neutral event markers. These values are semantic material only and do not
  become CTG GateInput.
- Spatial cognition composes visual and auditory perception results without
  scenario semantics. The WASM spatial kernel owns the low-level projection:
  `atan2(centroid.x - player.x, centroid.y - player.y)`, stereo balance based
  correction, fused direction, and normalized HUD coordinates.
- HUD and overlay providers generate DTOs only; they do not render.
- Gate and CTG logic remain outside WASM packages.
- `WasmAudioProvider.UploadGpuBufferAsync` exposes an optional GPU-resident
  audio-buffer carrier. When a browser bridge is absent it returns a structured
  failure descriptor and keeps the runtime buffer path deterministic.

## Local Surface Candidates

- `WebGpuResidentModelProvider`
- `WasmFramePerceptionProvider`
- `WasmAuditoryPerceptionProvider`
- `WasmSpatialCognitionProvider`
- `WasmHudSignalProvider`
- `WasmOverlayAnnotationProvider`

These names are implementation names, not canonical v0.1.2 contract decisions.
