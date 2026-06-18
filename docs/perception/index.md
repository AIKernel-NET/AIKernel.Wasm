# WASM Perception and Spatial Cognition

`AIKernel.Wasm.Perception`, `AIKernel.Wasm.Spatial`, `AIKernel.Wasm.Hud`, and `AIKernel.Wasm.Models` provide browser/WASM-owned execution surfaces for perception and model dispatch.

These packages are implementation-side surfaces for the v0.1.2 canonical package line. They preserve thin contracts so future canonical updates can promote or consolidate auditory perception, spatial cognition, and resident model execution contracts in AIKernel.NET.

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
- Spatial cognition also accepts an optional `sensorInputs` map aligned with
  the provider-neutral Sensor OS vocabulary. Runtime and UI carriers use
  ASCII-safe triadic concept names: `Aisthesis`, `Kinesis`, and `Phantasia`.
  `movement` is a derived `Kinesis` sensor and remains outside Gate input.
- Priority sensors may emit a retry intent carrier such as `health-death`.
  This is only a carrier for Control policy input; WASM does not run Gate or CTG
  decision logic.
- HUD and overlay providers generate DTOs only; they do not render.
- Gate and CTG logic remain outside WASM packages.
- `WasmAudioProvider.UploadGpuBufferAsync` exposes an optional GPU-resident
  audio-buffer carrier. When a browser bridge is absent it returns a structured
  failure descriptor and keeps the runtime buffer path deterministic.
- `WasmResidentPerceptionAlgorithmLibrary` exposes WebGPU-ready dispatch
  descriptors plus CPU fallbacks for resident perception algorithms. It does not
  depend on `AIKernel.Providers.Perception` at compile time and does not expose
  JSInterop names in public API.

## Local Surface Candidates

- `WebGpuResidentModelProvider`
- `WasmFramePerceptionProvider`
- `WasmAuditoryPerceptionProvider`
- `WasmSpatialCognitionProvider`
- `WasmHudSignalProvider`
- `WasmOverlayAnnotationProvider`
- `WasmResidentPerceptionAlgorithmLibrary`

## Resident Perception Algorithms

The resident algorithm library is the WASM-side counterpart to the pure managed
Providers substrate. It keeps the same Aisthesis to Phantasia meaning, but adds
`WasmResidentKernelDescriptor` carriers so browser hosts can dispatch equivalent
WebGPU kernels while CI or non-browser hosts use deterministic CPU fallback.

Implemented surfaces:

- RGB to HSV threshold mask
- Max-pooling downsample
- Morphology dilation / erosion
- Dense optical flow
- Audio spectrum / FFT-style carrier

These algorithms are preprocessing functions only. They do not run Gate, CTG,
Council, intent, action, or scenario-specific detection.

## Sensor OS Alignment

`AIKernel.Providers.Perception` owns the provider-neutral abstraction surface.
WASM keeps a local mirror shape so browser execution can remain independent of
provider substrate packages while still staying ready for the next canonical
interface extraction.

Current concept mapping:

| Concept | Sensor |
| --- | --- |
| `Aisthesis` | `visual`, `audio`, `health` |
| `Kinesis` | `motor`, `movement` |
| `Phantasia` | `compass`, `spatial` |

These names follow the triadic philosophical naming model prepared for the next
canonical interface extraction.

For ownership and promotion rules across Providers, Wasm, Control, and Doom,
see the
[Cross-Repository Developer Guide v0.1.1.1](https://github.com/AIKernel-NET/AIKernel.NET/blob/main/docs/development/cross-repository-developer-guide-v0.1.1.1.md).
