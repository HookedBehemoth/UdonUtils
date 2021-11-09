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

using UnhollowerBaseLib;
using VRC.Udon;
using System;
using System.Linq;

public static class UdonExtensions {
    private static Il2CppSystem.Object GetValue(this UdonBehaviour _this, string name) {
        var program = _this._program;

        var address = program.SymbolTable.GetAddressFromSymbol(name);
        var value = program.Heap.GetHeapVariable(address);

        return value;
    }

    private static void SetValue(this UdonBehaviour _this, string name, Il2CppSystem.Object value) {
        var program = _this._program;

        var address = program.SymbolTable.GetAddressFromSymbol(name);
        var type = program.Heap.GetHeapVariableType(address);

        program.Heap.SetHeapVariable(address, value, type);
    }

    public static T GetObject<T>(this UdonBehaviour _this, string name) where T : Il2CppSystem.Object {
        return _this.GetValue(name).Cast<T>();
    }

    public static T GetPrimitive<T>(this UdonBehaviour _this, string name) where T : unmanaged {
        return _this.GetValue(name).Unbox<T>();
    }

    public unsafe static void SetPrimitive<T>(this UdonBehaviour _this, string name, T value) where T : unmanaged {
        var ptr = IL2CPP.il2cpp_value_box(Il2CppClassPointerStore<T>.NativeClassPtr, (IntPtr)(&value));
        var v = new Il2CppSystem.Object(ptr);
        _this.SetValue(name, v);
    }

    public static T[] GetArray<T>(this UdonBehaviour _this, string name) where T : unmanaged {
        return _this.GetValue(name).Cast<Il2CppStructArray<T>>();
    }

    public static void SetArray<T>(this UdonBehaviour _this, string name, T[] t) where T : unmanaged {
        var value = (Il2CppStructArray<T>)t;

        _this.SetValue(name, value.Cast<Il2CppSystem.Object>());
    }

    public static T[][] Get2DArray<T>(this UdonBehaviour _this, string name) where T : unmanaged {
        var array = _this.GetValue(name).Cast<Il2CppReferenceArray<Il2CppSystem.Object>>();
        var result = new T[array.Length][];

        foreach (var s in Enumerable.Range(0, array.Length)) {
            var k = array[s].Cast<Il2CppStructArray<T>>();
            result[s] = new T[k.Length];
            k.CopyTo(result[s], 0);
        }

        return result;
    }

    public static void Set2DArray<T>(this UdonBehaviour _this, string name, T[][] t) where T : unmanaged {
        var result = new Il2CppReferenceArray<Il2CppSystem.Object>(t.Length);
        foreach (var s in Enumerable.Range(0, t.Length)) {
            result[s] = ((Il2CppStructArray<T>)t[s]).Cast<Il2CppSystem.Object>();
        }
        _this.SetValue(name, result.Cast<Il2CppSystem.Object>());
    }

    public static T[][][] Get3DArray<T>(this UdonBehaviour _this, string name) where T : unmanaged {
        var array = _this.GetValue(name).Cast<Il2CppReferenceArray<Il2CppSystem.Object>>();
        var result = new T[array.Length][][];

        foreach (var s in Enumerable.Range(0, array.Length)) {
            var k = array[s].Cast<Il2CppReferenceArray<Il2CppSystem.Object>>();
            result[s] = new T[k.Length][];
            foreach (var t in Enumerable.Range(0, k.Length)) {
                var l = k[t].Cast<Il2CppStructArray<T>>();
                result[s][t] = new T[l.Length];
                l.CopyTo(result[s][t], 0);
            }
        }

        return result;
    }
}
