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
    """[EN] Create a managed object using its public parameterless or optional defaults.

    [JA] public コンストラクターの引数なし呼び出し、または省略可能な引数の既定値で作成します。
    """
    resolved = managed_type(type_name, assembly_name)
    from System import Activator, Type  # type: ignore[import-not-found]

    default_constructors = []
    for constructor in resolved.GetConstructors():
        parameters = constructor.GetParameters()
        if len(parameters) == 0:
            return Activator.CreateInstance(resolved)
        if all(parameter.IsOptional and parameter.HasDefaultValue for parameter in parameters):
            default_constructors.append((constructor, len(parameters)))

    if len(default_constructors) == 1:
        constructor, parameter_count = default_constructors[0]
        # Let .NET substitute the typed defaults instead of round-tripping them through Python.
        return constructor.Invoke(_object_array((Type.Missing,) * parameter_count))

    # Preserve the original failure for required arguments or ambiguous overloads.
    return Activator.CreateInstance(resolved)


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
