add_library(p5ccs_core STATIC
    src/Version.cpp
    include/p5ccs/core/Version.h
)

target_include_directories(p5ccs_core PUBLIC
    ${CMAKE_CURRENT_SOURCE_DIR}/include
)

target_link_libraries(p5ccs_core PUBLIC
    Qt6::Core
)

target_compile_features(p5ccs_core PUBLIC cxx_std_20)
