// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static Box3D;

public static unsafe class Program
{
    public static void Main()
    {
        // https://github.com/erincatto/box3d/blob/main/docs/hello.md
        b3WorldDef worldDef = b3DefaultWorldDef();
        worldDef.gravity = new(0.0f, -10.0f, 0.0f);
        b3WorldId worldId = b3CreateWorld(&worldDef);

        // Ground Box
        b3BodyDef groundBodyDef = b3DefaultBodyDef();
        groundBodyDef.position = new(0.0f, -10.0f, 0.0f);
        b3BodyId groundId = b3CreateBody(worldId, &groundBodyDef);
        b3BoxHull groundBox = b3MakeBoxHull(50.0f, 10.0f, 50.0f);
        b3ShapeDef groundShapeDef = b3DefaultShapeDef();
        _ = b3CreateHullShape(groundId, &groundShapeDef, &groundBox.@base);

        // Dynamic Body
        b3BodyDef bodyDef = b3DefaultBodyDef();
        bodyDef.type = b3_dynamicBody;
        bodyDef.position = new(0.0f, 4.0f, 0.0f);

        b3BodyId bodyId = b3CreateBody(worldId, &bodyDef);
        b3BoxHull dynamicBox = b3MakeCubeHull(1.0f);
        b3ShapeDef shapeDef = b3DefaultShapeDef();
        shapeDef.density = 1.0f;
        shapeDef.baseMaterial.friction = 0.3f;
        _ = b3CreateHullShape(bodyId, &shapeDef, &dynamicBox.@base);

        // Simulating the World
        float timeStep = 1.0f / 60.0f;
        int subStepCount = 4;

        for (int i = 0; i < 90; ++i)
        {
            b3World_Step(worldId, timeStep, subStepCount);

            Vector3 position = b3Body_GetPosition(bodyId);
            Quaternion rotation = b3Body_GetRotation(bodyId);

            Console.WriteLine($"Position: {position}, Rotation: {rotation}");
        }

        b3DestroyWorld(worldId);
    }
}
