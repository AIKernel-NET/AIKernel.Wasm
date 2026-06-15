# WASM Perception と Spatial Cognition

`AIKernel.Wasm.Perception`、`AIKernel.Wasm.Spatial`、`AIKernel.Wasm.Hud`、`AIKernel.Wasm.Models` は、perception と model dispatch のための browser/WASM 所有 execution surface を提供します。

これらのパッケージは v0.1.1.1 では実装側 surface として扱います。次回の正典 Interface 更新で auditory perception、spatial cognition、resident model execution の契約を AIKernel.NET に昇格・統合しやすい形にしています。

## 境界

- WebGPU resident model execution は descriptor-driven であり、model family を hard-code しません。
- Frame perception は scenario 非依存で、`FrameSnapshot` から bounded frame signal を出力します。
- Auditory perception は WebAudio PCM ベースで、backend-specific audio 型を公開しません。
- Auditory perception は left / right energy、stereo balance、dominant frequency、
  neutral event marker を出力します。これらは semantic material であり、
  CTG GateInput には流しません。
- Spatial cognition は scenario semantics なしで visual / auditory perception result
  を合成します。低レイヤ projection は WASM spatial kernel が担当し、
  `atan2(centroid.x - player.x, centroid.y - player.y)`、stereo balance による
  auditory correction、fused direction、normalized HUD coordinate を計算します。
- HUD / overlay Provider は DTO を生成するだけで、rendering は行いません。
- Gate / CTG logic は WASM package の外側に置きます。
- `WasmAudioProvider.UploadGpuBufferAsync` は任意の GPU-resident audio buffer
  carrier を公開します。browser bridge が無い場合は structured failure
  descriptor を返し、runtime buffer path を deterministic に保ちます。

## Local Surface 候補

- `WebGpuResidentModelProvider`
- `WasmFramePerceptionProvider`
- `WasmAuditoryPerceptionProvider`
- `WasmSpatialCognitionProvider`
- `WasmHudSignalProvider`
- `WasmOverlayAnnotationProvider`

これらの名前は実装名であり、v0.1.2 の正典契約名を決定するものではありません。
