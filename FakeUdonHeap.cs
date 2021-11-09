/*
 * Copyright (c) 2021 HookedBehemoth
 *
 * This program is free software; you can redistribute it and/or modify it
 * under the terms and conditions of the GNU General Public License,
 * version 3, as published by the Free Software Foundation.
 *
 * This program is distributed in the hope it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for
 * more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using MelonLoader;
using VRC.Udon;

namespace FakeUdon {
    public class FakeUdonHeap : Il2CppSystem.Object /*, IUdonHeap */ {
        public FakeUdonHeap(System.IntPtr handle) : base(handle) {}

        object _obj;
        FieldInfo[] _fields;
        public List<IUdonSymbol> Symbols;
        public List<string> SymbolNames;

        public FakeUdonHeap(Type type, object obj) : base(ClassInjector.DerivedConstructorPointer<FakeUdonHeap>()) {
            ClassInjector.DerivedConstructorBody(this);
            _fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                            .Where(f => f.IsPublic || f.GetCustomAttributes<UdonSharp.UdonSyncedAttribute>().Any())
                            .Where(f => f.DeclaringType == type || f.Name.StartsWith("__refl_const_intnl_udonType"))
                            .ToArray();

            _obj = obj;
            Symbols = _fields.Select<FieldInfo, IUdonSymbol>(
                (field, index) => new UdonSymbol(
                    field.Name,
                    Injector.FromFieldType(field.FieldType),
                    (uint)index
                ).Cast<IUdonSymbol>()).ToList();
            SymbolNames = _fields.Select<FieldInfo, string>(field => field.Name).ToList();
        }

        public void CopyHeapVariable(uint sourceAddress, uint destAddress) {
            var value = _fields[sourceAddress].GetValue(_obj);
            _fields[destAddress].SetValue(_obj, value);
        }

        public void DumpHeapObjects(Il2CppSystem.Object destination) {
            /* stub */
        }

        public uint GetHeapCapacity() {
            return (uint)_fields.Length;
        }

        public Il2CppSystem.Object GetHeapVariable(uint address)
            => GetHeapVariable<Il2CppSystem.Object>(address);

        public T GetHeapVariable<T>(uint address) {
            var type = _fields[address].FieldType;
            var value = _fields[address].GetValue(_obj);

            if (value == null) {
                return (T)(object)null;
            }

            if (typeof(T) == typeof(Il2CppSystem.Object)) {
                if (type.IsValueType) {
                    IntPtr pnt = Marshal.AllocHGlobal(type.IsEnum ? 4 : Marshal.SizeOf(type));
                    try {
                        Marshal.StructureToPtr(value, pnt, false);
                        var klass = (IntPtr)typeof(Il2CppClassPointerStore<>).MakeGenericType(type).GetField("NativeClassPtr").GetValue(null);
                        return (T)(object)new Il2CppSystem.Object(IL2CPP.il2cpp_value_box(klass, pnt));
                    } finally {
                        Marshal.FreeHGlobal(pnt);
                    }
                } else if (type.BaseType == typeof(UdonSharp.UdonSharpBehaviour)) {
                    value = (value as UdonSharp.UdonSharpBehaviour).gameObject;
                } else if (type == typeof(string)) {
                    return (T)(object)new Il2CppSystem.Object(IL2CPP.ManagedStringToIl2Cpp((string)value));
                } else if (type.IsArray) {
                    var baseType = type.GetElementType();
                    var arrayType = (baseType.IsValueType ? typeof(Il2CppStructArray<>) : typeof(Il2CppReferenceArray<>)).MakeGenericType(baseType);
                    var castMethod = arrayType.GetMethod(
                        "op_Implicit",
                        (BindingFlags.Public | BindingFlags.Static), 
                        null, 
                        new Type[] { type }, 
                        new ParameterModifier[0]
                    );
                    value = castMethod.Invoke(null, new object[] { value });
                }
                return (T)(object)(value as Il2CppObjectBase).Cast<Il2CppSystem.Object>();
            } else {
                return (T)value;
            }
        }

        public Il2CppSystem.Type GetHeapVariableType(uint address) {
            return Injector.FromFieldType(_fields[address].FieldType);
        }

        public void InitializeHeapVariable(uint address, Il2CppSystem.Type type) {
            var field = _fields[address];
            field.SetValue(_obj, Activator.CreateInstance(field.FieldType));
        }

        public void InitializeHeapVariable<T>(uint address) {
            var field = _fields[address];
            field.SetValue(_obj, Activator.CreateInstance(field.FieldType));
        }

        public bool IsHeapVariableInitialized(uint address) {
            return _fields[address].GetValue(_obj) != null;
        }

        public void SetHeapVariable(uint address, Il2CppSystem.Object value, Il2CppSystem.Type type)
            => SetHeapVariable<Il2CppSystem.Object>(address, value);

        public unsafe void SetHeapVariable<T>(uint address, T value) {
            var type = _fields[address].FieldType;
            if (typeof(T) == typeof(Il2CppSystem.Object)) {
                var obj = value as Il2CppSystem.Object;
                if (type.IsValueType) {
                    _fields[address].SetValue(_obj, Marshal.PtrToStructure(IL2CPP.il2cpp_object_unbox(obj.Pointer), type));
                } else if (type.BaseType == typeof(UdonSharp.UdonSharpBehaviour)) {
                    var behaviour = Injector.GetOrCreateBehaviour(obj.Cast<UdonBehaviour>(), type);
                    _fields[address].SetValue(_obj, behaviour);
                } else if (type == typeof(string)) {
                    _fields[address].SetValue(_obj, IL2CPP.Il2CppStringToManaged(obj.Pointer));
                } else if (type.IsArray) {
                    var baseType = type.GetElementType();
                    var arrayType = (baseType.IsValueType ? typeof(Il2CppStructArray<>) : typeof(Il2CppReferenceArray<>)).MakeGenericType(baseType);
                    var array = Activator.CreateInstance(arrayType, obj.Pointer);
                    var castMethod = arrayType.BaseType.GetMethod(
                        "op_Implicit",
                        (BindingFlags.Public | BindingFlags.Static),
                        null,
                        new Type[] { arrayType },
                        new ParameterModifier[0]
                    );
                    var nativeArray = castMethod.Invoke(null, new object[] { array });
                    _fields[address].SetValue(_obj, nativeArray);
                } else {
                    _fields[address].SetValue(_obj, Activator.CreateInstance(type, obj.Pointer));
                }
            } else {
                _fields[address].SetValue(_obj, value);
            }
        }

        /* Unsupported Parameter type: reference */
        // public bool TryGetHeapVariable(uint address, out Il2CppSystem.Object value)
        //     => TryGetHeapVariable<Il2CppSystem.Object>(address, out value);

        // public bool TryGetHeapVariable<T>(uint address, out T value) {
        //     if (!IsHeapVariableInitialized(address)) {
        //         value = default (T);
        //         return false;
        //     }

        //     value = (T)_fields[address].GetValue(_obj);
        //     return true;
        // }
    }
}
