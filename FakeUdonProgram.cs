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

using System.Reflection;
using VRC.Udon.Common.Interfaces;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;

namespace FakeUdon {
    public class FakeUdonProgram : Il2CppSystem.Object /*, IUdonProgram */ {
        public FakeUdonProgram(System.IntPtr handle) : base(handle) {}
        public FakeUdonProgram() : base(ClassInjector.DerivedConstructorPointer<FakeUdonProgram>()) => ClassInjector.DerivedConstructorBody(this);

        public string InstructionSetIdentifier => "FakeUdonProgram";
        public int InstructionSetVersion => 1;
        public Il2CppStructArray<byte> ByteCode => null;
        public IUdonSyncMetadataTable SyncMetadataTable { get; set; }
        public int UpdateOrder => 0;
        public IUdonHeap Heap { get; set; }
        public IUdonSymbolTable EntryPoints { get; set; }
        public IUdonSymbolTable SymbolTable { get; set; }
        public object _obj;
        public MethodInfo[] _methods;
    }
}
