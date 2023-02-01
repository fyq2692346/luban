syntax = "proto3";

package {{x.top_module}};

option java_multiple_files = true;

{{~
    name = x.name
    parent_def_type = x.parent_def_type
    export_fields = x.export_fields
    hierarchy_export_fields = x.hierarchy_export_fields
~}}

{{~for f in x.bean_export_types ~}}
    {{~if f.is_bean ~}}
import "{{f.bean.pb_full_name}}.proto";
    {{~else~}}
    {{~if f.is_collection ~}}
    {{~if f.element_type.is_bean~}}
import "{{f.element_type.bean.pb_full_name}}.proto";
    {{~end~}}
    {{~end~}}
    {{~end~}}
{{~end~}}

message {{x.pb_full_name}} {
{{~if x.is_abstract_type ~}}
    oneof value {
        {{~for c in x.hierarchy_not_abstract_children~}}
        {{c.pb_full_name}} {{c.name}} = {{c.auto_id}};
        {{~end~}}
    }
{{~else~}}
    {{~for f in hierarchy_export_fields ~}}
    {{protobuf3_pre_decorator f.ctype}} {{protobuf_define_type f.ctype}} {{f.name}} = {{f.auto_id}} {{protobuf_suffix_options f.ctype}};
    {{~end~}}
{{~end~}}
}