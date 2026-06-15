from __future__ import annotations

from dataclasses import dataclass

from .managed import ManagedObject, call_static, create_managed

_WEBGPU_BRIDGE_TYPE = "AIKernel.Wasm.Compute.WebGpuComputePythonBridge"
_WEBGPU_ASSEMBLY = "WebGpuComputeProvider"


@dataclass(frozen=True)
class CapabilityContract:
    """[EN] Python view of a managed CapabilityModuleDescriptor.

    [JA] managed CapabilityModuleDescriptor の Python view です。
    """

    _managed: object

    @property
    def managed(self):
        """[EN] Return the underlying C# descriptor.

        [JA] 背後の C# descriptor を返します。
        """
        return self._managed

    @property
    def capability_id(self) -> str:
        """[EN] Return the capability identifier.

        [JA] capability identifier を返します。
        """
        return str(self.managed.CapabilityId)

    @property
    def name(self) -> str:
        """[EN] Return the capability display name.

        [JA] capability display name を返します。
        """
        return str(self.managed.Name)

    @property
    def provided_operations(self) -> tuple[str, ...]:
        """[EN] Return provided operation names.

        [JA] provided operation name を返します。
        """
        return tuple(str(value) for value in self.managed.ProvidedOperations)

    @property
    def metadata(self) -> dict[str, str]:
        """[EN] Return descriptor metadata.

        [JA] descriptor metadata を返します。
        """
        return {str(key): str(self.managed.Metadata[key]) for key in self.managed.Metadata.Keys}


@dataclass(frozen=True)
class WebGpuComputeCapability:
    """[EN] Wrapper for the WebGPU compute capability contract.

    [JA] WebGPU compute capability contract の wrapper です。
    """

    provider_id: str = "webgpu.compute"
    adapter_profile: str = "wasm-webgpu"

    def to_contract(self) -> CapabilityContract:
        """[EN] Create the managed capability descriptor.

        [JA] managed capability descriptor を作成します。
        """
        return CapabilityContract(
            call_static(
                _WEBGPU_BRIDGE_TYPE,
                _WEBGPU_ASSEMBLY,
                "ToContract",
                self.provider_id,
                self.adapter_profile,
            )
        )


class WebGpuComputeProvider(ManagedObject):
    """[EN] Wrapper for the public WebGpuComputeProvider.

    [JA] 公開 WebGpuComputeProvider の wrapper です。
    """

    @classmethod
    def create(cls) -> "WebGpuComputeProvider":
        """[EN] Create a managed WebGPU compute provider.

        [JA] managed WebGPU compute Provider を作成します。
        """
        return cls(call_static(_WEBGPU_BRIDGE_TYPE, _WEBGPU_ASSEMBLY, "CreateProvider"))


class WebGpuComputeInvoker(ManagedObject):
    """[EN] Wrapper for the WebGPU compute capability invoker.

    [JA] WebGPU compute capability invoker の wrapper です。
    """

    @classmethod
    def create(cls) -> "WebGpuComputeInvoker":
        """[EN] Create a managed WebGPU compute invoker.

        [JA] managed WebGPU compute invoker を作成します。
        """
        return cls(call_static(_WEBGPU_BRIDGE_TYPE, _WEBGPU_ASSEMBLY, "CreateInvoker"))


class WebGpuNativeBackend(ManagedObject):
    """[EN] Wrapper for the native WebGPU backend adapter.

    [JA] native WebGPU backend adapter の wrapper です。
    """

    @classmethod
    def create(cls) -> "WebGpuNativeBackend":
        """[EN] Create the native backend adapter.

        [JA] native backend adapter を作成します。
        """
        return cls(create_managed("AIKernel.Wasm.Compute.WebGpuNativeBackend", _WEBGPU_ASSEMBLY))


class WebGpuWasmBackend(ManagedObject):
    """[EN] Wrapper for the browser/WASM WebGPU backend adapter.

    [JA] browser/WASM WebGPU backend adapter の wrapper です。
    """

    @classmethod
    def create(cls) -> "WebGpuWasmBackend":
        """[EN] Create the WASM backend adapter.

        [JA] WASM backend adapter を作成します。
        """
        return cls(create_managed("AIKernel.Wasm.Compute.WebGpuWasmBackend", _WEBGPU_ASSEMBLY))
