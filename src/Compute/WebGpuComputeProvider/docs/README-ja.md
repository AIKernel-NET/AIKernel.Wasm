# WebGpuComputeProvider

`WebGpuComputeProvider` は、WebGPU browser/WASM 実行と deterministic CPU
fallback に対応した AIKernel 公式 compute Provider です。

以下を公開します。

- `compute.dispatch`
- `compute.vector_add`

WGSL compute kernel を受け取り、WebGPU 初期化を次の順序で扱います。

1. Adapter
2. Device
3. Queue
4. Compute pipeline
5. Bind group
6. Dispatch

browser または host が WebGPU binding を公開していない場合でも、Provider は
CPU fallback により利用可能です。fallback path は deterministic で、同梱の
vector-add sample kernel をサポートします。

Manifest file: `webgpu.provider.json`

Sample kernel: `samples/vector-add.wgsl`
