# AIKernel.Wasm Documentation

[English](README.md)

AIKernel.Wasm は AIKernel の browser / WebAssembly runtime layer です。WASM
process execution、WebGPU compute、browser 境界の runtime service を Core や
host 側 Providers から分離します。

この docs は、AIOS SDK の sandboxed runtime layer として Wasm を説明します。
browser / WebAssembly process、isolated memory、WASI-style service、WebGPU
boundary を扱う軽量 VM surface として機能します。

公式 AIOS ディストリビューション **AIKernel.Monolith** の開発も開始されています。
Monolith は 0.1.x 系の安定化後に WASM sandbox を kernel runtime、providers、
control、tools と統合する標準 reference distribution として位置づけられます。

## リポジトリ横断整合

共有の repository boundary、v0.1.3 development versioning、依存関係順、
GPU rev3 provider metadata、Python wrapper scope は canonical AIKernel GPU rev3
integration plan に従います。共有の v0.1.3 alignment document が公開されるまでは、
Wasm package / wrapper version を AIKernel.Core、AIKernel.Control、
AIKernel.Providers、Dawn、Cuda13 と同期してください。履歴としての v0.1.1.1 validation rule は
[AIKernel Repository Alignment v0.1.1.1](https://github.com/AIKernel-NET/AIKernel.NET/blob/main/docs/development/repository-alignment-v0.1.1.1-ja.md)
に残します。
複数 repository をまたぐ変更を行う場合は、まず
[リポジトリ横断開発者ガイド v0.1.1.1](https://github.com/AIKernel-NET/AIKernel.NET/blob/main/docs/development/cross-repository-developer-guide-v0.1.1.1-ja.md)
を読んでください。

Wasm は browser/WebAssembly execution、WebGPU/WebAudio、display、input、
perception、spatial、HUD、runtime surface を所有します。Doom semantics、
Providers substrate ownership、Gate/CTG decision は所有しません。

## Sections

- [Getting Started](getting-started/index-ja.md)
- [User Guide](user-guide/index-ja.md)
- [Architecture](architecture/index-ja.md)
- [Provider Catalog](providers/index-ja.md)
- [Runtime Providers](runtime/index-ja.md)
- [WebGPU Compute](webgpu/index-ja.md)
- [Concept Elevation Notes](development/concept-elevation.md) /
  [概念昇格ノート](development/concept-elevation-ja.md)
- [Manifests and Metadata](manifests/index-ja.md)
- [Python Wrapper](python/index-ja.md)
- [Testing](testing/index-ja.md)
- [Operations and Release Checklist](operations/index-ja.md)
- [Licensing](licensing/index-ja.md)

## どのページを読むべきか

- WASM runtime が Core と Providers.Standard の隣でどう位置づくかを最短で確認する場合は
  Getting Started を読んでください。
- host setup、package installation、deterministic runtime / provider workflow を
  確認する場合は User Guide を読んでください。
- process、memory、stdin、file system、event、audio、screenshot、save-state、
  time surface を扱う場合は Runtime Providers を読んでください。
- browser WebGPU integration や CPU fallback behavior を検証する場合は
  WebGPU Compute を読んでください。
- runtime を変更する前には Testing を読み、Windows / Linux の deterministic test と
  manual browser GPU check を分けて扱ってください。

## 最初の検証

まず deterministic test を実行してください。実 browser GPU validation は意図的に
別の manual step として扱います。

```powershell
dotnet build AIKernel.Wasm.slnx -c Release
dotnet test AIKernel.Wasm.slnx -c Release --no-build
```

## Release Scope

Version 0.1.3 は現在の canonical GPU integration line のままです。AIKernel.Wasm は
この repository の NuGet 限定 patch line として `0.1.3` を公開します。local NuGet package reference
には `0.1.3-dev{build-number}`、local `aikernel-wasm` wheel validation には
`0.1.3.dev{build-number}` を使います。次を提供します。

- `AIKernel.Wasm.Runtime`
- `AIKernel.Wasm.Audio`
- `AIKernel.Wasm.Display`
- `AIKernel.Wasm.Input`
- `AIKernel.Wasm.WebGpuComputeProvider`
- WASM process、memory、stdin、file system、event、audio、screenshot、
  save-state、time Provider surface
- browser audio、frame-source、framebuffer、分解済み virtual input の
  boundary surface
- `AIKernel.Providers.Standard` 経由の deterministic CPU fallback 付き
  WebGPU compute

AIKernel.Wasm の stable NuGet package artifact は `0.1.3` として作成します。
Python wrapper は、別途 PyPI patch task が明示されない限り、同期済み `0.1.3`
line のまま維持します。

AIKernel.Wasm は AIKernel Core contract と Providers.Standard fallback driver に
依存しますが、browser / WASM 固有の実装責務を Core へ戻しません。
