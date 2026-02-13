// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static Box2D;

namespace Alimer.WebGPU.Samples;

public static unsafe class Program
{
    public static void Main()
    {
        // https://box2d.org/documentation/hello.html
        b2WorldDef worldDef = b2DefaultWorldDef();
        worldDef.gravity = new(0.0f, -10.0f);
        b2WorldId worldId = b2CreateWorld(&worldDef);

        // Ground Box
        b2BodyDef groundBodyDef = b2DefaultBodyDef();
        groundBodyDef.position = new(0.0f, -10.0f);
        b2BodyId groundId = b2CreateBody(worldId, &groundBodyDef);
        b2Polygon groundBox = b2MakeBox(50.0f, 10.0f);
        b2ShapeDef groundShapeDef = b2DefaultShapeDef();
        b2CreatePolygonShape(groundId, &groundShapeDef, &groundBox);

        // Dynamic Body
        b2BodyDef bodyDef = b2DefaultBodyDef();
        bodyDef.type = b2_dynamicBody;
        bodyDef.position = new(0.0f, 4.0f);
        b2BodyId bodyId = b2CreateBody(worldId, &bodyDef);

        b2Polygon dynamicBox = b2MakeBox(1.0f, 1.0f);

        b2ShapeDef shapeDef = b2DefaultShapeDef();
        shapeDef.density = 1.0f;
        shapeDef.material.friction = 0.3f;
        b2CreatePolygonShape(bodyId, &shapeDef, &dynamicBox);

        // Simulating the World
        float timeStep = 1.0f / 60.0f;
        int subStepCount = 4;

        for (int i = 0; i < 90; ++i)
        {
            b2World_Step(worldId, timeStep, subStepCount);
            Vector2 position = b2Body_GetPosition(bodyId);
            b2Rot rotation = b2Body_GetRotation(bodyId);
            float angle = b2Rot_GetAngle(rotation);
            Console.WriteLine($"{position.X} {position.Y} {angle}");
        }

        b2DestroyWorld(worldId);
    }
}
