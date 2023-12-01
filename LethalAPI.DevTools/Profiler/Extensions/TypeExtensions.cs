// -----------------------------------------------------------------------
// <copyright file="TypeExtensions.cs" company="LethalAPI Modding Community">
// Copyright (c) LethalAPI Modding Community. All rights reserved.
// Licensed under the LGPL-3.0 license.
// </copyright>
// Credit to both O5Zereth and Mitzey for the incredible work on the profiler.
// View the project here: https://github.com/mitzey234/nwapi-profiler.
// Original project licensed under MIT.
// -----------------------------------------------------------------------

namespace LethalAPI.DevTools.Profiler.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using HarmonyLib;

/// <summary>
/// Provides extensions for types.
/// </summary>
public static class TypeExtensions
{
    /// <summary>This gets methods within contructed generic types. Note that generic methods are skipped.</summary>
    /// <returns>A list of fully constructed methods.</returns>
    /// <param name="type">The type to check.</param>
    /// <param name="includeNonPublic">Should non-public methods be checked.</param>
    public static IEnumerable<MethodInfo> GetFullyConstructedMethods(this Type? type, bool includeNonPublic)
    {
        if (type is null)
            yield break;

        if (type.IsGenericType && !type.IsConstructedGenericType)
        {
            yield break;
        }

        BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly;

        if (includeNonPublic)
        {
            flags |= BindingFlags.NonPublic;
        }

        while (type != null)
        {
            MethodInfo[] methods = type.GetMethods(flags);

            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo m = methods[i];

                if (m.IsGenericMethod)
                    continue;

                if (!m.HasMethodBody())
                    continue;

                yield return m;
            }

            type = type.BaseType;
        }
    }

    /// <summary>This only gets static fields within contructed generic types.</summary>
    /// <returns>The static fields with constructed generic types.</returns>
    /// <param name="type">The type to check.</param>
    /// <param name="includeNonPublic">Should non-public methods be checked.</param>
    public static IEnumerable<FieldInfo> GetFullyConstructedFields(this Type? type, bool includeNonPublic)
    {
        if (type is null)
            yield break;

        if (type.IsGenericType && !type.IsConstructedGenericType)
        {
            yield break;
        }

        BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly;

        if (includeNonPublic)
        {
            flags |= BindingFlags.NonPublic;
        }

        while (type != null)
        {
            FieldInfo[] fields = type.GetFields(flags);

            for (int i = 0; i < fields.Length; i++)
            {
                yield return fields[i];
            }

            type = type.BaseType;
        }
    }

    /// <summary>
    /// Gets a hash set of the nested types.
    /// </summary>
    /// <param name="types">The orignal types to search.</param>
    /// <returns>A hash set of the nested types.</returns>
    public static HashSet<Type> IncludingNestedTypes(this IEnumerable<Type> types)
    {
        return types.SelectMany(IncludingNestedTypes).ToHashSet();
    }

    /// <summary>
    /// todo.
    /// </summary>
    /// <param name="type">todo .</param>
    /// <returns> to do.</returns>
    public static IEnumerable<Type> IncludingNestedTypes(this Type type)
    {
        Queue<Type> types = new(8);
        types.Enqueue(type);

        while (types.Count > 0)
        {
            Type t = types.Dequeue();

            yield return t;

            Type[] nested = t.GetNestedTypes(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            for (int i = 0; i < nested.Length; i++)
            {
                types.Enqueue(nested[i]);
            }
        }
    }
}
