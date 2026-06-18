# Operations and Release Checklist

[English](index.md)

AIKernel.Wasm package 公開前にこの checklist を使用します。

## Build and Test

```powershell
dotnet build AIKernel.Wasm.slnx -c Release -p:WarningsAsErrors=1591
dotnet test AIKernel.Wasm.slnx -c Release --no-build
```

成功条件:

- public C# member の XML documentation warning がない
- runtime test がすべて通る
- audio、display、input、concept-elevation boundary test がすべて通る
- WebGPU provider test がすべて通る

## Package Metadata

NuGet package metadata の確認項目:

- package ID
- version
- license expression
- project URL
- repository URL
- package icon
- README inclusion
- release notes

v0.1.2.1 NuGet patch では、AIKernel.Wasm package artifact を `0.1.2.1`
として公開します。AIKernel.NET、AIKernel.Core、AIKernel.Providers への依存は
`0.1.2` のまま維持します。Python wrapper は、別途 PyPI patch task が明示されない限り
`0.1.2.dev{buildNumber}` の `aikernel-wasm` wheel で検証します。

## Documentation

必須 documentation:

- repository README 英日
- docs index 英日
- architecture guide
- runtime provider guide
- audio、display、input 向け browser boundary provider guide
- WebGPU guide
- concept elevation notes
- Python wrapper reference guide
- testing guide
- licensing guide
- manifest / metadata guide

## Browser GPU Validation

Automated Release test では物理 GPU を要求しません。Browser WebGPU validation は
browser、driver、adapter、platform state に依存するため、別途 E2E validation と
して管理します。

## Safety Checks

release 前に docs / reference Python source を scan します。

- stale capability ID
- temporary / incomplete wording
- unsafe scheduler example
- sensitive metadata
- public descriptor への browser implementation object 漏れ
