/*
 * Copyright (c) 2023 HookedBehemoth
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
using Il2CppVRC.Udon;
using Il2CppInterop.Runtime.Injection;
using Il2CppVRC.Udon.Common.Interfaces;

[assembly: MelonInfo(typeof(UdonUtils.Starter), nameof(UdonUtils), "1.0.0", "HookedBehemoth")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace UdonUtils
{
    public class Starter : MelonMod
    {
        public override void OnInitializeMelon()
        {
            ClassInjector.RegisterTypeInIl2Cpp<FakeUdon.FakeUdonProgram>(new RegisterTypeOptions { Interfaces = new[] { typeof(IUdonProgram) } });
            ClassInjector.RegisterTypeInIl2Cpp<FakeUdon.FakeUdonVM>(new RegisterTypeOptions { Interfaces = new[] { typeof(IUdonVM) } });
            ClassInjector.RegisterTypeInIl2Cpp<FakeUdon.FakeUdonHeap>(new RegisterTypeOptions { Interfaces = new[] { typeof(IUdonHeap) } });

            HarmonyInstance.Patch(
                typeof(UdonBehaviour).GetMethod("InitializeUdonContent", BindingFlags.Public | BindingFlags.Instance),
                prefix: new HarmonyLib.HarmonyMethod(typeof(FakeUdon.Injector).GetMethod("InitializeUdonContentInjected", BindingFlags.NonPublic | BindingFlags.Static)),
                postfix: new HarmonyLib.HarmonyMethod(typeof(Starter).GetMethod(nameof(OnUdonBehaviourLoaded), BindingFlags.NonPublic | BindingFlags.Static)));
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            if (buildIndex == -1)
                FakeUdon.Injector.ClearBehaviours();
        }

        public static event Action<UdonBehaviour> OnUdonBehaviourLoadedEvent;
        private static void OnUdonBehaviourLoaded(UdonBehaviour __instance)
            => OnUdonBehaviourLoadedEvent?.Invoke(__instance);
    }
}
