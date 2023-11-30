// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="LethalAPI Modding Community">
// Copyright (c) LethalAPI Modding Community. All rights reserved.
// Licensed under the LGPL-3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace LethalAPI.DevTools;

using System.ComponentModel;

using LethalAPI.Core.Interfaces;

/// <summary>
/// The main config class.
/// </summary>
// ReSharper disable ClassNeverInstantiated.Global
public class Config : IConfig
{
    /// <inheritdoc />
    [Description("Indicates whether or not the plugin should run.")]
    public bool IsEnabled { get; set; } = true;

    /// <inheritdoc />
    [Description("Indicates whether debug logs should be output.")]
    public bool Debug { get; set; } = false;
}