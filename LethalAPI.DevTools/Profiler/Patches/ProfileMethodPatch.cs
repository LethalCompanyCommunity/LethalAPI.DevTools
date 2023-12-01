// -----------------------------------------------------------------------
// <copyright file="ProfileMethodPatch.cs" company="LethalAPI Modding Community">
// Copyright (c) LethalAPI Modding Community. All rights reserved.
// Licensed under the LGPL-3.0 license.
// </copyright>
// Credit to both O5Zereth and Mitzey for the incredible work on the profiler.
// View the project here: https://github.com/mitzey234/nwapi-profiler.
// Original project licensed under MIT.
// -----------------------------------------------------------------------

// ReSharper disable MemberCanBePrivate.Global
#pragma warning disable SA1401 // Field should be private
namespace LethalAPI.DevTools.Profiler.Patches;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using API;
using Extensions;
using HarmonyLib;

using static HarmonyLib.AccessTools;

/// <summary>
/// Provides patching capabilites to profile methods..
/// </summary>
public static class ProfileMethodPatch
{
    /// <summary>
    /// Gets or sets a value indicating whether indicates whether the profiler is disabled.
    /// </summary>
    public static bool DisableProfiler = false;

    /// <summary>
    /// The maximum number of patches to profile.
    /// </summary>
    private const int MaxPatches = 8000;

    private static readonly HarmonyMethod ProfilerTranspiler = new(typeof(ProfileMethodPatch), nameof(Transpiler));

    private static ProfiledMethodInfo[] profilerInfos = new ProfiledMethodInfo[MaxPatches];

    private static HashSet<int>? optimizedMethods;

