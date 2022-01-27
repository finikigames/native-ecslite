using OdinGames.EcsLite.Native.WrappedData;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace OdinGames.EcsLite.Native.Extensions
{
    public static class WrappedDataExtensions
    {
        public static unsafe NativeWrappedData<T> WrapToNative<T> (this T[] managedData) where T : unmanaged {
            fixed (void* ptr = managedData) {
#if UNITY_EDITOR
                var nativeData = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T> (ptr, managedData.Length, Allocator.None);
                var sh = AtomicSafetyHandle.Create ();
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle (ref nativeData, sh);
                return new NativeWrappedData<T> { Array = nativeData, SafetyHandle = sh };
#else
                return new NativeWrappedData<T> { Array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T> (ptr, managedData.Length, Allocator.None) };
#endif
            }
        }

        public static ReadOnlyNativeWrappedData<T> ToReadOnly<T>(this NativeWrappedData<T> wrappedData)
            where T : unmanaged
        {
            ReadOnlyNativeWrappedData<T> readOnlyNativeWrappedData;

            readOnlyNativeWrappedData.Array = wrappedData.Array.AsReadOnly();
#if UNITY_EDITOR
            readOnlyNativeWrappedData.SafetyHandle = wrappedData.SafetyHandle;
#endif

            return readOnlyNativeWrappedData;
        }

        public static unsafe ReadOnlyNativeWrappedData<T> WrapToReadOnlyNative<T> (this T[] managedData) where T : unmanaged {
            fixed (void* ptr = managedData) {
#if UNITY_EDITOR
                var nativeData = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T> (ptr, managedData.Length, Allocator.None);
                var sh = AtomicSafetyHandle.Create ();
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle (ref nativeData, sh);
                return new ReadOnlyNativeWrappedData<T> { Array = nativeData.AsReadOnly(), SafetyHandle = sh };
#else
                return new ReadOnlyNativeWrappedData<T> { Array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T> (ptr, managedData.Length, Allocator.None).AsReadOnly() };
#endif
            }
        }
        
        public static void UnwrapFromNative<T1> (this ReadOnlyNativeWrappedData<T1> sh) where T1 : unmanaged {
#if UNITY_EDITOR
            AtomicSafetyHandle.CheckDeallocateAndThrow (sh.SafetyHandle);
            AtomicSafetyHandle.Release (sh.SafetyHandle);
#endif
        }
        
        public static void UnwrapFromNative<T1> (this NativeWrappedData<T1> sh) where T1 : unmanaged {
#if UNITY_EDITOR
            AtomicSafetyHandle.CheckDeallocateAndThrow (sh.SafetyHandle);
            AtomicSafetyHandle.Release (sh.SafetyHandle);
#endif
        }
    }
}