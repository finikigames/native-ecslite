using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace OdinGames.EcsLite.Native.Extensions
{
    static unsafe class UnsafeExtensions
    {
        internal static ref T GetRef<T>(this NativeArray<T> array, int index)
            where T : struct
        {
            if (index < 0 || index >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            unsafe
            {
                return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafePtr(), index);
            }
        }
        
        internal readonly struct SharedIndex : IDisposable
        {
            [NativeDisableUnsafePtrRestriction] 
            private readonly int* _index;

            private readonly Allocator _allocator;

            public ref int Value => ref *_index;

            // Use this if you want a Job shared index that value can be safely changed between threads
            public SharedIndex(Allocator allocator = Allocator.TempJob, int startValue = 0)
            {
                _index = Alloc(allocator, startValue);
                _allocator = allocator;
            }

            // Use this if you want to create Shared index from existing variable
            // For example, if you have Managed and Native API and share some variables over them
            public SharedIndex(ref int index)
            {
                fixed (int* indexPtr = &index)
                {
                    _index = indexPtr;
                }

                _allocator = Allocator.None;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int PostIncrement()
            {
                var oldValue = *_index;
                System.Threading.Interlocked.Increment(ref *_index);
                return oldValue;
            }

            public static SharedIndex operator ++(SharedIndex index)
            {
                index.PostIncrement();
                return index;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref int PreIncrement()
            {
                System.Threading.Interlocked.Increment(ref *_index);
                return ref Value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref int PreDecrement()
            {
                System.Threading.Interlocked.Decrement(ref *_index);
                return ref Value;
            }

            public void Dispose()
            {
                if (_allocator != Allocator.None)
                    Free(_index, _allocator);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Alloc<T>(Allocator allocator, T defaultValue = default)
            where T : unmanaged
        {
            T* allocated = (T*) UnsafeUtility.Malloc(
                sizeof(T), sizeof(T), allocator);

            *allocated = defaultValue;

            return allocated;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free<T>(T* pointer, Allocator allocator)
            where T : unmanaged
        {
            UnsafeUtility.Free(pointer, allocator);
        }
    }
}