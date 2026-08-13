add_library(p5ccs_engine STATIC
    src/SketchViewportWidget.cpp
    include/p5ccs/engine/SketchViewportWidget.h
)

target_include_directories(p5ccs_engine PUBLIC
    ${CMAKE_CURRENT_SOURCE_DIR}/include
)

target_link_libraries(p5ccs_engine PUBLIC
    Qt6::Core
    Qt6::Widgets
    p5ccs_core
)

target_compile_features(p5ccs_engine PUBLIC cxx_std_20)
