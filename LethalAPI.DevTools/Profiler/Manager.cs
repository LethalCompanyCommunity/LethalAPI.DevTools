// -----------------------------------------------------------------------
// <copyright file="Manager.cs" company="LethalAPI Modding Community">
// Copyright (c) LethalAPI Modding Community. All rights reserved.
// Licensed under the LGPL-3.0 license.
// </copyright>
// Credit to both O5Zereth and Mitzey for the incredible work on the profiler.
// View the project here: https://github.com/mitzey234/nwapi-profiler.
// Original project licensed under MIT.
// -----------------------------------------------------------------------

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace LethalAPI.DevTools.Profiler;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using API;
using Extensions;
using HarmonyLib;
using Patches;
using UnityEngine;

/// <summary>
/// Contains the api for using the profiler.
/// </summary>
public class Manager
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Manager"/> class.
    /// </summary>
    public Manager()
    {
        Instance = this;
    }

    /// <summary>
    /// Gets or sets a value indicating whether or not the framerate should be made uncapped.
    /// </summary>
    public static bool Uncapped { get; set; } = false;

    /// <summary>
    /// Gets the main instance for the profiling API.
    /// </summary>
    public static Manager Instance { get; private set; } = null!;

    /// <summary>
    /// Gets the number of patched methods.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public static int Patched { get; internal set; }

    /// <summary>
    /// Gets a hashset of all fields being profiled.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public static HashSet<FieldInfo> AllFields => new();

    /// <summary>
    /// Gets the primary harmony instance for the profiler.
    /// </summary>
    internal static Harmony Harmony => new ("com.LethalAPI.DevTools.Profiler");

    /// <summary>
    /// Gets the harmony instance for code optimizations.
    /// </summary>
    internal static Harmony HarmonyOptimizations => new("com.LethalAPI.DevTools.Optimizations");

    /// <summary>
    /// Resets the profiling system.
    /// </summary>
    public static void Reset()
    {
        foreach (AsRef<ProfileMethodPatch.ProfiledMethodInfo> asRefInfo in ProfileMethodPatch.GetProfilerInfos())
        {
            ref ProfileMethodPatch.ProfiledMethodInfo info = ref asRefInfo.Value;

            info.InvocationCount = 0;
            info.TotalTicks = 0;
            info.MaxTicks = 0;
            info.TotalMemory = 0;
        }
    }

    /// <summary>
    /// Disables the profiler.
    /// </summary>
    public static void DisableProfiler()
    {
        // MethodMetrics.methodMetrics.Clear(); //Sometimes I wanna keep the data, lets reset it when enabling
        ProfileMethodPatch.DisableProfiler = true;
    }

    /// <summary>
    /// Enables the profiler.
    /// </summary>
    public static void EnableProfiler()
    {
        if (Patched > 0)
        {
            ProfileMethodPatch.DisableProfiler = false;
            return;
        }

        Type[] types = typeof(GameNetworkManager).Assembly.GetTypes().IncludingNestedTypes().ToArray();

        // use hashset so we dont
        // try to patch the same method twice.
        HashSet<MethodBase> methods = new();

        int failed = 0;

        foreach (Type t in types)
        {
            foreach (MethodInfo m in t.GetFullyConstructedMethods(includeNonPublic: true))
            {
                if (!m.AllowsProfiling())
                    continue;

                methods.Add(m);
            }
        }

        Log.Info("Patching " + methods.Count + " methods");

        foreach (MethodBase m in methods)
        {
            try
            {
                ProfileMethodPatch.ApplyProfiler(m);
            }
            catch (Exception e)
            {
                failed++;
                Log.Error($"{m.DeclaringType?.FullName ?? "null"}::{m.Name} => " + e);
            }

            Patched++;
        }

        Log.Info("Failed to patch " + failed + " methods");
    }

    // ReSharper disable IdentifierTypo
    // ReSharper disable UnusedMember.Local
    #pragma warning disable SA1201 // field shouldnt follow a method.
    private static float timer;

    // ReSharper disable once NotAccessedField.Local
    private static int upcount;

    private static void OnUpdate()
    {
        timer += Time.deltaTime;

        if (timer <= 1.0f)
            return;
        timer = 0;

        if (ProfileMethodPatch.DisableProfiler && AllFields.Count > 0)
            AllFields.Clear();
        upcount++;
        AllFields.RemoveWhere(x => x == null);

        Application.targetFrameRate = Uncapped ? 1000 : 60;
    }
}