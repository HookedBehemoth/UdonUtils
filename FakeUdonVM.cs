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

using Il2CppSystem.Collections.Generic;
using VRC.Udon.Common.Interfaces;
using UnhollowerRuntimeLib;

namespace FakeUdon {
    public class FakeUdonVM : Il2CppSystem.Object /*, IUdonVM */ {
        public FakeUdonVM(System.IntPtr handle) : base(handle) {}
        public FakeUdonVM() : base(ClassInjector.DerivedConstructorPointer<FakeUdonVM>()) => ClassInjector.DerivedConstructorBody(this);

        private FakeUdonProgram _program;
        private uint _pc;
        public bool DebugLogging { get; set; }
        public uint GetProgramCounter() {
            return _pc;
        }
        public Stack<uint> GetStack() {
            return new Stack<uint>();
        }
        public IUdonHeap InspectHeap()
            => _program.Heap;

        public uint Interpret() {
            _program._methods[_pc].Invoke(_program._obj, null);
            _pc = 0;
            return 0;
        }
        public bool LoadProgram(IUdonProgram program) {
            _program = program.Cast<FakeUdonProgram>();
            return true;
        }
        public IUdonProgram RetrieveProgram() {
            return _program.Cast<IUdonProgram>();
        }
        public void SetProgramCounter(uint value) {
            _pc = value;
        }
    }
}
