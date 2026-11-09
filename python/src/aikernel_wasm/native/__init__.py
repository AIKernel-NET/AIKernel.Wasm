"""[EN]
Managed assembly discovery and pythonnet loading for AIKernel.Wasm.

[JA]
AIKernel.Wasm の managed assembly 探索と pythonnet 読み込みを提供します。
"""

from __future__ import annotations

import os
from dataclasses import dataclass
from pathlib import Path


_WASM_PACKAGE_VERSION = "0.1.3"
_PROVIDER_PACKAGE_VERSION = "0.1.3"
_CORE_PACKAGE_VERSION = "0.1.3"
_CONTRACT_PACKAGE_VERSION = "0.1.3"
_MICROSOFT_EXTENSIONS_VERSION = "10.0.8"
_ASSEMBLIES = (
    "AIKernel.Abstractions.dll",
    "AIKernel.Common.dll",
    "AIKernel.Core.dll",
    "AIKernel.Dtos.dll",
    "AIKernel.Enums.dll",
    "AIKernel.Providers.Standard.dll",
    "AIKernel.Wasm.Runtime.dll",
    "WebGpuComputeProvider.dll",
    "Microsoft.Extensions.DependencyInjection.Abstractions.dll",
)
_ASSEMBLY_PACKAGES = {
    "AIKernel.Abstractions.dll": ("AIKernel.Abstractions", _CONTRACT_PACKAGE_VERSION),
    "AIKernel.Common.dll": ("AIKernel.Common", _CORE_PACKAGE_VERSION),
    "AIKernel.Core.dll": ("AIKernel.Core", _CORE_PACKAGE_VERSION),
    "AIKernel.Dtos.dll": ("AIKernel.Dtos", _CONTRACT_PACKAGE_VERSION),
    "AIKernel.Enums.dll": ("AIKernel.Enums", _CONTRACT_PACKAGE_VERSION),
    "AIKernel.Providers.Standard.dll": ("AIKernel.Providers.Standard", _PROVIDER_PACKAGE_VERSION),
    "AIKernel.Wasm.Runtime.dll": ("AIKernel.Wasm.Runtime", _WASM_PACKAGE_VERSION),
    "WebGpuComputeProvider.dll": ("AIKernel.Wasm.WebGpuComputeProvider", _WASM_PACKAGE_VERSION),
    "Microsoft.Extensions.DependencyInjection.Abstractions.dll": (
        "Microsoft.Extensions.DependencyInjection.Abstractions",
        _MICROSOFT_EXTENSIONS_VERSION,
    ),
}


@dataclass(frozen=True)
class WasmAssemblySet:
    """[EN] Resolved managed assemblies for AIKernel.Wasm.

    [JA] AIKernel.Wasm 用に解決された managed assembly 群です。
    """

    root: Path
    assemblies: tuple[Path, ...]

    @property
    def is_complete(self) -> bool:
        """[EN] Return whether every expected assembly exists.

        [JA] 期待されるすべての assembly が存在するかを返します。
        """
        return all(path.exists() for path in self.assemblies)

    @property
    def missing(self) -> tuple[str, ...]:
        """[EN] Return missing assembly names.

        [JA] 不足している assembly 名を返します。
        """
        return tuple(path.name for path in self.assemblies if not path.exists())


def wasm_assemblies() -> WasmAssemblySet:
    """[EN] Resolve bundled or NuGet-provided WASM assemblies.

    [JA] 同梱または NuGet 提供の WASM assembly を解決します。
    """
    root = _native_root()
    return WasmAssemblySet(root=root, assemblies=tuple(_resolve_assembly(name) for name in _ASSEMBLIES))


def require_wasm_assemblies() -> WasmAssemblySet:
    """[EN] Resolve WASM assemblies and fail closed when any are missing.

    [JA] WASM assembly を解決し、不足があれば fail-closed します。
    """
    assemblies = wasm_assemblies()
    if not assemblies.is_complete:
        raise FileNotFoundError(
            "AIKernel.Wasm managed assemblies are not available: "
            f"{', '.join(assemblies.missing)}. Bundle them in aikernel_wasm/native "
            "or restore the corresponding NuGet packages."
        )
    return assemblies


def load_wasm_runtime() -> WasmAssemblySet:
    """[EN] Load AIKernel.Wasm assemblies through pythonnet.

    [JA] AIKernel.Wasm assembly を pythonnet 経由で読み込みます。
    """
    assemblies = require_wasm_assemblies()
    try:
        from pythonnet import load  # type: ignore[import-not-found]

        try:
            load("coreclr")
        except RuntimeError:
            pass
        import clr  # type: ignore[import-not-found]
    except ImportError as exc:
        raise RuntimeError("pythonnet is required to load AIKernel.Wasm assemblies.") from exc

    for assembly in assemblies.assemblies:
        clr.AddReference(str(assembly))
    return assemblies


def _resolve_assembly(name: str) -> Path:
    for root in _assembly_roots():
        candidate = root / name
        if candidate.exists():
            return candidate

    nuget_candidate = _resolve_nuget_assembly(name)
    if nuget_candidate is not None:
        return nuget_candidate

    return _native_root() / name


def _assembly_roots() -> tuple[Path, ...]:
    roots = [_native_root()]
    roots.extend(_source_build_roots())
    override = os.environ.get("AIKERNEL_WASM_ASSEMBLY_PATH")
    if override:
        roots.extend(Path(path) for path in override.split(os.pathsep) if path)
    return tuple(roots)


def _native_root() -> Path:
    return Path(__file__).resolve().parent


def _source_build_roots() -> tuple[Path, ...]:
    repository_root = Path(__file__).resolve().parents[4]
    workspace_root = repository_root.parent
    return tuple(
        path
        for path in (
            repository_root / "src" / "Runtime" / "AIKernel.Wasm.Runtime" / "bin" / "Release" / "net10.0",
            repository_root / "src" / "Compute" / "WebGpuComputeProvider" / "bin" / "Release" / "net10.0",
            workspace_root / "AIKernel.Providers" / "src" / "Standard" / "AIKernel.Providers.Standard" / "bin" / "Release" / "net10.0",
        )
        if path.exists()
    )


def _resolve_nuget_assembly(name: str) -> Path | None:
    package, version = _ASSEMBLY_PACKAGES[name]
    package_root = _nuget_root() / package.lower() / version / "lib"
    for framework in ("net10.0", "net9.0", "net8.0", "netstandard2.1", "netstandard2.0"):
        candidate = package_root / framework / name
        if candidate.exists():
            return candidate
    for candidate in package_root.rglob(name):
        if candidate.exists():
            return candidate
    return None


def _nuget_root() -> Path:
    configured = os.environ.get("NUGET_PACKAGES")
    if configured:
        return Path(configured)
    return Path.home() / ".nuget" / "packages"
