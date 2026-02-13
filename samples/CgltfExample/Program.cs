// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static Cgltf;

namespace Alimer.WebGPU.Samples;

public static unsafe class Program
{
    public static void Main()
    {
        // Test cgltf
        string glbFilePath = Path.Combine(AppContext.BaseDirectory, "Assets", $"DamagedHelmet.glb");
        byte[] glbFileContent = File.ReadAllBytes(glbFilePath);

        cgltf_options options = new();
        cgltf_data* data = default;
        //cgltf_result result = cgltf_parse_file(&options, glbFilePath, out data);
        //if (result == cgltf_result_success)
        //{
        //    /* TODO make awesome stuff */
        //    cgltf_free(data);
        //}

        cgltf_result result = cgltf_parse(&options, glbFileContent, &data);
        if (result == cgltf_result_success)
        {
            cgltf_result loadResult = cgltf_load_buffers(&options, data, glbFilePath);
            if (loadResult != cgltf_result_success)
            {
            }

            nuint size = cgltf_write(&options, data);
            byte[] buffer = new byte[(int)size];

            nuint written = cgltf_write(&options, buffer, data);
            if (written != size)
            {
                /* TODO handle error */
            }

            string glbSaveFilePath = Path.Combine(AppContext.BaseDirectory, "Assets", "Meshes", $"Out.glb");
            options.type = cgltf_file_type_glb;
            result = cgltf_write_file(&options, glbSaveFilePath, data);
            if (result != cgltf_result_success)
            {
                /* TODO handle error */
            }

            /* TODO make awesome stuff */
            cgltf_free(data);
        }
    }
}
