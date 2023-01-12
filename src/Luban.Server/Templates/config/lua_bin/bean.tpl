
local function InitTypes(methods)
    local readBool = methods.readBool
    local readByte = methods.readByte
    local readShort = methods.readShort
    local readFshort = methods.readFshort
    local readInt = methods.readInt
    local readFint = methods.readFint
    local readLong = methods.readLong
    local readFlong = methods.readFlong
    local readFloat = methods.readFloat
    local readDouble = methods.readDouble
    local readSize = methods.readSize

    local readString = methods.readString

    local function readVector2(bs)
        return { x = readFloat(bs), y = readFloat(bs) }
    end

    local function readVector3(bs)
        return { x = readFloat(bs), y = readFloat(bs), z = readFloat(bs) }
    end

    local function readVector4(bs)
        return { x = readFloat(bs), y = readFloat(bs), z = readFloat(bs), w = readFloat(bs) }
    end

    local function readList(bs, keyFun)
        local list = {}
        local v
        for i = 1, readSize(bs) do
            tinsert(list, keyFun(bs))
        end
        return list
    end

    local readArray = readList
    
    local function readIdCount(bs)
        local o = {
            id = readInt(bs),
            count = readInt(bs),
        }
        return o
    end
    
     local function readListInt(bs)
            local list = {}
            local v
            for i = 1, readSize(bs) do
                tinsert(list,readInt(bs))
            end
            return list
        end

    local function readSet(bs, keyFun)
        local set = {}
        local v
        for i = 1, readSize(bs) do
            tinsert(set, keyFun(bs))
        end
        return set
    end

    local function readMap(bs, keyFun, valueFun)
        local map = {}
        for i = 1, readSize(bs) do
            local k = keyFun(bs)
            local v = valueFun(bs)
            map[k] = v
        end
        return map
    end

    local function readNullableBool(bs)
        if readBool(bs) then
            return readBool(bs)
        end
    end

    local bean = {}
    do
    ---@class {{x.full_name}} {{if x.parent_def_type}}:{{x.parent}} {{end}}
    {{~ for field in x.export_fields~}}
     ---@field public {{field.name}} {{lua_comment_type field.ctype}}
    {{~end~}}
        local class = {}
        class._id = {{x.id}}
        class['{{x.lua_type_name_key}}'] = '{{x.full_name}}'
        local id2name = { {{for c in x.hierarchy_not_abstract_children}} [{{c.id}}] = '{{c.full_name}}', {{end}} }
{{~if x.is_abstract_type~}}
        class._deserialize = function(bs)
            local id = readInt(bs)
            return beans[id2name[id]]._deserialize(bs)
        end
{{~else~}}
        class._deserialize = function(bs)
            local o = {
        {{~ for field in x.hierarchy_export_fields ~}}
            {{~if !(need_marshal_bool_prefix field.ctype)~}}
            {{field.convention_name}} = {{lua_undering_deserialize 'bs' field.ctype}},
            {{~else~}}
            {{field.convention_name}} = {{if !field.ctype.is_bool}}readBool(bs) and {{lua_undering_deserialize 'bs' field.ctype}} or nil {{-else-}} readNullableBool(bs) {{-end-}},
            {{~end~}}
        {{~end~}}
            }
            setmetatable(o, class)
            return o
        end
{{~end~}}
        bean = class
    end
    return bean
    end

return { InitTypes = InitTypes }

