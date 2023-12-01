// -----------------------------------------------------------------------
// <copyright file="FieldCollectionReaders.cs" company="LethalAPI Modding Community">
// Copyright (c) LethalAPI Modding Community. All rights reserved.
// Licensed under the LGPL-3.0 license.
// </copyright>
// Credit to both O5Zereth and Mitzey for the incredible work on the profiler.
// View the project here: https://github.com/mitzey234/nwapi-profiler.
// Original project licensed under MIT.
// -----------------------------------------------------------------------

// ReSharper disable MemberCanBePrivate.Global
namespace LethalAPI.DevTools.Profiler.Utils;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

using Extensions;

using static HarmonyLib.AccessTools;

/// <summary>
/// Creates dynamic methods which read the counts of collections within static fields across the appdomain.
/// </summary>
internal static class FieldCollectionReaders
{
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private static readonly List<FieldInfo> AllFields;

    private static readonly Dictionary<Type, Action<FieldInfo, ILGenerator>> FieldProcessors = new()
    {
        { typeof(List<>), CountGetter.ImplementMethod },
        { typeof(HashSet<>), CountGetter.ImplementMethod },
        { typeof(Dictionary<,>), CountGetter.ImplementMethod },
        { typeof(ArrayList), CountGetter.ImplementMethod },
        { typeof(BitArray), CountGetter.ImplementMethod },
        { typeof(Stack<>), CountGetter.ImplementMethod },
        { typeof(Array), CountGetter.ImplementMethod },
        { typeof(Queue<>), CountGetter.ImplementMethod },
    };

    static FieldCollectionReaders()
    {
        Type[] types = typeof(GameNetworkManager).Assembly.GetTypes();

        AllFields = new();

        foreach (Type type in types)
        {
            foreach (FieldInfo field in type.GetFullyConstructedFields(includeNonPublic: true))
            {
                if (field.DeclaringType?.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) != null)
                    continue;

                Type fieldType = field.FieldType;

                if (!fieldType.IsGenericType)
                    continue;

                if (!FieldProcessors.ContainsKey(fieldType.GetGenericTypeDefinition()))
                    continue;

                AllFields.Add(field);
            }
        }

        foreach (FieldInfo field in AllFields)
        {
            ProcessField(field);
        }
    }

    /// <summary>
    /// Gets the count of the collection.
    /// </summary>
    /// <returns>The count of the object.</returns>
    public delegate int GetCount();

    /// <summary>
    /// Gets the count for each field.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public static Dictionary<FieldInfo, GetCount> CachedFields { get; private set; } = new();

    /// <summary>
    /// Gets the counts of the fields.
    /// </summary>
    /// <returns>The current field and the respective count.</returns>
    public static IEnumerable<(FieldInfo Field, int Count)> GetCounts()
    {
        foreach (KeyValuePair<FieldInfo, GetCount> pair in CachedFields)
        {
            yield return (pair.Key, pair.Value());
        }
    }

    /// <summary>
    /// Processes a field.
    /// </summary>
    /// <param name="field">The field to process.</param>
    public static void ProcessField(FieldInfo field)
    {
        if (CachedFields.ContainsKey(field))
            return;

        Type genericTypeDef = field.FieldType;

        if (genericTypeDef.IsGenericType)
            genericTypeDef = genericTypeDef.GetGenericTypeDefinition();

        DynamicMethod method = new DynamicMethod($"{field.DeclaringType?.Name}.{field.Name}", typeof(int), Array.Empty<Type>(), true);

        FieldProcessors[genericTypeDef](field, method.GetILGenerator());

        CachedFields[field] = (GetCount)method.CreateDelegate(typeof(GetCount));
    }

    private static class CountGetter
    {
        public static void ImplementMethod(FieldInfo field, ILGenerator generator)
        {
            Label notNullLabel = generator.DefineLabel();

            // return field?.Count ?? 0;
            generator.Emit(OpCodes.Ldsfld, field);
            generator.Emit(OpCodes.Dup);
            generator.Emit(OpCodes.Brtrue_S, notNullLabel);
            generator.Emit(OpCodes.Pop);
            generator.Emit(OpCodes.Ldc_I4_0);
            generator.Emit(OpCodes.Ret);

            MethodInfo countGetter = PropertyGetter(field.FieldType, "Count");

            generator.MarkLabel(notNullLabel);
            generator.Emit(countGetter.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, countGetter);
            generator.Emit(OpCodes.Ret);
        }
    }
}
