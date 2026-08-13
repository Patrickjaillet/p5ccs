add_library(p5ccs_exportpipeline STATIC
    src/ExportJob.cpp
    include/p5ccs/exportpipeline/ExportJob.h
)

target_include_directories(p5ccs_exportpipeline PUBLIC
    ${CMAKE_CURRENT_SOURCE_DIR}/include
)

target_link_libraries(p5ccs_exportpipeline PUBLIC
    Qt6::Core
    p5ccs_core
)

target_compile_features(p5ccs_exportpipeline PUBLIC cxx_std_20)
