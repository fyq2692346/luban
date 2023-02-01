{{~
    name = x.name
    key_type = x.key_ttype
    value_type =  x.value_ttype
~}}
syntax = "proto3";

package {{x.namespace_with_top_module}};

option java_multiple_files = true;

import "{{x.value_type}}.proto";



message {{x.pb_full_name}} {
    repeated {{protobuf_define_type value_type}} data_list = 1 [packed = false];
}