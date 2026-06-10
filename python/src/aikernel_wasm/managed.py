from __future__ import annotations

from .native import load_wasm_runtime


class ManagedObject:
    """[EN] Base wrapper for public AIKernel.Wasm managed objects.

    [JA] 公開 AIKernel.Wasm managed object の基底 wrapper です。
    """

    def __init__(self, managed):
        self._managed = managed

    @property
    def managed(self):
        """[EN] Return the underlying C# object.

        [JA] 背後の C# object を返します。
        """
        return self._managed

    def to_managed(self):
        """[EN] Return the underlying C# object.

        [JA] 背後の C# object を返します。
        """
        return self._managed


def managed_type(type_name: str, assembly_name: str):
    """[EN] Resolve a C# type by assembly-qualified name.

    [JA] assembly-qualified name で C# type を解決します。
    """
    load_wasm_runtime()
    from System import Type  # type: ignore[import-not-found]

    resolved = Type.GetType(f"{type_name}, {assembly_name}")
    if resolved is None:
        raise RuntimeError(f"Unable to resolve managed type: {type_name}, {assembly_name}")
    return resolved


def create_managed(type_name: str, assembly_name: str):
    """[EN] Create a managed object with its public default constructor.

    [JA] public default constructor で managed object を作成します。
    """
    load_wasm_runtime()
    from System import Activator  # type: ignore[import-not-found]

    return Activator.CreateInstance(managed_type(type_name, assembly_name))


def call_static(type_name: str, assembly_name: str, method_name: str, *args):
    """[EN] Invoke a public static managed method.

    [JA] 公開 static managed method を呼び出します。
    """
    method = managed_type(type_name, assembly_name).GetMethod(method_name)
    if method is None:
        raise RuntimeError(f"Unable to resolve managed method: {type_name}.{method_name}")
    return method.Invoke(None, _object_array(args))


def _object_array(values):
    load_wasm_runtime()
    from System import Array, Object  # type: ignore[import-not-found]

    items = Array[Object](len(values))
    for index, value in enumerate(values):
        items[index] = value
    return items
