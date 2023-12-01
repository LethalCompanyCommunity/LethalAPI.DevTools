// -----------------------------------------------------------------------
// <copyright file="MethodExtensions.cs" company="LethalAPI Modding Community">
// Copyright (c) LethalAPI Modding Community. All rights reserved.
// Licensed under the LGPL-3.0 license.
// </copyright>
// Credit to both O5Zereth and Mitzey for the incredible work on the profiler.
// View the project here: https://github.com/mitzey234/nwapi-profiler.
// Original project licensed under MIT.
// -----------------------------------------------------------------------

namespace LethalAPI.DevTools.Profiler.Extensions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using HarmonyLib;
using UnityEngine;

/// <summary>
/// Extensions for checking methods.
/// </summary>
public static class MethodExtensions
{
    private static readonly Assembly AllowedAssembly = typeof(GameNetworkManager).Assembly;

    private static readonly string[] DisallowedNamespaces = new string[]
    {
        "_Scripts.",
        "Achievements.",
        "AudioPooling.",
        "Authenticator.",
        "CameraShaking.",
        "CommandSystem.",
        "Cryptography.",
        "CursorManagement.",
        "CustomCulling.",
        "CustomRendering.",
        "DeathAnimations.",
        "Decals.",
        "GameCore.",
        "Hints.",
        "LiteNetLib.",
        "LiteNetLib4Mirror.",
        "MapGeneration.",
        "Microsoft.",
        "Mirror.",
        "RadialMenus.",
        "Security.",
        "Serialization.",
        "ServerOutput.",
        "Subtitles.",
        "System.",
        "Targeting.",
        "ToggleableMenus.",
        "UserSettings.",
        "Utf8Json.",
        "Utils.",
        "Waits.",
        "Windows.",
    };

    private static readonly Type[] DisallowedTypes = new Type[]
    {
    };

    /// <summary>
    /// Checks a method to see if it allows profiling.
    /// </summary>
    /// <param name="method">The method to check.</param>
    /// <returns>True if the profiler patch applied properly, false if an issue occured.</returns>
    public static bool AllowsProfiling(this MethodInfo method)
    {
        // Must be the Assembly-CSharp assembly.
        if (method.DeclaringType?.Assembly != AllowedAssembly)
            return false;

        // Disallowed namespaces
        if (method.DeclaringType.Namespace != null && DisallowedNamespaces.Any((method.DeclaringType.FullName ?? string.Empty).StartsWith))
            return false;

        // Disallowed types
        if (DisallowedTypes.Contains(method.DeclaringType))
            return false;
        if (DisallowedTypes.Any(x => x.IsAssignableFrom(method.DeclaringType)))
            return false;

        // Allow coroutine MoveNext functions
        if (method.IsCoroutineMoveNext())
            return true;

        // Don't allow generic methods
        if (method.IsGenericMethod)
            return false;

        // Don't allow constructors
        if (method.IsConstructor)
            return false;

        // Don't allow abstract members
        if (method.IsAbstract)
            return false;

        // Don't allow methods without a body
        if (!method.HasMethodBody())
            return false;

        // Don't allow RuntimeInitializeOnLoadMethod attributed methods to be patched
        // These methods are only run once
        if (method.IsRuntimeInitializeOnLoad())
            return false;

        bool compilerGenerated = method.IsCompilerGenerated();

        // Don't allow compiler generated property getters / setters
        if (compilerGenerated && method.IsGetterSetter())
            return false;

        // Don't allow compiler generated event adds / removes
        if (compilerGenerated && method.IsAddRemove())
            return false;

        // Don't allow operators
        if (method.IsOperator())
            return false;

        // Don't patch really small methods
        // if (method.GetMethodBody().GetILAsByteArray().Length < 30)
            // return false;

        // Don't allow methods that return IEnumerable
        if (method.ReturnsEnumerable())
            return false;

        return true;
    }

    /// <summary>
    /// Checks a method to see if it is initialized on load.
    /// </summary>
    /// <param name="method">The method to check.</param>
    /// <returns>True if it is initialized on load, false if it is not.</returns>
    public static bool IsRuntimeInitializeOnLoad(this MethodInfo method)
    {
        return method.IsDefined(typeof(RuntimeInitializeOnLoadMethodAttribute));
    }

    /// <summary>
    /// Checks a method to see if it is a compiler generated item.
    /// </summary>
    /// <param name="method">The method to check.</param>
    /// <returns>True if it is a compiler generated item, false if it is not.</returns>
    public static bool IsCompilerGenerated(this MethodInfo method)
    {
        return method.IsDefined(typeof(CompilerGeneratedAttribute), false);
    }

    /// <summary>
    /// Checks a method to see if it is a getter or setter.
    /// </summary>
    /// <param name="method">The method to check.</param>
    /// <returns>True if it is a getter or setter, false if it is not.</returns>
    public static bool IsGetterSetter(this MethodInfo method)
    {
        if (!method.IsSpecialName)
            return false;

        return method.Name.StartsWith("get_") || method.Name.StartsWith("set_");
    }

    /// <summary>
    /// Checks a method to see if it is an add or remove method.
    /// </summary>
    /// <param name="method">The method to check.</param>
    /// <returns>True if it is a add or remove method, false if it is not.</returns>
    public static bool IsAddRemove(this MethodInfo method)
    {
        if (!method.IsSpecialName)
            return false;

        return method.Name.StartsWith("add_") || method.Name.StartsWith("remove_");
    }

    /// <summary>
    /// Checks a method to see if it is an operator.
    /// </summary>
    /// <param name="method">The method to check.</param>
    /// <returns>True if it is an operator, false if it is not.</returns>
    public static bool IsOperator(this MethodInfo method)
    {
        if (!method.IsSpecialName)
            return false;

        return method.Name.StartsWith("op_");
    }

    /// <summary>
    /// Checks a method to see if it is a coroutine move next.
    /// </summary>
    /// <param name="method">The method to check.</param>
    /// <returns>True if it is a coroutine move next, false if it is not.</returns>
    public static bool IsCoroutineMoveNext(this MethodInfo method)
    {
        if (method.Name != "MoveNext")
            return false;

        if (method.ReturnType != typeof(bool))
            return false;

        if (method.GetParameters().Length != 0)
            return false;

        return typeof(IEnumerator<float>).IsAssignableFrom(method.DeclaringType);
    }

    /// <summary>
    /// Checks a method to see if it is an eneumerable or not.
    /// </summary>
    /// <param name="method">The method to check.</param>
    /// <returns>True if it is an enumerable, false if it is not.</returns>
    public static bool ReturnsEnumerable(this MethodInfo method)
    {
        return typeof(IEnumerable).IsAssignableFrom(method.ReturnType);
    }
}