    /// <summary>
    /// Tracks a method and applies the profiling methods.
    /// </summary>
    /// <param name="instructions">The instructions of the method.</param>
    /// <param name="method">The method being patched.</param>
    /// <param name="generator">The ILGenerator being used.</param>
    /// <returns>The modified code instructions.</returns>
    internal static IEnumerable<CodeInstruction> AddMethodAndApplyProfiler(this IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    {
        ProfiledMethodsTracker.AddMethod(method);

        return Transpiler(instructions, method, generator);
    }

    /// <summary>
    /// Gets profiled method information.
    /// </summary>
    /// <returns>A enumerable containing information of the profiled methods.</returns>
    internal static IEnumerable<AsRef<ProfiledMethodInfo>> GetProfilerInfos()
    {
        int count = ProfiledMethodsTracker.PatchedCount;

        for (int i = 0; i < count; i++)
        {
            yield return new(ref profilerInfos[i]);
        }
    }

    /// <summary>
    /// Applies profiling to a method.
    /// </summary>
    /// <param name="method">The method to apply profiling to.</param>
    /// <exception cref="Exception">Thrown when the method cannot be profiled.</exception>
    /// <exception cref="ArgumentException">Thrown when the method is invalid.</exception>
    internal static void ApplyProfiler(MethodBase method)
    {
        if (!ProfiledMethodsTracker.AddMethod(method))
        {
            throw new Exception("Failed to add method.");
        }

        optimizedMethods ??= new(Manager.HarmonyOptimizations.GetPatchedMethods().Select(x => x.GetHashCode()).Distinct());

        if (optimizedMethods.Contains(method.GetHashCode()))
        {
            return;
        }

        if (method.GetMethodBody() == null)
        {
            throw new ArgumentException("Cannot patch a method without a body.");
        }

        if (method.DeclaringType?.IsGenericType ?? false)
        {
            if (method.DeclaringType.ContainsGenericParameters)
            {
                throw new ArgumentException($"Cannot patch method with a generic declaring type that is not constructed.");
            }
        }

        Type? baseType = method.DeclaringType?.BaseType;

        while (baseType != null)
        {
            if (baseType is { IsGenericType: true, ContainsGenericParameters: true })
            {
                throw new ArgumentException($"Cannot patch generic method within a declaring type deriving from a generic type that is not constructed.");
            }

            baseType = baseType.BaseType;
        }

        Manager.Harmony.Patch(method, transpiler: ProfilerTranspiler);
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    {
        if (!ProfiledMethodsTracker.GetMethodIndex(method, out int methodIndex))
        {
            Log.Error("Could not locate method index?");
            return instructions;
        }

        instructions.BeginTranspiler(out List<CodeInstruction> newInstructions);

        // This is a method that does not return.
        // Likely throws an exception.
        if (newInstructions.All(x => x.opcode != OpCodes.Ret))
            return newInstructions.FinishTranspiler();

        // long startTimestamp;
        LocalBuilder startTimestamp = generator.DeclareLocal(typeof(long));
        LocalBuilder startMemory = generator.DeclareLocal(typeof(long));

        Label profilerDisabledLabel = generator.DefineLabel();

        newInstructions[0].labels.Add(profilerDisabledLabel);

        // We get the starting timestamp recorded by the timer mechanism
        // and store it into a variable we can use later.
        newInstructions.InsertRange(0, new CodeInstruction[]
        {
            // if (!ProfileMethodPatch.DisableProfiler)
            // {
            //     long startTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
            //     long startMemory = MonoNative.mono_gc_get_used_size();
            // }
            new(OpCodes.Ldsfld, Field(typeof(ProfileMethodPatch), nameof(DisableProfiler))),
            new(OpCodes.Brtrue_S, profilerDisabledLabel),

            new(OpCodes.Call, Method(typeof(Stopwatch), nameof(Stopwatch.GetTimestamp))),
            new(OpCodes.Stloc_S, startTimestamp),

            new(OpCodes.Call, Method(typeof(MonoNative), nameof(MonoNative.mono_gc_get_used_size))),
            new(OpCodes.Stloc_S, startMemory),
        });

        int index = newInstructions.FindLastIndex(x => x.opcode == OpCodes.Ret);

        Label justReturn = generator.DefineLabel();
        Label doProfilerCheck = generator.DefineLabel();
        Label shouldAssign = generator.DefineLabel();
        List<Label> originalReturnLabels = newInstructions[index].ExtractLabels();

        Label memoryGreaterOrEqualZero = generator.DefineLabel();
        Label memoryCheck = generator.DefineLabel();

        newInstructions[index].labels.Add(justReturn);

        // if (!ProfileMethodPatch.DisableProfiler)
        // {
        //     long totalTicks = Stopwatch.GetTimestamp() - startTimestamp;
        //     long totalMemory = MonoNative.mono_gc_get_used_size() - startMemory;
        //
        //     ref ProfiledMethodInfo profilerInfo = ref ProfileMethodPatch.ProfilerInfos[methodIndex];
        //
        //     profilerInfo.InvocationCount++;
        //     profilerInfo.TotalTicks += (uint)totalTicks;
        //
        //     if (profilerInfo.MaxTicks < (uint)totalTicks)
        //     {
        //         profilerInfo.MaxTicks = (uint)totalTicks;
        //     }
        //     if (totalMemory >= 0)
        //     {
        //         profilerInfo.MemoryAllocated += totalMemory;
        //     }
        // }
        newInstructions.InsertRange(index, new[]
        {
            new CodeInstruction(OpCodes.Ldsfld, Field(typeof(ProfileMethodPatch), nameof(DisableProfiler)))
                .WithLabels(doProfilerCheck).WithLabels(originalReturnLabels),
            new(OpCodes.Brtrue_S, justReturn),

            // long totalTicks = Stopwatch.GetTimestamp() - startTimestamp;
            new(OpCodes.Call, Method(typeof(Stopwatch), nameof(Stopwatch.GetTimestamp))),
            new(OpCodes.Ldloc_S, startTimestamp),
            new(OpCodes.Sub),
            new(OpCodes.Stloc_S, startTimestamp),

            // long totalMemory = MonoNative.mono_gc_get_used_size() - startMemory;
            new(OpCodes.Call, Method(typeof(MonoNative), nameof(MonoNative.mono_gc_get_used_size))),
            new(OpCodes.Ldloc_S, startMemory),
            new(OpCodes.Sub),
            new(OpCodes.Stloc_S, startMemory),

            // ref ProfiledMethodInfo profilerInfo = ref ProfileMethodPatch.ProfilerInfos[methodIndex];
            new(OpCodes.Ldsfld, Field(typeof(ProfileMethodPatch), nameof(profilerInfos))),
            new(OpCodes.Ldc_I4, methodIndex),
            new(OpCodes.Ldelema, typeof(ProfiledMethodInfo)),
            new(OpCodes.Dup),

            // profilerInfo.InvocationCount++;
            new(OpCodes.Ldflda, Field(typeof(ProfiledMethodInfo), nameof(ProfiledMethodInfo.InvocationCount))),
            new(OpCodes.Dup),
            new(OpCodes.Ldind_U4),
            new(OpCodes.Ldc_I4_1),
            new(OpCodes.Add),
            new(OpCodes.Stind_I4),

            // profilerInfo.TotalTicks += (uint)totalTicks;
            new(OpCodes.Dup),
            new(OpCodes.Ldflda, Field(typeof(ProfiledMethodInfo), nameof(ProfiledMethodInfo.TotalTicks))),
            new(OpCodes.Dup),
            new(OpCodes.Ldind_U4),
            new(OpCodes.Ldloc_S, startTimestamp),
            new(OpCodes.Conv_U4),
            new(OpCodes.Add),
            new(OpCodes.Stind_I4),

            // if (profilerInfo.MaxTicks < (uint)totalTicks)
            // {
            //     profilerInfo.MaxTicks = (uint)totalTicks;
            // }
            new(OpCodes.Dup),
            new(OpCodes.Ldflda, Field(typeof(ProfiledMethodInfo), nameof(ProfiledMethodInfo.MaxTicks))), // [uint&] MaxTicks
            new(OpCodes.Dup), // [uint&] MaxTicks | [uint&] MaxTicks
            new(OpCodes.Ldind_U4), // [uint&] MaxTicks | [uint] MaxTicks
            new(OpCodes.Ldloc_S, startTimestamp), // [uint&] MaxTicks | [uint] MaxTicks | [long] total
            new(OpCodes.Conv_U4), // [uint&] MaxTicks | [uint] MaxTicks | [uint] total
            new(OpCodes.Blt_Un_S, shouldAssign), // [uint&] MaxTicks
            new(OpCodes.Pop),
            new(OpCodes.Br, memoryCheck),
            new CodeInstruction(OpCodes.Ldloc_S, startTimestamp).WithLabels(shouldAssign), // [uint&] MaxTicks | [long] total
            new(OpCodes.Conv_U4), // [uint&] MaxTicks | [uint] total
            new(OpCodes.Stind_I4),

            // if (totalMemory >= 0)
            // {
            //     profilerInfo.MemoryAllocated += totalMemory;
            // }
            new CodeInstruction(OpCodes.Ldloc_S, startMemory).WithLabels(memoryCheck),
            new(OpCodes.Conv_U4),
            new(OpCodes.Ldc_I4_0),
            new(OpCodes.Conv_U4),
            new(OpCodes.Bge_S, memoryGreaterOrEqualZero),
            new(OpCodes.Pop),
            new(OpCodes.Ret, justReturn),

            new CodeInstruction(OpCodes.Ldflda, Field(typeof(ProfiledMethodInfo), nameof(ProfiledMethodInfo.TotalMemory))).WithLabels(memoryGreaterOrEqualZero), // [uint&] MemoryAllocated
            new(OpCodes.Dup), // [uint&] MemoryAllocated | [uint&] MemoryAllocated
            new(OpCodes.Ldind_U4), // [uint&] MemoryAllocated | [uint] MemoryAllocated
            new(OpCodes.Ldloc_S, startMemory), // [uint&] MemoryAllocated | [uint] MemoryAllocated | [long] totalMemory
            new(OpCodes.Conv_U4), // [uint&] MemoryAllocated | [uint] MemoryAllocated | [uint] totalMemory
            new(OpCodes.Add), // [uint&] MemoryAllocated | [uint] MemoryAllocated + totalMemory
            new(OpCodes.Stind_I4),
        });

        CodeInstruction profilerCheckBegin = newInstructions[index];

        index = newInstructions.FindLastIndex(index, x => x.opcode == OpCodes.Ret);

        while (index != -1)
        {
            CodeInstruction instruction = newInstructions[index];

            instruction.opcode = OpCodes.Br;
            instruction.operand = doProfilerCheck;
            instruction.MoveLabelsTo(profilerCheckBegin);

            index = newInstructions.FindLastIndex(index, x => x.opcode == OpCodes.Ret);
        }

        return newInstructions.FinishTranspiler();
    }

    /// <summary>
    /// A struct for containing profiler info.
    /// </summary>
    /// <remarks><b>Do not create instances of this struct, or you will encounter problems.</b></remarks>
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi, Pack = 1, Size = MySize)]
    public struct ProfiledMethodInfo
    {
        /// <summary>
        /// Gets the total number of invocations by the method associated with this instance.
        /// </summary>
        [FieldOffset(0)]
        public uint InvocationCount;

