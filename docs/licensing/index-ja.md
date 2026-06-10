# Licensing

[English](index.md)

AIKernel.Wasm は package release 用に repository で宣言された license に従います。
NuGet / Python package metadata は、他の AIKernel public package と同じ license
expression を使用します。

## Package Metadata

Public package metadata には次を含めます。

- package license expression
- project URL
- repository URL
- package icon
- support される場合は bilingual README reference

## Dependency Notes

AIKernel.Wasm は AIKernel contracts、Core runtime package、Providers.Standard
CPU fallback、Python wrapper 用の pythonnet を使用します。Browser WebGPU binding
は `IWebGpuJsInterop` 境界の背後に置きます。

新しい browser / native dependency を追加する場合は、release notes と package
metadata で license 情報を見える状態に保ちます。
