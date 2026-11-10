CPMAddPackage(
    NAME box2d
    GITHUB_REPOSITORY erincatto/box2d
    GIT_TAG "v3.2.0"
    OPTIONS
        "BUILD_SHARED_LIBS ON"
        "BOX2D_DOUBLE_PRECISION OFF"
)