        /// <summary>
        /// Gets the total ticks taken to execute by the method associated with this instance over all invocations.
        /// </summary>
        [FieldOffset(4)]
        public uint TotalTicks;

        /// <summary>
        /// Gets the maximum tick count generated by the method associated with this instance.
        /// </summary>
        [FieldOffset(8)]
        public uint MaxTicks;

        /// <summary>
        /// Gets the total number of memory (in bytes) that has been allocated by the method associated with this instance.
        /// </summary>
        [FieldOffset(12)]
        public uint TotalMemory;

        /// <summary>
        /// Gets the size of the struct.
        /// </summary>
        private const int MySize = 4 + 4 + 4 + 4;

        /// <summary>
        /// Gets the method associated with this instance.
        /// </summary>
        /// <remarks>
        /// <b>NEVER call this method unless you are using the struct by reference.</b>
        /// <code>ref ProfileMethodPatch.ProfiledMethodInfo info = ref ProfileMethodPatch.ProfilerInfos[someIndex];</code>
        /// </remarks>
        public readonly string GetMyMethod
        {
            get
            {
                IntPtr byteOffset = Unsafe.ByteOffset(ref profilerInfos[0], ref Unsafe.AsRef(this));
                int myIndex = byteOffset.ToInt32() / MySize;

                ProfiledMethodsTracker.GetMethod(myIndex, out string result);
                return result;
            }
        }

