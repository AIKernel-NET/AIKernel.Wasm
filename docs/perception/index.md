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
- `WasmSyntheticSensorProvider` builds Phainesis phenomena and Nous meaning
  vectors from resident buffers. Scenario layers should consume these neutral
  signals instead of re-implementing detector logic in JavaScript.

## Local Surface Candidates

- `WebGpuResidentModelProvider`
- `WasmFramePerceptionProvider`
- `WasmAuditoryPerceptionProvider`
- `WasmSpatialCognitionProvider`
- `WasmHudSignalProvider`
- `WasmOverlayAnnotationProvider`
- `WasmResidentPerceptionAlgorithmLibrary`
- `WasmSyntheticSensorProvider`
- `WasmSyntheticSensorKernelPlanner`

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

## Synthetic Sensors

`WasmSyntheticSensorProvider` is the runtime-side bridge from resident
preprocessing to the Sensor OS. It consumes current/previous scalar buffers,
navigable masks, threat masks, and normalized scalar hints, then emits:

- Phainesis phenomena such as `gap`, `corridorFlow`, `wallFlow`, `stuck`,
  `threatField`, and `confidenceFusion`
- Nous vectors such as `gapVector`, `corridorVector`, `wallFlowVector`,
  `stuckVector`, `threatVector`, and `confidenceVector`

The provider also implements `IWasmSyntheticSensorPipeline`. Use
`TryAnalyzeAsync` when composing perception work through `AIKernel.Common`
`Result<T>` / LINQ query syntax. `AnalyzeAsync` remains the DTO-envelope
compatibility method for callers that expect `WasmSyntheticSensorSnapshot`
directly.

The provider is still scenario-neutral. Doom-specific names such as doors,
rooms, weapons, or map landmarks must remain outside this package. Browser or
scenario JavaScript should act as a thin adapter that forwards resident buffers
and consumes the returned neutral phenomena/vectors.

The synthetic sensor snapshot also carries a fused WebGPU kernel descriptor.
Descriptor planning is owned by `IWasmSyntheticSensorKernelPlanner` /
`WasmSyntheticSensorKernelPlanner` so future synthetic sensor providers can use
the same resident buffer layout, binding order, and zero-copy metadata instead
of duplicating WebGPU planning logic.

The shared fused pass shape is:

- `currentFrame`, `previousFrame`, `navigableMask`, `threatMask`, and
  `sensorScalars` are explicit resident inputs with binding indices.
- `phenomena` and `vectors` are explicit storage outputs.
- Workgroups default to `16x16x1`, and metadata marks the pass as
  `fusedPass=true` and `zeroCopyPreferred=true`.

This is intentional. Hosts should keep framebuffer, mask, flow, and sensor
scalar data resident where possible, then dispatch one fused low-layer pass for
Phainesis/Nous extraction instead of bouncing intermediate buffers through
JavaScript or CPU memory.

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
