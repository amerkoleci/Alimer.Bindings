option(CIMGUI_INCLUDE_DEMOS "Include ImGui demo files" ON)
option(IMGUI_FREETYPE "Build the FreeType font rasteriser" OFF)

set(IMGUI_SOURCES
    cimgui.h
    cimgui.cpp
    imgui/imgui.cpp
    imgui/imgui_draw.cpp
    
    imgui/imgui_widgets.cpp
	imgui/imgui_tables.cpp
)

set(IMGUI_PLOT_SOURCES
    cimplot/cimplot.h
    cimplot/cimplot.cpp
    cimplot/implot/implot.cpp
    cimplot/implot/implot_items.cpp
)

set(IMGUIZMO_SOURCES
    cimguizmo/cimguizmo.h
    cimguizmo/cimguizmo.cpp
    cimguizmo/ImGuizmo/src/ImGuizmo.cpp
    cimguizmo/ImGuizmo/src/GraphEditor.cpp
    cimguizmo/ImGuizmo/src/ImCurveEdit.cpp
    cimguizmo/ImGuizmo/src/ImGradient.cpp
    cimguizmo/ImGuizmo/src/ImSequencer.cpp
    cimguizmo/ImGuizmo/src/ImVectorEditor.cpp
)

if(CIMGUI_INCLUDE_DEMOS)
    set(IMGUI_SOURCES ${IMGUI_SOURCES} imgui/imgui_demo.cpp)
    set(IMGUI_PLOT_SOURCES ${IMGUI_PLOT_SOURCES} cimplot/implot/implot_demo.cpp)
endif()

add_library(cimgui ${IMGUI_SOURCES} ${IMGUI_PLOT_SOURCES} ${IMGUIZMO_SOURCES})

if(IMGUI_FREETYPE)
    if(EMSCRIPTEN)
        # Emscripten ships FreeType as a port, so there is nothing to find: the flag
        # both compiles it and links it.
        target_compile_options(cimgui PRIVATE -sUSE_FREETYPE=1)
        target_link_options(cimgui PRIVATE -sUSE_FREETYPE=1)
    else()
        find_package(Freetype REQUIRED)
        target_link_libraries(cimgui PRIVATE Freetype::Freetype)
    endif()
    target_sources(cimgui PRIVATE cimgui/imgui/misc/freetype/imgui_freetype.cpp)

    # Both loaders, deliberately. imgui guards each behind its own flag, and defining
    # only IMGUI_ENABLE_FREETYPE drops the stb_truetype one -- while cimgui still emits
    # a wrapper calling ImFontAtlasGetFontLoaderForStbTruetype, so the library stops
    # compiling. That is what the first attempt at this did.
    #
    # Keeping both is also what a binding wants: ImGuiFreeType_GetFontLoader exists so
    # the consumer can choose at runtime, which requires both to be present.
    target_compile_definitions(cimgui PRIVATE IMGUI_ENABLE_FREETYPE IMGUI_ENABLE_STB_TRUETYPE)
endif()

target_compile_definitions(cimgui PUBLIC IMGUI_DISABLE_OBSOLETE_FUNCTIONS=1 CIMGUI_VARGS0)
target_include_directories(cimgui PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})
target_include_directories(cimgui PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/imgui)
target_include_directories(cimgui PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/cimplot)
target_include_directories(cimgui PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/cimplot/implot)
target_include_directories(cimgui PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/cimguizmo)
target_include_directories(cimgui PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/cimguizmo/ImGuizmo/src)
set_target_properties(cimgui PROPERTIES PREFIX "")

if (NATIVE_ENABLE_INSTALL)
    install(TARGETS cimgui
        ARCHIVE DESTINATION  ${CMAKE_INSTALL_LIBDIR}
        LIBRARY DESTINATION  ${CMAKE_INSTALL_LIBDIR}
        RUNTIME DESTINATION  ${CMAKE_INSTALL_BINDIR}
    )
endif ()
