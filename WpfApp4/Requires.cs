using System;
using System.Collections.Generic;
using System.Diagnostics;

#if JETBRAINS_ANNOTATIONS
using JetBrains.Annotations;
#endif

namespace WpfApp4
{
    public static class Requires
    {

        [DebuggerStepThrough]
#if JETBRAINS_ANNOTATIONS
        [ContractAnnotation("argument:null => halt")]
#endif
        public static void NotNull<T>(
#if JETBRAINS_ANNOTATIONS
            [NoEnumeration]
#endif
            T argument, string argumentName = null) where T : class
        {
            if (argument == null)
                throw new ArgumentNullException(argumentName);
        }
        [DebuggerStepThrough]
        public static void NotDefault<T>(T argument, string argumentName = null) where T : struct
        {
            if (argument.Equals(default(T)))
                throw new ArgumentException(argumentName);
        }

        [DebuggerStepThrough]
#if JETBRAINS_ANNOTATIONS
        [ContractAnnotation("condition:false => halt")]
#endif
        public static void Argument(bool condition, string message = null)
        {
            if (!condition)
                throw new ArgumentException(message);
        }

        [DebuggerStepThrough]
#if JETBRAINS_ANNOTATIONS
        [ContractAnnotation("collection:null => halt")]
#endif
        public static void Empty<T>(ICollection<T> collection, string argumentName = null)
        {
            if (collection.Count > 0)
                throw new ArgumentException(argumentName);
        }
        [DebuggerStepThrough]
#if JETBRAINS_ANNOTATIONS
        [ContractAnnotation("collection:null => halt")]
#endif
        public static void NotEmpty<T>(ICollection<T> collection, string argumentName = null)
        {
            if (collection.Count == 0)
                throw new ArgumentException(argumentName);
        }
        [DebuggerStepThrough]
#if JETBRAINS_ANNOTATIONS
        [ContractAnnotation("condition:false => halt")]
#endif
        public static void Range(bool condition, string argumentName = null)
        {
            if (!condition)
                throw new ArgumentOutOfRangeException(argumentName);
        }

        [DebuggerStepThrough]
#if JETBRAINS_ANNOTATIONS
        [ContractAnnotation("argument:null => halt")]
#endif
        public static void NotNullOrWhiteSpace(string argument, string argumentName = null)
        {
            if (string.IsNullOrWhiteSpace(argument))
                throw new ArgumentException(argumentName);
        }

        [DebuggerStepThrough]
        public static void EnumIsDefined<T>(T argument, string argumentName = null) where T : struct
        {
            if (Enum.IsDefined(typeof(T), argument))
                throw new ArgumentException(argumentName);
        }

        [DebuggerStepThrough]
#if JETBRAINS_ANNOTATIONS
        [ContractAnnotation("collection:null => halt")]
#endif
        public static void ForAll<T>(IEnumerable<T> collection, Predicate<T> condition, string argumentName = null)
        {
            foreach (var element in collection)
                if (!condition(element))
                    throw new ArgumentException(argumentName);
        }

        [DebuggerStepThrough]
        public static void NumberIsFinite(double value, string argumentName = null)
        {
            if (!Number.IsFinite(value))
                throw new NotFiniteNumberException(argumentName, value);
        }

        [DebuggerStepThrough]
#if JETBRAINS_ANNOTATIONS
        [ContractAnnotation("condition:false => halt")]
#endif
        public static void State(bool condition, string message = null)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        [DebuggerStepThrough]
#if JETBRAINS_ANNOTATIONS
        [ContractAnnotation("condition:false => halt")]
#endif
        public static void Data(bool condition, string message = null)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}