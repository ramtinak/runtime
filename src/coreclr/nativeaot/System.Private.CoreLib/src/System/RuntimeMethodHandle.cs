// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

using Internal.Reflection.Augments;
using Internal.Runtime.Augments;
using Internal.Runtime.CompilerServices;

namespace System
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RuntimeMethodHandle : IEquatable<RuntimeMethodHandle>, ISerializable
    {
        private IntPtr _value;

        private RuntimeMethodHandle(IntPtr value)
        {
            _value = value;
        }

        public IntPtr Value => _value;

        public override bool Equals(object? obj)
        {
            if (!(obj is RuntimeMethodHandle))
                return false;

            return Equals((RuntimeMethodHandle)obj);
        }

        public bool Equals(RuntimeMethodHandle handle)
        {
            if (_value == handle._value)
                return true;

            if (_value == IntPtr.Zero || handle._value == IntPtr.Zero)
                return false;

            RuntimeTypeHandle declaringType1, declaringType2;
            MethodNameAndSignature nameAndSignature1, nameAndSignature2;
            RuntimeTypeHandle[] genericArgs1, genericArgs2;

            RuntimeAugments.TypeLoaderCallbacks.GetRuntimeMethodHandleComponents(this, out declaringType1, out nameAndSignature1, out genericArgs1);
            RuntimeAugments.TypeLoaderCallbacks.GetRuntimeMethodHandleComponents(handle, out declaringType2, out nameAndSignature2, out genericArgs2);

            if (!declaringType1.Equals(declaringType2))
                return false;
            if (!nameAndSignature1.Equals(nameAndSignature2))
                return false;
            if ((genericArgs1 == null && genericArgs2 != null) || (genericArgs1 != null && genericArgs2 == null))
                return false;
            if (genericArgs1 != null)
            {
                if (genericArgs1.Length != genericArgs2!.Length)
                    return false;
                for (int i = 0; i < genericArgs1.Length; i++)
                {
                    if (!genericArgs1[i].Equals(genericArgs2![i]))
                        return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            if (_value == IntPtr.Zero)
                return 0;

            RuntimeTypeHandle declaringType;
            MethodNameAndSignature nameAndSignature;
            RuntimeTypeHandle[] genericArgs;
            RuntimeAugments.TypeLoaderCallbacks.GetRuntimeMethodHandleComponents(this, out declaringType, out nameAndSignature, out genericArgs);

            int hashcode = declaringType.GetHashCode();
            hashcode = (hashcode + int.RotateLeft(hashcode, 13)) ^ nameAndSignature.Name.GetHashCode();
            if (genericArgs != null)
            {
                for (int i = 0; i < genericArgs.Length; i++)
                {
                    int argumentHashCode = genericArgs[i].GetHashCode();
                    hashcode = (hashcode + int.RotateLeft(hashcode, 13)) ^ argumentHashCode;
                }
            }

            return hashcode;
        }

        public static RuntimeMethodHandle FromIntPtr(IntPtr value) => new RuntimeMethodHandle(value);

        public static IntPtr ToIntPtr(RuntimeMethodHandle value) => value.Value;

        public static bool operator ==(RuntimeMethodHandle left, RuntimeMethodHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RuntimeMethodHandle left, RuntimeMethodHandle right)
        {
            return !left.Equals(right);
        }

        public IntPtr GetFunctionPointer()
        {
            RuntimeTypeHandle declaringType;
            RuntimeAugments.TypeLoaderCallbacks.GetRuntimeMethodHandleComponents(this, out declaringType, out _, out _);

            return ReflectionAugments.GetFunctionPointer(this, declaringType);
        }

        [Obsolete(Obsoletions.LegacyFormatterImplMessage, DiagnosticId = Obsoletions.LegacyFormatterImplDiagId, UrlFormat = Obsoletions.SharedUrlFormat)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
