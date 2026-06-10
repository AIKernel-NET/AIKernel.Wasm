from __future__ import annotations

from dataclasses import dataclass

from .managed import ManagedObject, create_managed


@dataclass(frozen=True)
class WasmProviderContract:
    """[EN] Python descriptor for a public WASM provider wrapper.

    [JA] 公開 WASM Provider wrapper の Python descriptor です。
    """

    provider_id: str
    name: str
    managed_type: str


_RUNTIME_ASSEMBLY = "AIKernel.Wasm.Runtime"


class WasmRuntime(ManagedObject):
    """[EN] Wrapper for the public WasmRuntime process host.

    [JA] 公開 WasmRuntime process host の wrapper です。
    """

    @classmethod
    def create(cls) -> "WasmRuntime":
        """[EN] Create a managed WASM runtime.

        [JA] managed WASM runtime を作成します。
        """
        return cls(create_managed("AIKernel.Wasm.Runtime.WasmRuntime", _RUNTIME_ASSEMBLY))


class WasmRuntimeContext(ManagedObject):
    """[EN] Wrapper for the public WasmRuntimeContext.

    [JA] 公開 WasmRuntimeContext の wrapper です。
    """

    @classmethod
    def create(cls) -> "WasmRuntimeContext":
        """[EN] Create a managed WASM runtime context.

        [JA] managed WASM runtime context を作成します。
        """
        return cls(create_managed("AIKernel.Wasm.Runtime.WasmRuntimeContext", _RUNTIME_ASSEMBLY))


class WasmProcessProvider(ManagedObject):
    """[EN] Wrapper for WasmProcessProvider.

    [JA] WasmProcessProvider の wrapper です。
    """

    @classmethod
    def create(cls) -> "WasmProcessProvider":
        """[EN] Create the managed process provider.

        [JA] managed process Provider を作成します。
        """
        return cls(create_managed("AIKernel.Wasm.Runtime.WasmProcessProvider", _RUNTIME_ASSEMBLY))


def _provider_wrapper(name: str, type_name: str):
    return type(
        name,
        (ManagedObject,),
        {
            "__doc__": f"[EN] Wrapper for {name}.\n\n[JA] {name} の wrapper です。",
            "create": classmethod(lambda cls, t=type_name: cls(create_managed(t, _RUNTIME_ASSEMBLY))),
        },
    )


WasmMemoryProvider = _provider_wrapper("WasmMemoryProvider", "AIKernel.Wasm.Runtime.WasmMemoryProvider")
WasmStdinProvider = _provider_wrapper("WasmStdinProvider", "AIKernel.Wasm.Runtime.WasmStdinProvider")
WasmFileSystemProvider = _provider_wrapper("WasmFileSystemProvider", "AIKernel.Wasm.Runtime.WasmFileSystemProvider")
WasmEventProvider = _provider_wrapper("WasmEventProvider", "AIKernel.Wasm.Runtime.WasmEventProvider")
WasmAudioProvider = _provider_wrapper("WasmAudioProvider", "AIKernel.Wasm.Runtime.WasmAudioProvider")
WasmScreenshotProvider = _provider_wrapper("WasmScreenshotProvider", "AIKernel.Wasm.Runtime.WasmScreenshotProvider")
WasmSaveStateProvider = _provider_wrapper("WasmSaveStateProvider", "AIKernel.Wasm.Runtime.WasmSaveStateProvider")
WasmTimeProvider = _provider_wrapper("WasmTimeProvider", "AIKernel.Wasm.Runtime.WasmTimeProvider")


def wasm_provider_contracts() -> tuple[WasmProviderContract, ...]:
    """[EN] Return the public WASM provider surface covered by Python.

    [JA] Python が網羅する公開 WASM Provider surface を返します。
    """
    return (
        WasmProviderContract("wasm.runtime", "WASM Runtime", "AIKernel.Wasm.Runtime.WasmRuntime"),
        WasmProviderContract("wasm.process", "WASM Process Provider", "AIKernel.Wasm.Runtime.WasmProcessProvider"),
        WasmProviderContract("wasm.memory", "WASM Memory Provider", "AIKernel.Wasm.Runtime.WasmMemoryProvider"),
        WasmProviderContract("wasm.stdin", "WASM Stdin Provider", "AIKernel.Wasm.Runtime.WasmStdinProvider"),
        WasmProviderContract("wasm.filesystem", "WASM File System Provider", "AIKernel.Wasm.Runtime.WasmFileSystemProvider"),
        WasmProviderContract("wasm.event", "WASM Event Provider", "AIKernel.Wasm.Runtime.WasmEventProvider"),
        WasmProviderContract("wasm.audio", "WASM Audio Provider", "AIKernel.Wasm.Runtime.WasmAudioProvider"),
        WasmProviderContract("wasm.screenshot", "WASM Screenshot Provider", "AIKernel.Wasm.Runtime.WasmScreenshotProvider"),
        WasmProviderContract("wasm.savestate", "WASM Save State Provider", "AIKernel.Wasm.Runtime.WasmSaveStateProvider"),
        WasmProviderContract("wasm.time", "WASM Time Provider", "AIKernel.Wasm.Runtime.WasmTimeProvider"),
    )
