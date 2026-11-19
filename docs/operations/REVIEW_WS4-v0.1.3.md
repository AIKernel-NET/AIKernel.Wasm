# WS4: WebGPUのバッファ容量・ディスパッチ上限の事前検証

検証日: 2026-11-13。Wasmのdev `2c6b11f`起点の`review/WS4`です。
他のレビュー待ちブランチは取り込んでいません。

## 再現と変更

ブラウザー用envelope executorは、バッファ確保とdispatchWorkgroupsの前に
adapter/deviceのlimitsを確認していませんでした。
元のdev実装に最終版の追加49ケースを適用すると41件失敗、8件成功でした。

標準のAisthesis / SpatialReasoning / HudCompositeの3パスについて、
シェーダー作成・GPUバッファ確保・コマンド記録の前に必要量を検証します。

- `maxBufferSize`: 入力、出力、readbackのバッファ容量
- `maxStorageBufferBindingSize`: storageとして全体をバインドするバッファ容量
- `maxUniformBufferBindingSize`: HUDのuniformバッファ容量
- `maxComputeWorkgroupsPerDimension`: X/Y/Zのワークグループ数
- envelope内のValuesの長さ、Stride×Count、フレーム寸法とバイト数の安全な整数計算

必ずdevice.limitsを参照し、adapterも保持している場合は両者の小さい方を使用します。
limitsが欠落・不正なら既定値で許可せず拒否します。requestDeviceの要求上限を自動で引き上げません。
数値のNaN/Infinity、負数、小数、安全な整数範囲を超える乗算も拒否します。
正規化処理で不正な寸法・件数を0に置き換えてしまう迂回も防ぎます。

既存のバッファ作成・readbackヘルパーと、実際の標準dispatch呼出しでも再検査します。
限界値ちょうどは許可し、空のバッファの4バイト最小確保も維持します。

拒否はRangeErrorとして伝播し、envelope bridgeが成功のフォールバック結果を返す経路へは進めません。
executorからの拒否理由は既存診断のLastFallbackReasonにも残ります。

- `webgpu-limit-exceeded:<limit-name>`
- `webgpu-limit-unavailable:<limit-name>`
- `webgpu-invalid-size`
- `webgpu-invalid-dispatch`

根拠: [W3C WebGPU仕様](https://www.w3.org/TR/webgpu/)のSupported Limits、GPUDevice.createBuffer、
GPUBufferBinding、GPUComputePassEncoder.dispatchWorkgroupsの検証規則。
deviceの上限を使用する必要があるため、adapterの上限だけで判定しません。

## 検証

新規`tests/js/webgpu-limits.test.mjs`には49ケースがあります。
境界値、上限超過、上限情報の欠落・不正、adapter/device差、配列長・宣言長・乗算の桁あふれ、
TypedArray変換前の拒否、注入パスでのヘルパー呼出し、外側のbridge経由、動的なdevice取得を確認します。
事前検証で拒否するケースでは、GPUバッファ確保・シェーダー・パイプライン・テクスチャ作成・
コマンド記録・dispatch・submit・注入パス呼出しの回数が0であることも検証します。

既存のVMテストには実GPUDeviceに必須のlimitsをモックへ補いました。
既存の期待値とassertionは変更していません。C#の既存テストも変更していません。

JavaScriptは追加49ケースと既存VMテストの計50件がすべて成功しました。
WindowsのWasm・Demo・Doomのrestore/buildが成功し、.NETテストは268件成功、Doomの既知の2件のみ失敗しました。

| リポジトリ | 成功 | 失敗 |
| --- | ---: | ---: |
| Wasm | 64 | 0 |
| Demo | 71 | 0 |
| Doom | 133 | 2（既知） |

Doomの既知の失敗は以下の2件で、前回CT5とテスト名・成否・エラーメッセージが一致しました。

- `DoomPrompt_UsesCanonicalAisthesisNoesisKrisisKinesisPanelMapping`
- `DoomWebGpuProvider_SeparatesRawFramebufferAndHudCompositeTargets`

どちらも`const HUD_UNIFORM_FLOAT_COUNT = 32`を探す既存Assert.Containsの失敗です。
対象3リポジトリの既存テストの成否を照合し、新しい失敗や削除がないことを確認しました。

ソースJS・Wasm出力・候補nupkg・隔離キャッシュ・DoomのJS出力2か所がSHA-256で一致しました。
JS SHA-256: `f97955a72e64d6c887fe47bfb0bfcae49546ddfda1705d641d3618ce8ea4cf13`。
WebGpuComputeProvider.dllも生成元・パッケージ・キャッシュ・出力9か所で一致しました。

全体の集計は`verified-results.json`、.NET結果は`results/*.trx`、JS結果は`js-tests.tap`、
改修前の再現は`reproduction-js.tap`です。

次の候補はV2（Result/SelectManyのモナド則テスト）です。TL2aは依存先V1がdevへ未統合のため保留です。

再実行:

```powershell
node --test --test-reporter=tap tests/js/webgpu-limits.test.mjs tests/js/webgpu-envelope-bridge-vm.test.mjs
dotnet build AIKernel.Wasm.slnx -c Release
dotnet test AIKernel.Wasm.slnx -c Release --no-build
```

証跡: `D:\AIKernel\.codex_tmp\review-ws4-20261113`。
NuGetパッケージとキャッシュは専用フィードに隔離し、Demo/Doomのdevコピーから検証します。
検証用パッケージは現行の0.1.3設定で作成したもので、公開用ではありません。

## 範囲と残る確認点

対象はWasmが配布するブラウザーexecutorの標準3パス、envelopeの宣言、提供するバッファヘルパーです。
公開済みC#インターフェース、DTO、enum、Canon、ハッシュ計算は変更していません。

任意の`IWebGpuJsInterop`実装や注入パスがGPUDeviceを直接操作する実装は外部ホストが担当します。
既存のC#相互運用契約にはlimits取得の項目がないため、外部実装を一律に検証する契約は今回追加していません。
提供ヘルパーを使わない外部GPUコードの上限チェックも含める場合は、別途契約と実装範囲の整理が必要です。

テクスチャ寸法・シェーダー内のworkgroup_size、総VRAM予算、既存のGPUリソース解放処理の変更は含みません。
実GPU、Linux、ブラウザー実機の動作・性能は未検証です。

push、PR、dev/mainへの統合、タグ作成、パッケージ公開は行いません。
