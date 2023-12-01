// -----------------------------------------------------------------------
// <copyright file="DisableSelfPatch.cs" company="LethalAPI Modding Community">
// Copyright (c) LethalAPI Modding Community. All rights reserved.
// Licensed under the LGPL-3.0 license.
// </copyright>
// Credit to both O5Zereth and Mitzey for the incredible work on the profiler.
// View the project here: https://github.com/mitzey234/nwapi-profiler.
// Original project licensed under MIT.
// -----------------------------------------------------------------------

// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Local
namespace LethalAPI.DevTools.Profiler.Patches;

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;
using UnityEngine;

using static HarmonyLib.AccessTools;

/// <summary>
/// This patch automatically forces behaviours to disable themselves.
/// </summary>
[HarmonyPatch]
public static class DisableSelfPatch
{
    private static IEnumerable<MethodInfo> TargetMethods()
    {
        // yield return Method(typeof(PersonalRadioPlayback), nameof(PersonalRadioPlayback.Awake)); //Disable radio updates and let manual updater do it every second, potentially breaks the game / radios
        yield break;
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
    {
        return new CodeInstruction[]
        {
            // this.enabled = false;
            // return;
            new(OpCodes.Ldarg_0),
            new(OpCodes.Ldc_I4_0),
            new(OpCodes.Call, PropertySetter(typeof(Behaviour), nameof(Behaviour.enabled))),
            new(OpCodes.Ret),
        };
    }
}