using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace OdinGames.EcsLite.Native.WrappedData
{
    public struct ReadOnlyNativeWrappedData<T> where T : unmanaged
    {
        [ReadOnly]
        public NativeArray<T>.ReadOnly Array;
#if UNITY_EDITOR
        public AtomicSafetyHandle SafetyHandle;
#endif
    }
}