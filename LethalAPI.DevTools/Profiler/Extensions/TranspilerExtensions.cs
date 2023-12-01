// -----------------------------------------------------------------------
// <copyright file="TranspilerExtensions.cs" company="LethalAPI Modding Community">
// Copyright (c) LethalAPI Modding Community. All rights reserved.
// Licensed under the LGPL-3.0 license.
// </copyright>
// Credit to both O5Zereth and Mitzey for the incredible work on the profiler.
// View the project here: https://github.com/mitzey234/nwapi-profiler.
// Original project licensed under MIT.
// -----------------------------------------------------------------------

namespace LethalAPI.DevTools.Profiler.Extensions;

using System.Collections.Generic;

using HarmonyLib;

/// <summary>
/// Provides extensions for making and working with transpilers.
/// </summary>
public static class TranspilerExtensions
{
    /// <summary>
    /// Gets the instructions as a list.
    /// </summary>
    /// <param name="instructions">The provided enumerable of instructions.</param>
    /// <param name="newInstructions">The list of instructions.</param>
    public static void BeginTranspiler(this IEnumerable<CodeInstruction> instructions, out List<CodeInstruction> newInstructions)
    {
        newInstructions = new(instructions);
    }

    /// <summary>
    /// Finishes a transpiler.
    /// </summary>
    /// <param name="newInstructions">The instructions.</param>
    /// <returns>The enumerable containing the code instructions.</returns>
    public static IEnumerable<CodeInstruction> FinishTranspiler(this List<CodeInstruction> newInstructions)
    {
        for (int i = 0; i < newInstructions.Count; i++)
        {
            yield return newInstructions[i];
        }
    }
}
