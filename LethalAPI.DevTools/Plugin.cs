// -----------------------------------------------------------------------
// <copyright file="Plugin.cs" company="LethalAPI Modding Community">
// Copyright (c) LethalAPI Modding Community. All rights reserved.
// Licensed under the LGPL-3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace LethalAPI.DevTools;

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
    public static Harmony Harmony => new("com.LethalAPI.DevTools");

    /// <inheritdoc />
    public override string Name => "LethalAPI.DevTools";

    /// <inheritdoc />
    public override string Description => "A collection of tools for LethalAPI to help make the job of developers easier.";

    /// <inheritdoc />
    public override string Author => "LethalAPI Modding Community";

    /// <inheritdoc />
    public override Version Version => new(1, 0, 0);

    /// <inheritdoc />
    public override void OnEnabled()
    {
        if (!this.Config.IsEnabled)
        {
            return;
        }

        Instance = this;
        Log.Info($"Started plugin &3LethalAPI.DevTools &gv1.0.0 &rby &7LethalAPI Modding Community&r.{(this.Config.Debug ? " [&2Debug&r]" : string.Empty)}");
    }
}