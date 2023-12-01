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

using System.Collections.Generic;
using System.Linq;
using System.Text;

using API;
using Patches;

/// <summary>
/// Used to access profiler metrics.
/// </summary>
public class MethodMetrics
{
    /// <summary>
    /// Gets the method metrics of a profiled method.
    /// </summary>
    /// <param name="print">Indicates whether or not to print the metrics to the console.</param>
    /// <returns>The resulting string.</returns>
    public static string GetMethodMetrics(bool print = false)
    {
        StringBuilder builder = new();

        List<AsRef<ProfileMethodPatch.ProfiledMethodInfo>> enumerable = ProfileMethodPatch.GetProfilerInfos().ToList();

        int count = 0;
        IOrderedEnumerable<AsRef<ProfileMethodPatch.ProfiledMethodInfo>> sortedDict = from entry in enumerable orderby entry.Value.InvocationCount descending select entry;

        builder.AppendLine("Invocation count: ");

        foreach (AsRef<ProfileMethodPatch.ProfiledMethodInfo> asRefInfo in sortedDict)
        {
            ref ProfileMethodPatch.ProfiledMethodInfo info = ref asRefInfo.Value;
            string method = info.GetMyMethod;

            builder.AppendLine($"{method} - {info.InvocationCount} - Avg. Ticks Per: {info.AvgTicks}");

            if (count++ > 10)
                break;
        }

        builder.AppendLine();

        count = 0;
        sortedDict = from entry in enumerable orderby entry.Value.TotalTicks descending select entry;

        builder.AppendLine("Tick count: ");
        foreach (AsRef<ProfileMethodPatch.ProfiledMethodInfo> asRefInfo in sortedDict)
        {
            ref ProfileMethodPatch.ProfiledMethodInfo info = ref asRefInfo.Value;
            string method = info.GetMyMethod;

            builder.AppendLine($"{method} - {info.TotalTicks} - Avg. Ticks Per: {info.AvgTicks}");

            if (count++ > 10)
                break;
        }

        builder.AppendLine();

        count = 0;
        sortedDict = from entry in enumerable.Where(x => x.Value.InvocationCount > 10) orderby entry.Value.AvgTicks descending select entry;

        builder.AppendLine("Ticks per invoke:");
        foreach (AsRef<ProfileMethodPatch.ProfiledMethodInfo> asRefInfo in sortedDict)
        {
            ref ProfileMethodPatch.ProfiledMethodInfo info = ref asRefInfo.Value;
            string method = info.GetMyMethod;

            builder.AppendLine($"{method} - {info.AvgTicks} - Invocation count: {info.InvocationCount}");

            if (count++ > 10)
                break;
        }

        builder.AppendLine();

        count = 0;
        sortedDict = from entry in enumerable orderby entry.Value.MaxTicks descending select entry;

        builder.AppendLine("Max ticks:");
        foreach (AsRef<ProfileMethodPatch.ProfiledMethodInfo> asRefInfo in sortedDict)
        {
            ref ProfileMethodPatch.ProfiledMethodInfo info = ref asRefInfo.Value;
            string method = info.GetMyMethod;

            builder.AppendLine($"{method} - {info.MaxTicks} - Invocation count: {info.InvocationCount}");

            if (count++ > 10)
                break;
        }

        builder.AppendLine();

        count = 0;
        sortedDict = from entry in enumerable orderby entry.Value.TotalMemory descending select entry;

        builder.AppendLine("Memory Allocated:");
        foreach (AsRef<ProfileMethodPatch.ProfiledMethodInfo> asRefInfo in sortedDict)
        {
            ref ProfileMethodPatch.ProfiledMethodInfo info = ref asRefInfo.Value;
            string method = info.GetMyMethod;

            builder.AppendLine($"{method} - {info.TotalMemory} - Invocation count: {info.InvocationCount}");

            if (count++ > 10)
                break;
        }

        string result = builder.ToString();

        if (print)
            Log.Info(result);

        return result;
    }
}
