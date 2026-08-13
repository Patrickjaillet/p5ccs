cmake_minimum_required(VERSION 3.21)

project(P5CCS
    VERSION 2.0.0
    DESCRIPTION "Processing 5 - Creative Coding Station (Qt6 rewrite)"
    LANGUAGES CXX
)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTORCC ON)
set(CMAKE_AUTOUIC ON)

if(NOT CMAKE_BUILD_TYPE AND NOT CMAKE_CONFIGURATION_TYPES)
    set(CMAKE_BUILD_TYPE "Debug" CACHE STRING "Build type" FORCE)
endif()

find_package(Qt6 REQUIRED COMPONENTS Core Widgets WebEngineWidgets)

enable_testing()

add_subdirectory(core)
add_subdirectory(engine)
add_subdirectory(editor)
add_subdirectory(exportpipeline)
add_subdirectory(app)
add_subdirectory(tests)
