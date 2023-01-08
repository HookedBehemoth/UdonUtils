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
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;

namespace FakeUdon
{
    static class Injector
    {
        private static bool InitializeUdonContentInjected(UdonBehaviour __instance)
        {
            /* Early abort */
            if (__instance._initialized)
                return false;

            if (!FakeUdonRegistry.Find(__instance, out var behaviourType))
                return true;

            MelonLogger.Msg($"Overwriting UdonBehaviour in {__instance.name} with {behaviourType.FullName}");
            __instance._udonManager = UdonManager.Instance;

            // LoadProgram()
            var behaviour = GetOrCreateBehaviour(__instance, behaviourType);

            var heap = new FakeUdonHeap(behaviourType, behaviour);
            /* Prevent heap from being garbage collected */
            Objects.Add(heap);

            var symbolTable = new UdonSymbolTable(heap.Symbols.ToIl2CppList(), heap.SymbolNames.ToIl2CppList().Cast<Il2CppSystem.Collections.Generic.IEnumerable<string>>());

            string FilterName(string name)
            {
                if (!EventTable.TryGetValue(name, out string replacement))
                    return name;
                return replacement;
            }

            var methods = behaviourType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                        .Where(m => m.DeclaringType == behaviourType)
                                        .Where(m => m.IsPublic || EventTable.ContainsKey(m.Name) || EventTable.ContainsValue(m.Name))
                                        .ToArray();

            var entries = new UdonSymbolTable(
                methods.Select<MethodInfo, IUdonSymbol>((method, index) => new UdonSymbol(FilterName(method.Name), FromFieldType(method.ReturnType), (uint)index).Cast<IUdonSymbol>()).ToList().ToIl2CppList(),
                methods.Select<MethodInfo, string>(method => FilterName(method.Name)).ToList().ToIl2CppList().Cast<Il2CppSystem.Collections.Generic.IEnumerable<string>>()
            );

            var syncFields = behaviourType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                            .Where(f => f.GetCustomAttributes(false).Any(a => a is UdonSharp.UdonSyncedAttribute))
                                            .Select(f => (f.Name, f.GetCustomAttribute<UdonSharp.UdonSyncedAttribute>().networkSyncType));

            var syncMetadataList = new Il2CppSystem.Collections.Generic.List<IUdonSyncMetadata>();
            foreach (var field in syncFields)
            {
                var syncProperties = new Il2CppSystem.Collections.Generic.List<IUdonSyncProperty>(1);
                syncProperties.Add(new UdonSyncProperty("this", field.networkSyncType).Cast<IUdonSyncProperty>());
                syncMetadataList.Add(new UdonSyncMetadata(field.Name, syncProperties).Cast<IUdonSyncMetadata>());
            }

            var syncMetadataTable = new UdonSyncMetadataTable(syncMetadataList.Cast<Il2CppSystem.Collections.Generic.IEnumerable<IUdonSyncMetadata>>());

            var program = new FakeUdonProgram
            {
                SyncMetadataTable = syncMetadataTable.Cast<IUdonSyncMetadataTable>(),
                Heap = new IUdonHeap(heap.Pointer),
                EntryPoints = entries.Cast<IUdonSymbolTable>(),
                SymbolTable = symbolTable.Cast<IUdonSymbolTable>(),
                _obj = behaviour,
                _methods = methods,
            };

            /* Prevent program from being garbage collected */
            Objects.Add(program);

            __instance._program = new IUdonProgram(program.Pointer);

            var variableTable = __instance.publicVariables.Cast<UdonVariableTable>();
            foreach (var variableSymbol in variableTable._publicVariables.keys)
            {
                if (!symbolTable.HasAddressForSymbol(variableSymbol))
                {
                    continue;
                }

                uint symbolAddress = symbolTable.GetAddressFromSymbol(variableSymbol);

                if (!__instance.publicVariables.TryGetVariableType(variableSymbol, out Il2CppSystem.Type declaredType))
                {
                    continue;
                }

                __instance.publicVariables.TryGetVariableValue(variableSymbol, out Il2CppSystem.Object value);
                if (declaredType == Il2CppType.Of<GameObject>() || declaredType == Il2CppType.Of<UdonBehaviour>() ||
                    declaredType == Il2CppType.Of<Transform>())
                {
                    if (value == null)
                    {
                        value = new UdonGameObjectComponentHeapReference(declaredType);
                        declaredType = Il2CppType.Of<UdonGameObjectComponentHeapReference>();
                    }
                }

                heap.SetHeapVariable(symbolAddress, value, declaredType);
            }

            // Let UdonManager apply any processing or scans.
            __instance._udonManager.ProcessUdonProgram(__instance._program);

            __instance.ResolveUdonHeapReferences(program.SymbolTable, program.Heap);
            var vm = new FakeUdonVM();

            /* Prevent vm from being garbage collected */
            Objects.Add(vm);

            __instance._udonVM = new IUdonVM(vm.Pointer);
            __instance._udonVM.LoadProgram(__instance._program);
            __instance.ProcessEntryPoints();
            __instance._isReady = true;
            __instance._initialized = true;
            __instance.RunOnInit();

            return false;
        }

        private readonly static Dictionary<int, UdonSharp.UdonSharpBehaviour> ActiveBehaviours = new();
        private readonly static Il2CppSystem.Collections.Generic.List<Il2CppSystem.Object> Objects = new();
        public static UdonSharp.UdonSharpBehaviour GetOrCreateBehaviour(UdonBehaviour node, System.Type type)
        {
            if (!ActiveBehaviours.TryGetValue(node.GetInstanceID(), out var behaviour))
            {
                behaviour = (UdonSharp.UdonSharpBehaviour)System.Activator.CreateInstance(type);
                behaviour.Initialize(node, type);
                ActiveBehaviours.Add(node.GetInstanceID(), behaviour);
            }
            return behaviour;
        }

