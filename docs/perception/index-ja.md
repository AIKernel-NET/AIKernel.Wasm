# WASM Perception と Spatial Cognition

`AIKernel.Wasm.Perception`、`AIKernel.Wasm.Spatial`、`AIKernel.Wasm.Hud`、`AIKernel.Wasm.Models` は、perception と model dispatch のための browser/WASM 所有 execution surface を提供します。

これらのパッケージは v0.1.3 正典 package line の実装側 surface です。将来の正典更新で auditory perception、spatial cognition、resident model execution の契約を AIKernel.NET に昇格・統合しやすい形を維持します。

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
- Spatial cognition は任意の `sensorInputs` map も受け取ります。この map は
  provider-neutral な Sensor OS vocabulary と整合し、runtime / UI carrier では
  ASCII-safe な三形体 concept name として `Aisthesis`、`Kinesis`、`Phantasia` を使用します。
  `movement` は derived `Kinesis` sensor であり、Gate input には流しません。
- 優先 sensor は `health-death` などの retry intent carrier を出力できます。
  これは Control policy input 用の carrier であり、WASM は Gate / CTG decision
  logic を実行しません。
- HUD / overlay Provider は DTO を生成するだけで、rendering は行いません。
- Gate / CTG logic は WASM package の外側に置きます。
- `WasmAudioProvider.UploadGpuBufferAsync` は任意の GPU-resident audio buffer
  carrier を公開します。browser bridge が無い場合は structured failure
  descriptor を返し、runtime buffer path を deterministic に保ちます。
- `WasmResidentPerceptionAlgorithmLibrary` は resident perception algorithm 向けに
  WebGPU-ready な dispatch descriptor と CPU fallback を公開します。compile-time では
  `AIKernel.Providers.Perception` に依存せず、public API に JSInterop 名を出しません。
- `WasmSyntheticSensorProvider` は resident buffer から Phainesis の現象と
  Nous の意味ベクトルを構築します。scenario 層は JavaScript で detector logic を
  再実装せず、この中立 signal を消費します。

## Local Surface 候補

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

Resident algorithm library は pure managed な Providers substrate に対応する
WASM 側 surface です。Aisthesis から Phantasia への意味論は同じまま、
`WasmResidentKernelDescriptor` carrier を追加し、browser host は等価な WebGPU
kernel を dispatch でき、CI や非 browser host は deterministic な CPU fallback を使えます。

実装済み surface:

- RGB to HSV threshold mask
- Max-pooling downsample
- Morphology dilation / erosion
- Dense optical flow
- Audio spectrum / FFT-style carrier

これらは preprocessing function のみです。Gate、CTG、Council、intent、action、
scenario-specific detection は実行しません。

## Synthetic Sensors

`WasmSyntheticSensorProvider` は、resident preprocessing から Sensor OS へつなぐ
runtime 側 bridge です。current / previous scalar buffer、navigable mask、
threat mask、正規化済み scalar hint を受け取り、以下を出力します。

- `gap`、`corridorFlow`、`wallFlow`、`stuck`、`threatField`、
  `confidenceFusion` などの Phainesis 現象
- `gapVector`、`corridorVector`、`wallFlowVector`、`stuckVector`、
  `threatVector`、`confidenceVector` などの Nous ベクトル

この provider は `IWasmSyntheticSensorPipeline` も実装します。
`AIKernel.Common` の `Result<T>` / LINQ query syntax で perception work を合成する
場合は `TryAnalyzeAsync` を使用します。`AnalyzeAsync` は
`WasmSyntheticSensorSnapshot` を直接期待する呼び出し元向けの DTO-envelope
互換 method として残します。

この provider は scenario-neutral です。door、room、weapon、map landmark などの
Doom 固有名はこの package に入れません。browser / scenario JavaScript は resident
buffer を渡し、返却された中立の phenomenon / vector を消費する薄い adapter に留めます。

Synthetic sensor snapshot は fused WebGPU kernel descriptor も保持します。
Descriptor planning は `IWasmSyntheticSensorKernelPlanner` /
`WasmSyntheticSensorKernelPlanner` が所有します。これにより、今後の
synthetic sensor provider は WebGPU の resident buffer layout、binding 順序、
zero-copy metadata を重複実装せず、同じ扱いで再利用できます。

- `currentFrame`、`previousFrame`、`navigableMask`、`threatMask`、
  `sensorScalars` は binding index を持つ明示的な resident input です。
- `phenomena` と `vectors` は明示的な storage output です。
- workgroup は既定で `16x16x1` とし、metadata で `fusedPass=true` と
  `zeroCopyPreferred=true` を示します。

これは意図的な設計です。host は framebuffer、mask、flow、sensor scalar を可能な限り
resident に保ち、JavaScript や CPU memory へ intermediate buffer を戻さず、
Phainesis / Nous 抽出を 1 つの低レイヤ fused pass として dispatch する方針です。

## Sensor OS 整合

`AIKernel.Providers.Perception` が provider-neutral な抽象 surface を所有します。
WASM は provider substrate package へ直接依存せずに browser execution の境界を保つため、
同形の local mirror carrier を持ちます。これにより、次回の正典 Interface 抽出に
乗せやすい状態を維持します。

現在の concept mapping:

| Concept | Sensor |
| --- | --- |
| `Aisthesis` | `visual`, `audio`, `health` |
| `Kinesis` | `motor`, `movement` |
| `Phantasia` | `compass`, `spatial` |

これらの名前は、次回の正典 Interface 抽出に向けた三形体の哲学的命名モデルに従います。

Providers、Wasm、Control、Doom をまたぐ ownership と昇格ルールは
[リポジトリ横断開発者ガイド v0.1.1.1](https://github.com/AIKernel-NET/AIKernel.NET/blob/main/docs/development/cross-repository-developer-guide-v0.1.1.1-ja.md)
を参照してください。
