using UnityEngine;
using Il2CppVRC.SDKBase;
using Il2CppVRC.Udon;
using Il2CppVRC.Udon.Common.Interfaces;
using Il2CppVRC.Udon.Common.Enums;
using Il2CppVRC.Udon.Common;
using Il2CppVRC.SDK3.Components.Video;
using static UdonSharp.Internal.UdonSharpInternalUtility;

namespace UdonSharp
{
    public class UdonSharpBehaviour
    {
        public long __refl_const_intnl_udonTypeID;
        public long __refl_typeid;
        public string __refl_const_intnl_udonTypeName;
        public string __refl_typename;
        public Transform transform;
        public GameObject gameObject;
        public UdonBehaviour behaviour;
        public void Initialize(UdonBehaviour _behaviour, System.Type type)
        {
            gameObject = _behaviour.gameObject;
            transform = _behaviour.transform;
            behaviour = _behaviour;
            __refl_typeid = __refl_const_intnl_udonTypeID = GetTypeID(type);
            __refl_typename = __refl_const_intnl_udonTypeName = GetTypeName(type);
        }
        // Stubs for the UdonBehaviour functions that emulate Udon behavior
        public object GetProgramVariable(string name)
            => behaviour.GetProgramVariable(name);

        public void SetProgramVariable(string name, object value)
            => behaviour.SetProgramVariable(name, value);

        public void SendCustomEvent(string eventName)
            => behaviour.SendCustomEvent(eventName);

        public void SendCustomNetworkEvent(NetworkEventTarget target, string eventName)
            => behaviour.SendCustomNetworkEvent(target, eventName);

        /// <summary>
        /// Executes target event after delaySeconds. If 0.0 delaySeconds is specified, will execute the following frame
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="delaySeconds"></param>
        /// <param name="eventTiming"></param>
        public void SendCustomEventDelayedSeconds(string eventName, float delaySeconds, EventTiming eventTiming = EventTiming.Update)
            => behaviour.SendCustomEventDelayedSeconds(eventName, delaySeconds, eventTiming);

        /// <summary>
        /// Executes target event after delayFrames have passed. If 0 frames is specified, will execute the following frame. In effect 0 frame delay and 1 fame delay are the same on this method.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="delayFrames"></param>
        /// <param name="eventTiming"></param>
        public void SendCustomEventDelayedFrames(string eventName, int delayFrames, EventTiming eventTiming = EventTiming.Update)
            => behaviour.SendCustomEventDelayedFrames(eventName, delayFrames, eventTiming);

        // Method stubs for auto completion
        public virtual void PostLateUpdate() { }
        public virtual void Interact() { }
        public virtual void OnDrop() { }
        public virtual void OnOwnershipTransferred(VRCPlayerApi player) { }
        public virtual void OnPickup() { }
        public virtual void OnPickupUseDown() { }
        public virtual void OnPickupUseUp() { }
        public virtual void OnPlayerJoined(VRCPlayerApi player) { }
        public virtual void OnPlayerLeft(VRCPlayerApi player) { }
        public virtual void OnSpawn() { }
        public virtual void OnStationEntered(VRCPlayerApi player) { }
        public virtual void OnStationExited(VRCPlayerApi player) { }
        public virtual void OnVideoEnd() { }
        public virtual void OnVideoError(VideoError videoError) { }
        public virtual void OnVideoLoop() { }
        public virtual void OnVideoPause() { }
        public virtual void OnVideoPlay() { }
        public virtual void OnVideoReady() { }
        public virtual void OnVideoStart() { }
        public virtual void OnPreSerialization() { }
        public virtual void OnDeserialization() { }
        public virtual void OnPlayerTriggerEnter(VRCPlayerApi player) { }
        public virtual void OnPlayerTriggerExit(VRCPlayerApi player) { }
        public virtual void OnPlayerTriggerStay(VRCPlayerApi player) { }
        public virtual void OnPlayerCollisionEnter(VRCPlayerApi player) { }
        public virtual void OnPlayerCollisionExit(VRCPlayerApi player) { }
        public virtual void OnPlayerCollisionStay(VRCPlayerApi player) { }
        public virtual void OnPlayerParticleCollision(VRCPlayerApi player) { }
        public virtual void OnPlayerRespawn(VRCPlayerApi player) { }

        public virtual void OnPostSerialization(SerializationResult result) { }
        public virtual bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner) => true;

        public virtual void MidiNoteOn(int channel, int number, int velocity) { }
        public virtual void MidiNoteOff(int channel, int number, int velocity) { }
        public virtual void MidiControlChange(int channel, int number, int value) { }

        public virtual void InputJump(bool value, UdonInputEventArgs args) { }
        public virtual void InputUse(bool value, UdonInputEventArgs args) { }
        public virtual void InputGrab(bool value, UdonInputEventArgs args) { }
        public virtual void InputDrop(bool value, UdonInputEventArgs args) { }
        public virtual void InputMoveHorizontal(float value, UdonInputEventArgs args) { }
        public virtual void InputMoveVertical(float value, UdonInputEventArgs args) { }
        public virtual void InputLookHorizontal(float value, UdonInputEventArgs args) { }
        public virtual void InputLookVertical(float value, UdonInputEventArgs args) { }
    }
}
