// -----------------------------------------------------------------------
// <copyright file="MethodMetrics.cs" company="LethalAPI Modding Community">
// Copyright (c) LethalAPI Modding Community. All rights reserved.
// Licensed under the LGPL-3.0 license.
// </copyright>
// Credit to both O5Zereth and Mitzey for the incredible work on the profiler.
// View the project here: https://github.com/mitzey234/nwapi-profiler.
// Original project licensed under MIT.
// -----------------------------------------------------------------------

namespace LethalAPI.DevTools.Profiler.Metrics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using API;
using Patches;

/// <summary>
/// Used to access profiler metrics.
/// </summary>
public static class MethodMetrics
{
    /// <summary>
    /// Gets the method metrics of a profiled method.
    /// </summary>
    /// <param name="print">Indicates whether or not to print the metrics to the console.</param>
    /// <returns>The resulting string.</returns>
    public static string GetMethodMetrics(bool print = false)
    {
        const int typePadding = 30;
        const int methodPadding = -25;
        const int invocationPadding = 2;
        const int averageTickPadding = 7;
        const int memoryPadding = 6;
        const int totalTickPadding = 6;
        const int maxTicksPadding = 8;
        StringBuilder builder = new();

        List<AsRef<ProfileMethodPatch.ProfiledMethodInfo>> enumerable = ProfileMethodPatch.GetProfilerInfos().ToList();

        int count = 0;
        IOrderedEnumerable<AsRef<ProfileMethodPatch.ProfiledMethodInfo>> sortedDict = from entry in enumerable orderby entry.Value.InvocationCount descending select entry;

        builder.AppendLine($"\n&a=======================================&r");
        builder.AppendLine($"            &3Profiling Stats&r            ");
        builder.AppendLine("&6" + $"{DateTime.Now:h:mm.ss tt}".PadBoth(39));
        builder.AppendLine($"&a=======================================&r");
        builder.AppendLine("\n" + $"[&a=======[ &3{"Invocation Count".PadBoth(16)}&a ]=======&r]");
        builder.AppendLine($"{"[Type]".PadBoth(typePadding)}.{"[Method]".PadBoth(methodPadding)} [Invocations] [Average Ticks Per]");

        foreach (AsRef<ProfileMethodPatch.ProfiledMethodInfo> asRefInfo in sortedDict)
        {
            ref ProfileMethodPatch.ProfiledMethodInfo info = ref asRefInfo.Value;
            string[] methodInfo = info.GetMyMethod.Split('.');
            string method = methodInfo[methodInfo.Length - 1];
            string type = methodInfo[0] + (methodInfo.Length > 2 ? methodInfo[1] : string.Empty);

            builder.AppendLine($"&r{type,typePadding}.&6{method,methodPadding}&r - &7{info.InvocationCount,invocationPadding}&r - Avg. Ticks Per: &7{info.AvgTicks,averageTickPadding}&r");

            if (count++ > 10)
                break;
        }

        builder.AppendLine();

        count = 0;
        sortedDict = from entry in enumerable orderby entry.Value.TotalTicks descending select entry;

        builder.AppendLine($"[&a=======[ &3{"Tick Count".PadBoth(16)}&a ]=======&r]");
        builder.AppendLine($"{"[Type]".PadBoth(typePadding)}.{"[Method]".PadBoth(methodPadding)} [Total Ticks] [Average Ticks Per]");
        foreach (AsRef<ProfileMethodPatch.ProfiledMethodInfo> asRefInfo in sortedDict)
        {
            ref ProfileMethodPatch.ProfiledMethodInfo info = ref asRefInfo.Value;
            string[] methodInfo = info.GetMyMethod.Split('.');
            string method = methodInfo[methodInfo.Length - 1];
            string type = methodInfo[0] + (methodInfo.Length > 2 ? methodInfo[1] : string.Empty);

            builder.AppendLine($"&r{type,typePadding}.&6{method,methodPadding}&r - &7{info.TotalTicks,totalTickPadding}&r - Avg. Ticks Per: &7{info.AvgTicks,averageTickPadding}&r");

            if (count++ > 10)
                break;
        }

        builder.AppendLine();

        count = 0;
        sortedDict = from entry in enumerable.Where(x => x.Value.InvocationCount > 10) orderby entry.Value.AvgTicks descending select entry;

        builder.AppendLine($"[&a=======[ &3{"Ticks Per Invoke".PadBoth(16)}&a ]=======&r]");
        builder.AppendLine($"{"[Type]".PadBoth(typePadding)}.{"[Method]".PadBoth(methodPadding)} [Average Ticks] [Invocations]");
        foreach (AsRef<ProfileMethodPatch.ProfiledMethodInfo> asRefInfo in sortedDict)
        {
            ref ProfileMethodPatch.ProfiledMethodInfo info = ref asRefInfo.Value;
            string[] methodInfo = info.GetMyMethod.Split('.');
            string method = methodInfo[methodInfo.Length - 1];
            string type = methodInfo[0] + (methodInfo.Length > 2 ? methodInfo[1] : string.Empty);

            builder.AppendLine($"&r{type,typePadding}.&6{method,methodPadding}&r - &7{info.AvgTicks,averageTickPadding}&r - Invocation count: &7{info.InvocationCount,invocationPadding}&r");

            if (count++ > 10)
                break;
        }

        builder.AppendLine();

        count = 0;
        sortedDict = from entry in enumerable orderby entry.Value.MaxTicks descending select entry;

        builder.AppendLine($"[&a=======[ &3{"Max Ticks".PadBoth(16)}&a ]=======&r]");
        builder.AppendLine($"{"[Type]".PadBoth(typePadding)}.{"[Method]".PadBoth(methodPadding)} [Max Ticks] [Invocations]");
        foreach (AsRef<ProfileMethodPatch.ProfiledMethodInfo> asRefInfo in sortedDict)
        {
            ref ProfileMethodPatch.ProfiledMethodInfo info = ref asRefInfo.Value;
            string[] methodInfo = info.GetMyMethod.Split('.');
            string method = methodInfo[methodInfo.Length - 1];
            string type = methodInfo[0] + (methodInfo.Length > 2 ? methodInfo[1] : string.Empty);

            builder.AppendLine($"&r{type,typePadding}.&6{method,methodPadding}&r - &7{info.MaxTicks,maxTicksPadding}&r - Invocation count: &7{info.InvocationCount,invocationPadding}&r");

            if (count++ > 10)
                break;
        }

        builder.AppendLine();

        count = 0;
        sortedDict = from entry in enumerable orderby entry.Value.TotalMemory descending select entry;

        builder.AppendLine($"[&a=======[ &3{"Memory Allocated".PadBoth(16)}&a ]=======&r]");
        builder.AppendLine($"{"[Type]".PadBoth(typePadding)}.{"[Method]".PadBoth(methodPadding)} [TotalMemory] [Invocations]");
        foreach (AsRef<ProfileMethodPatch.ProfiledMethodInfo> asRefInfo in sortedDict)
        {
            ref ProfileMethodPatch.ProfiledMethodInfo info = ref asRefInfo.Value;
            string[] methodInfo = info.GetMyMethod.Split('.');
            string method = methodInfo[methodInfo.Length - 1];
            string type = methodInfo[0] + (methodInfo.Length > 2 ? methodInfo[1] : string.Empty);

            builder.AppendLine($"&r{type,typePadding}.&6{method,methodPadding}&r - &7{info.TotalMemory,memoryPadding}&r - Invocation count: &7{info.InvocationCount,invocationPadding}&r");

            if (count++ > 10)
                break;
        }

        string result = builder.ToString();

        if (print)
            Log.Info(result);

        return result;
    }

    private static string PadBoth(this string source, int length, char padChar = ' ')
    {
        if (length < 0)
            length = UnityEngine.Mathf.Abs(length);
        int spaces = length - source.Length;
        int padLeft = (spaces / 2) + source.Length;
        return source.PadLeft(padLeft, padChar).PadRight(length, padChar);
    }
}
