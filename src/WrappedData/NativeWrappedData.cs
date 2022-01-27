using OdinGames.EcsLite.Native.Extensions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace OdinGames.EcsLite.Native.WrappedData
{
    public struct NativeWrappedData<T> where T : unmanaged
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<T> Array;

        // Is it really needed here?
        public ref T this[int index] => ref Array.GetRef(index);
#if UNITY_EDITOR
        public AtomicSafetyHandle SafetyHandle;
#endif
    }
}