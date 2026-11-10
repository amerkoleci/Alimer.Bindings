file(GLOB SOURCE_FILES
    cgltf.h
    cgltf_write.h
    cgltf.c
)
add_library(cgltf ${SOURCE_FILES})

set_target_properties(cgltf PROPERTIES
    C_STANDARD 11
    WINDOWS_EXPORT_ALL_SYMBOLS ON
)

if (NATIVE_ENABLE_INSTALL)
    install(TARGETS cgltf
        ARCHIVE DESTINATION  ${CMAKE_INSTALL_LIBDIR}
        LIBRARY DESTINATION  ${CMAKE_INSTALL_LIBDIR}
        RUNTIME DESTINATION  ${CMAKE_INSTALL_BINDIR}
    )
endif()
