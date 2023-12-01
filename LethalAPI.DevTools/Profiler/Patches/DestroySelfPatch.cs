// -----------------------------------------------------------------------
// <copyright file="DestroySelfPatch.cs" company="LethalAPI Modding Community">
// Copyright (c) LethalAPI Modding Community. All rights reserved.
// Licensed under the LGPL-3.0 license.
// </copyright>
// Credit to both O5Zereth and Mitzey for the incredible work on the profiler.
// View the project here: https://github.com/mitzey234/nwapi-profiler.
// Original project licensed under MIT.
// -----------------------------------------------------------------------

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local
namespace LethalAPI.DevTools.Profiler.Patches;

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;
using UnityEngine;

using static HarmonyLib.AccessTools;

/// <summary>
/// This patch automatically forces objects to destroy themselves.
/// </summary>
/// <remarks>Note that the <see cref="GameObject"/> any component is attached to will not be destroyed, instead the component itself will.</remarks>
[HarmonyPatch]
public static class DestroySelfPatch
{
    private static IEnumerable<MethodInfo> TargetMethods()
    {
        // yield return Method(typeof(DecalProjector), "Awake");
        yield break;
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    {
        return new CodeInstruction[]
        {
            // UnityEngine.Object.Destroy(this, 0f);
            // return;
            new(OpCodes.Ldarg_0),
            new(OpCodes.Ldc_R4, 0f),
            new(OpCodes.Call, Method(typeof(Object), nameof(Object.Destroy), new[] { typeof(Object), typeof(float) })),
            new(OpCodes.Ret),
        };
    }
}
