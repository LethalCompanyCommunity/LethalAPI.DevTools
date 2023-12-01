// -----------------------------------------------------------------------
// <copyright file="MonoNative.cs" company="LethalAPI Modding Community">
// Copyright (c) LethalAPI Modding Community. All rights reserved.
// Licensed under the LGPL-3.0 license.
// </copyright>
// Credit to both O5Zereth and Mitzey for the incredible work on the profiler.
// View the project here: https://github.com/mitzey234/nwapi-profiler.
// Original project licensed under MIT.
// -----------------------------------------------------------------------

#pragma warning disable SA1300 // method naming
namespace LethalAPI.DevTools.Profiler.API;

using System.Runtime.InteropServices;

/// <summary>
/// Contains tools for getting data from Mono.
/// </summary>
internal static class MonoNative
{
    /// <summary>
    /// Get the approximate amount of memory used by managed objects.
    /// </summary>
    /// <returns>the amount of memory used in bytes.</returns>
    [DllImport("__Internal")]
    public static extern long mono_gc_get_used_size();

    /// <summary>
    /// Get the amount of memory used by the garbage collector.
    /// </summary>
    /// <returns>the size of the heap in bytes.</returns>
    [DllImport("__Internal")]
    public static extern long mono_gc_get_heap_size();
}
