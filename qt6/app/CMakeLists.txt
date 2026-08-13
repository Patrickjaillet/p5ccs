qt_add_executable(P5CCS
    src/main.cpp
    src/MainWindow.cpp
    src/MainWindow.h
)

target_link_libraries(P5CCS PRIVATE
    Qt6::Core
    Qt6::Widgets
    p5ccs_core
    p5ccs_engine
    p5ccs_editor
    p5ccs_exportpipeline
)

set_target_properties(P5CCS PROPERTIES
    WIN32_EXECUTABLE ON
)
