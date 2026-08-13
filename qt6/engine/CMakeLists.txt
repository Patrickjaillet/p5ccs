find_package(Qt6 REQUIRED COMPONENTS WebEngineWidgets)

add_library(p5ccs_engine STATIC
    src/SketchViewportWidget.cpp
    include/p5ccs/engine/SketchViewportWidget.h
    src/SketchUrlSchemeHandler.cpp
    include/p5ccs/engine/SketchUrlSchemeHandler.h
)

target_include_directories(p5ccs_engine PUBLIC
    ${CMAKE_CURRENT_SOURCE_DIR}/include
)

target_link_libraries(p5ccs_engine PUBLIC
    Qt6::Core
    Qt6::Widgets
    Qt6::WebEngineWidgets
    p5ccs_core
)

target_compile_features(p5ccs_engine PUBLIC cxx_std_20)

qt_add_resources(p5ccs_engine "p5ccs_engine_resources"
    PREFIX "/p5ccs/engine"
    BASE "${CMAKE_CURRENT_SOURCE_DIR}/resources"
    FILES
        resources/index.html
        resources/p5.min.js
)
