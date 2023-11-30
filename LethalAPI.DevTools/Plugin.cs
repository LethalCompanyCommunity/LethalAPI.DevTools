// -----------------------------------------------------------------------
// <copyright file="Plugin.cs" company="LethalAPI Modding Community">
// Copyright (c) LethalAPI Modding Community. All rights reserved.
// Licensed under the LGPL-3.0 license.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace LethalAPI.DevTools;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnusedType.Global
#pragma warning disable SA1401 // Field should be made private.
#pragma warning disable SA1402 // File can only contain a single type.

using System;

using HarmonyLib;

using LethalAPI.Core;

/// <summary>
///     The main plugin class.
/// </summary>
public class Plugin : LethalAPI.Core.Features.Plugin<Config>
{
    /// <summary>
    ///     Gets the main <see cref="Plugin"/> instance.
    /// </summary>
    /// <remarks>
    ///     This is optional. A custom implementation can replace this.
    /// </remarks>
    public static Plugin Instance { get; private set; } = null!;

    /// <summary>
    ///     Gets the main <see cref="Harmony"/> instance for the plugin.
    /// </summary>
    /// <remarks>
    ///     This is optional. A custom implementation can replace this.
    /// </remarks>
    public static Harmony Harmony => new("LethalAPI.DevTools");

    /// <inheritdoc />
    public override string Name => "LethalAPI.DevTools";

    /// <inheritdoc />
    public override string Description => "A collection of tools for LethalAPI to help make the job of developers easier.";

    /// <inheritdoc />
    public override string Author => "LethalAPI Modding Community";

    /// <inheritdoc />
    public override Version Version => new(0, 0, 1);

    /// <inheritdoc />
    public override void OnEnabled()
    {
        if (!this.Config.IsEnabled)
        {
            return;
        }

        Instance = this;
        Log.Info($"Started plugin LethalAPI.DevTools v{this.Version} by LethalAPI Modding Community.");
    }
}
