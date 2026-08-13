add_library(p5ccs_editor STATIC
    src/CodeEditorWidget.cpp
    include/p5ccs/editor/CodeEditorWidget.h
)

target_include_directories(p5ccs_editor PUBLIC
    ${CMAKE_CURRENT_SOURCE_DIR}/include
)

target_link_libraries(p5ccs_editor PUBLIC
    Qt6::Core
    Qt6::Widgets
    p5ccs_core
)

target_compile_features(p5ccs_editor PUBLIC cxx_std_20)
