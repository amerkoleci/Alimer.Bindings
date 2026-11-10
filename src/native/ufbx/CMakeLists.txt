file(GLOB SOURCE_FILES
    ufbx.h
    ufbx.c
)
add_library(ufbx ${SOURCE_FILES})

if (BUILD_SHARED_LIBS)
    if(WIN32)
        # VMA decorates every public function with VMA_CALL_PRE
        target_compile_definitions(ufbx PRIVATE "ufbx_abi=__declspec(dllexport)")
    else()
        # Everything else stays internal, so the exported surface is the C API and nothing from the C++ implementation.
        set_target_properties(ufbx PROPERTIES
            CXX_VISIBILITY_PRESET hidden
            VISIBILITY_INLINES_HIDDEN ON
        )
        target_compile_definitions(ufbx PRIVATE "ufbx_abi=__attribute__((visibility(\"default\")))")
    endif ()
endif ()


if (NATIVE_ENABLE_INSTALL)
    install(TARGETS ufbx
        ARCHIVE DESTINATION  ${CMAKE_INSTALL_LIBDIR}
        LIBRARY DESTINATION  ${CMAKE_INSTALL_LIBDIR}
        RUNTIME DESTINATION  ${CMAKE_INSTALL_BINDIR}
    )
endif()