        public static void ClearBehaviours()
        {
            ActiveBehaviours.Clear();
            Objects.Clear();
        }

        public static Il2CppSystem.Type FromFieldType(Type type)
        {
            if (type.IsArray)
            {
                var baseType = type.GetElementType();
                return UnhollowerRuntimeLib.Il2CppType.From((baseType.IsValueType ? typeof(Il2CppStructArray<>) : typeof(Il2CppReferenceArray<>)).MakeGenericType(baseType));
            }
            else if (type.BaseType == typeof(UdonSharp.UdonSharpBehaviour))
            {
                return UnhollowerRuntimeLib.Il2CppType.Of<UdonBehaviour>();
            }
            else
            {
                return UnhollowerRuntimeLib.Il2CppType.From(type);
            }
        }

        private static readonly Dictionary<string, string> EventTable = new Dictionary<string, string>
        {
            ["Custom"] = "_custom",
            ["OnDataStorageAdded"] = "_onDataStorageAdded",
            ["OnDataStorageChanged"] = "_onDataStorageChanged",
            ["OnDataStorageRemoved"] = "_onDataStorageRemoved",
            ["OnDrop"] = "_onDrop",
            ["Interact"] = "_interact",
            ["OnNetworkReady"] = "_onNetworkReady",
            ["OnOwnershipTransferred"] = "_onOwnershipTransferred",
            ["OnPickup"] = "_onPickup",
            ["OnPickupUseDown"] = "_onPickupUseDown",
            ["OnPickupUseUp"] = "_onPickupUseUp",
            ["OnPlayerJoined"] = "_onPlayerJoined",
            ["OnPlayerLeft"] = "_onPlayerLeft",
            ["OnSpawn"] = "_onSpawn",
            ["OnStationEntered"] = "_onStationEntered",
            ["OnStationExited"] = "_onStationExited",
            ["OnVideoEnd"] = "_onVideoEnd",
            ["OnVideoPause"] = "_onVideoPause",
            ["OnVideoPlay"] = "_onVideoPlay",
            ["OnVideoStart"] = "_onVideoStart",
            ["MidiNoteOn"] = "_midiNoteOn",
            ["MidiNoteOff"] = "_midiNoteOff",
            ["MidiControlChange"] = "_midiControlChange",

            ["Start"] = "_start",
            ["Update"] = "_update",
            ["FixedUpdate"] = "_fixedUpdate",
            ["LateUpdate"] = "_lateUpdate",
            ["PostLateUpdate"] = "_postLateUpdate",
            ["OnAnimatorIK"] = "_onAnimatorIK",
            ["OnAnimatorMove"] = "_onAnimatorMove",
            ["OnCollisionEnter"] = "_onCollisionEnter",
            ["OnCollisionEnter2D"] = "_onCollisionEnter2D",
            ["OnCollisionExit"] = "_onCollisionExit",
            ["OnCollisionExit2D"] = "_onCollisionExit2D",
            ["OnCollisionStay"] = "_onCollisionStay",
            ["OnCollisionStay2D"] = "_onCollisionStay2D",
            ["OnControllerColliderHit"] = "_onControllerColliderHit",
            ["OnDestroy"] = "_onDestroy",
            ["OnDisable"] = "_onDisable",
            ["OnEnable"] = "_onEnable",
            ["OnJointBreak"] = "_onJointBreak",
            ["OnJointBreak2D"] = "_onJointBreak2D",
            ["OnMouseDown"] = "_onMouseDown",
            ["OnMouseDrag"] = "_onMouseDrag",
            ["OnMouseEnter"] = "_onMouseEnter",
            ["OnMouseExit"] = "_onMouseExit",
            ["OnMouseOver"] = "_onMouseOver",
            ["OnMouseUp"] = "_onMouseUp",
            ["OnMouseUpAsButton"] = "_onMouseUpAsButton",
            ["OnParticleCollision"] = "_onParticleCollision",
            ["OnParticleTrigger"] = "_onParticleTrigger",
            ["OnPostRender"] = "_onPostRender",
            ["OnPreCull"] = "_onPreCull",
            ["OnPreRender"] = "_onPreRender",
            ["OnRenderImage"] = "_onRenderImage",
            ["OnRenderObject"] = "_onRenderObject",
            ["OnTransformChildrenChanged"] = "_onTransformChildrenChanged",
            ["OnTransformParentChanged"] = "_onTransformParentChanged",
            ["OnTriggerEnter"] = "_onTriggerEnter",
            ["OnTriggerEnter2D"] = "_onTriggerEnter2D",
            ["OnTriggerExit"] = "_onTriggerExit",
            ["OnTriggerExit2D"] = "_onTriggerExit2D",
            ["OnTriggerStay"] = "_onTriggerStay",
            ["OnTriggerStay2D"] = "_onTriggerStay2D",
            ["OnWillRenderObject"] = "_onWillRenderObject",
            ["InputJump"] = "_inputJump",
            ["InputUse"] = "_inputUse",
            ["InputGrab"] = "_inputGrab",
            ["InputDrop"] = "_inputDrop",
            ["InputMoveVertical"] = "_inputMoveVertical",
            ["InputMoveHorizontal"] = "_inputMoveHorizontal",
            ["InputLookVertical"] = "_inputLookVertical",
            ["InputLookHorizontal"] = "_inputLookHorizontal"
        };
    }
}
