using Bright.Serialization;
using Google.Protobuf;
using Luban.Job.Cfg.Datas;
using Luban.Job.Cfg.DataSources;
using Luban.Job.Cfg.Defs;
using System;
using System.Collections.Generic;
using System.IO;

namespace Luban.Job.Cfg.DataExporters;

public class ProtobufIndexKeyExportor
{
    public static ProtobufIndexKeyExportor Ins { get; } = new ProtobufIndexKeyExportor();

    public void WriteList(DefTable table, List<Record> datas, ByteBuf x)
    {
        x.WriteSize(datas.Count);
        var tableDataBuf = new ByteBuf(10 * 1024);
        long offset = 0;
        tableDataBuf.WriteSize(datas.Count);
        foreach (var d in datas)
        {
            var ms = new MemoryStream();
            var tableDatacos = new CodedOutputStream(ms);
            //tableDatacos.WriteTag(1, WireFormat.WireType.LengthDelimited);
            d.Data.Apply(ProtoBufBinOneExportor.Ins, tableDatacos);
            tableDatacos.Flush();
            string keyStr = "";
            foreach (IndexInfo index in table.IndexList)
            {
                DType key = d.Data.Fields[index.IndexFieldIdIndex];
                key.Apply(BinaryExportor.Ins, x);
                keyStr += key.ToString() + ",";
            }
            x.WriteSize((int)offset);
            Console.WriteLine($"table:{table.Name} key:{keyStr} offset:{offset}");
            offset += ms.Length;
        }
        x.WriteSize((int)offset);
    }
}