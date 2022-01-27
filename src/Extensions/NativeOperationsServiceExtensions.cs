using System;
using Leopotam.EcsLite;
using OdinGames.EcsLite.Native.NativeOperationsWrapper;
using OdinGames.EcsLite.Native.NativeOperationsWrapper.Base;

namespace OdinGames.EcsLite.Native.Extensions
{
    public static class NativeOperationsServiceExtensions
    {
        public static bool GetResetMethod<T>(INativeOperationsWrapper<T> wrapper) where T : unmanaged
        {
            var isAutoReset = typeof (IEcsAutoReset<T>).IsAssignableFrom (typeof(T));

            if (!isAutoReset) 
                return false; 
            
            var autoResetMethod = typeof(T).GetMethod(nameof(IEcsAutoReset<T>.AutoReset));

            var autoResetDelegate = (NativeReadWriteOperationsWrapper<T>.AutoResetDelegate)Delegate
                .CreateDelegate(typeof(NativeReadWriteOperationsWrapper<T>.AutoResetDelegate),
                    null,
                    autoResetMethod);
            
            //wrapper.ResetDelegate = autoResetDelegate;

           // wrapper.NativeResetDelegate =
             //   new FunctionPointer<INativeOperationsWrapper<T>.AutoResetDelegate>(Marshal.GetFunctionPointerForDelegate(wrapper.ResetDelegate));

            return true;
        }
    }
}