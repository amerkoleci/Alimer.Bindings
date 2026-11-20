// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

public static partial class Box2D
{
    public const string LibraryName = "box2d";

    /// <summary>
    /// Get the angle in radians in the range [-pi, pi]
    /// </summary>
    /// <param name="q"></param>
    /// <returns></returns>
    public static float b2Rot_GetAngle(in b2Rot q) => MathF.Atan2(q.s, q.c);
}
