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

using MelonLoader;
using System;
using System.Reflection;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using static UnhollowerRuntimeLib.ClassInjector;

[assembly: MelonInfo(typeof(UdonUtils.Starter), nameof(UdonUtils), "1.0.0")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace UdonUtils {
    public class Starter : MelonMod {
        public override void OnApplicationStart() {
            RegisterTypeInIl2CppWithInterfaces<FakeUdon.FakeUdonProgram>(true, typeof(IUdonProgram));
            RegisterTypeInIl2CppWithInterfaces<FakeUdon.FakeUdonVM>(true, typeof(IUdonVM));
            RegisterTypeInIl2CppWithInterfaces<FakeUdon.FakeUdonHeap>(true, typeof(IUdonHeap));

            HarmonyInstance.Patch(
                typeof(VRC.Udon.UdonBehaviour).GetMethod("InitializeUdonContent", BindingFlags.Public | BindingFlags.Instance),
                prefix: new HarmonyLib.HarmonyMethod(typeof(FakeUdon.Injector).GetMethod("InitializeUdonContentInjected", BindingFlags.NonPublic | BindingFlags.Static)),
                postfix: new HarmonyLib.HarmonyMethod(typeof(Starter).GetMethod(nameof(OnUdonBehaviourLoaded), BindingFlags.NonPublic | BindingFlags.Static)));
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName) {
            if (buildIndex == -1)
                FakeUdon.Injector.ClearBehaviours();
        }

        public static event Action<UdonBehaviour> OnUdonBehaviourLoadedEvent;
        private static void OnUdonBehaviourLoaded(UdonBehaviour __instance)
            => OnUdonBehaviourLoadedEvent?.Invoke(__instance);
    }
}