        /// <summary>
        /// Gets the average tick count for the method associated with this instance.
        /// </summary>
        public readonly uint AvgTicks => TotalTicks / Math.Max(1, InvocationCount);

        /// <summary>
        /// Gets the average memory allocated (in bytes) for the method associated with this instance.
        /// </summary>
        public readonly uint AvgMemory => TotalMemory / Math.Max(1, InvocationCount);
    }

    /// <summary>
    /// Contains information about the profiled methods.
    /// </summary>
    public static class ProfiledMethodsTracker
    {
        private static readonly Dictionary<int, int> Patched = new(MaxPatches);

        private static readonly Dictionary<int, string> ByIndex = new(MaxPatches);

        private static volatile int patchedCount;

        /// <summary>
        /// Gets the number of profiled methods.
        /// </summary>
        public static int PatchedCount => patchedCount;

        /// <summary>
        /// Attempts to profile a method.
        /// </summary>
        /// <param name="method">The method to profile.</param>
        /// <returns>True if the method was profiled successfully, False if the method cannot be profiled.</returns>
        public static bool AddMethod(MethodBase method)
        {
            if (patchedCount == MaxPatches)
                return false;

            if (Patched.ContainsKey(method.GetHashCode()))
                return false;

            int index = Interlocked.Exchange(ref patchedCount, patchedCount + 1);

            Patched.Add(method.GetHashCode(), index);
            ByIndex.Add(index, string.Concat(method.DeclaringType?.FullName, ".", method.Name));
            return true;
        }

        /// <summary>
        /// Gets the index of the method.
        /// </summary>
        /// <param name="method">The method to find.</param>
        /// <param name="index">The index result.</param>
        /// <returns>True if the index was found, False if the index could not be found.</returns>
        public static bool GetMethodIndex(MethodBase method, out int index)
        {
            return Patched.TryGetValue(method.GetHashCode(), out index);
        }

        /// <summary>
        /// Gets the method at an index.
        /// </summary>
        /// <param name="index">The index to search.</param>
        /// <param name="method">The method name.</param>
        /// <returns>True if the method was found, False if the method could not be found.</returns>
        public static bool GetMethod(int index, out string method)
        {
            return ByIndex.TryGetValue(index, out method);
        }
    }
}
