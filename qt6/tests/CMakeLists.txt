find_package(Qt6 REQUIRED COMPONENTS Test)

qt_add_executable(p5ccs_core_tests test_Version.cpp)

target_link_libraries(p5ccs_core_tests PRIVATE
    Qt6::Core
    Qt6::Test
    p5ccs_core
)

add_test(NAME p5ccs_core_tests COMMAND p5ccs_core_tests)
