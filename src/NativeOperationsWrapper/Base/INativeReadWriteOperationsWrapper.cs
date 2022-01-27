using System;
using Unity.Collections;

namespace OdinGames.EcsLite.Native.NativeOperationsWrapper.Base
{
    public interface INativeReadWriteOperationsWrapper : IDisposable
    {
        NativeHashMap<int, int> DeleteCache { get; set; }
        NativeHashMap<int, int> AddCache { get; set; }
        NativeHashSet<int> EntitiesToRemove { get; set; }
    }
}