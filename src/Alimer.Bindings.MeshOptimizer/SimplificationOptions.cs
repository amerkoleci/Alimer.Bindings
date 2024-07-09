// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace Meshopt;

[Flags]
public enum SimplificationOptions
{
    None = 0,
    /// <summary>
    /// Do not move vertices that are located on the topological border (vertices on triangle edges that don't have a paired triangle). Useful for simplifying portions of the larger mesh.
    /// </summary>
    SimplifyLockBorder = 1 << 0,
    /// <summary>
    /// Improve simplification performance assuming input indices are a sparse subset of the mesh. Note that error becomes relative to subset extents.
    /// </summary>
    meshopt_SimplifySparse = 1 << 1,
    /// <summary>
    /// Treat error limit and resulting error as absolute instead of relative to mesh extents.
    /// </summary>
    meshopt_SimplifyErrorAbsolute = 1 << 2,
}
