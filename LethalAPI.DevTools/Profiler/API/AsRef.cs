// -----------------------------------------------------------------------
// <copyright file="AsRef.cs" company="LethalAPI Modding Community">
// Copyright (c) LethalAPI Modding Community. All rights reserved.
// Licensed under the LGPL-3.0 license.
// </copyright>
// Credit to both O5Zereth and Mitzey for the incredible work on the profiler.
// View the project here: https://github.com/mitzey234/nwapi-profiler.
// Original project licensed under MIT.
// -----------------------------------------------------------------------

namespace LethalAPI.DevTools.Profiler.API;

using System.Runtime.CompilerServices;

/// <summary>
/// Provides tools for working with references.
/// </summary>
/// <typeparam name="T">The type of the reference.</typeparam>
public unsafe readonly struct AsRef<T>
{
    private readonly void* ptr;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsRef{T}"/> struct.
    /// </summary>
    /// <param name="reference">The value to reference.</param>
    public AsRef(ref T reference)
    {
        ptr = Unsafe.AsPointer(ref reference);
    }

    /// <summary>
    /// Gets the value of the pointer as a reference.
    /// </summary>
    public ref T Value
    {
        get
        {
            return ref Unsafe.AsRef<T>(ptr);
        }
    }
}
