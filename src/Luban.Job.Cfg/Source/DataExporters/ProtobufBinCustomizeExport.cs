using Google.Protobuf;
using Luban.Job.Cfg.Datas;
using Luban.Job.Cfg.DataSources;
using Luban.Job.Cfg.DataVisitors;
using Luban.Job.Cfg.Defs;
using Luban.Job.Common.Types;
using Luban.Job.Common.TypeVisitors;
using System;
using System.Collections.Generic;
using System.IO;

namespace Luban.Job.Cfg.DataExporters;

public class ProtobufBinCustomizeExport:IDataActionVisitor<CodedOutputStream>
{
    public static ProtobufBinCustomizeExport Ins { get; } = new();
    private void FreeMemoryStream(MemoryStream cos)
    {
        cos.Seek(0, SeekOrigin.Begin);
    }

    public void WriteList(DefTable table, List<Record> datas, MemoryStream dataBytes, MemoryStream structureBytes)
    {
        WriteStructure(table, structureBytes);
        WriteData(table, datas, dataBytes, structureBytes);
    }

    public void WriteStructure(DefTable table, MemoryStream structureBytes)
    {
        var valueTType = table.ValueTType;
        var exportFields = valueTType.Bean.Fields;
        var ms1 = new MemoryStream();
        var cos1 = new CodedOutputStream(ms1);
        
        for (int i = 0; i < exportFields.Count; i++)
        {
            var field = exportFields[i];
            var name = field.Name;
            cos1.WriteTag(1,WireFormat.WireType.LengthDelimited);
            cos1.WriteString(name);
        }
        for (int i = 0; i < exportFields.Count; i++)
        {
            var field = exportFields[i];
            var type = field.TypeName;
            cos1.WriteTag(2,WireFormat.WireType.LengthDelimited);
            cos1.WriteString(type);
        }
        cos1.Flush();
        structureBytes.Write(ms1.ToArray());
    }
    public void WriteData(DefTable table, List<Record> datas, MemoryStream dataBytes, MemoryStream structureBytes)
    {
        var sms = new MemoryStream();
        var scos = new CodedOutputStream(sms);
      
        
       
        DefField indexField =  table.IndexField;
        int id = indexField.AutoId;
    
        
        var ms1 = new MemoryStream();
        var cos1 = new CodedOutputStream(ms1);
        for (int i = 0; i < datas.Count; i++)
        {  
            scos.WriteTag(3,WireFormat.WireType.LengthDelimited);
            
            var sms1 = new MemoryStream();
            var scos1 = new CodedOutputStream(sms1);
            scos1.WriteTag(1,WireFormat.WireType.LengthDelimited);
            cos1.WriteTag(1,WireFormat.WireType.LengthDelimited);
            var ms = new MemoryStream();
            var cos = new CodedOutputStream(ms);
            SetRecord(datas[i], cos,scos1,id);
            long l1  = cos.Position;
            cos.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            ByteString bs = ByteString.FromStream(ms);
            cos1.WriteBytes(bs);
            long endcount = cos1.Position;
            long startCount = endcount - l1;
            scos1.WriteTag(2,WireFormat.WireType.Varint);
            scos1.WriteInt32((int)startCount);
            scos1.WriteTag(3,WireFormat.WireType.Varint);
            scos1.WriteInt32((int)endcount);
            scos1.Flush();
            sms1.Seek(0, SeekOrigin.Begin);
            scos.WriteBytes(ByteString.FromStream(sms1));
        }
        cos1.Flush();
       
        dataBytes.Write(ms1.ToArray());
       
       
        
        scos.Flush();
        structureBytes.Write(sms.ToArray());
    }
    
    public void WriteList(DefTable table, List<Record> datas, MemoryStream bytes)
    {
        var valueTType = table.ValueTType;
        var exportFields = valueTType.Bean.Fields;
        var ms1 = new MemoryStream();
        var cos1 = new CodedOutputStream(ms1);
    }
    

    public void SetRecord(Record record,CodedOutputStream x,CodedOutputStream structureBytes,int index)
    {
      
        for (int i = 0; i < record.Data.Fields.Count; i++)
        {
            x.WriteTag(1, WireFormat.WireType.LengthDelimited);
            var ms1 = new MemoryStream();
            var cos1 = new CodedOutputStream(ms1);
            SetData(record.Data.Fields[i], cos1);
            if (i == index - 1)
            {
                cos1.Flush();
                ms1.Seek(0, SeekOrigin.Begin);
                structureBytes.WriteBytes(ByteString.FromStream(ms1));
            }
            cos1.Flush();
            ms1.Seek(0, SeekOrigin.Begin);
            x.WriteBytes(ByteString.FromStream(ms1));
        }
        
    }

    public void SetData(DType data,CodedOutputStream x)
    {
        switch (data.TypeName)
        {
            case "int":
                x.WriteTag(1,WireFormat.WireType.Varint);
                data.Apply(this, x);
                break;
            case "string":
                x.WriteTag(2,WireFormat.WireType.LengthDelimited);
                data.Apply(this, x);
                break;
            case "float":
                x.WriteTag(3,WireFormat.WireType.Fixed32);
                data.Apply(this, x);
                break;
            case "bool":
                x.WriteTag(4,WireFormat.WireType.Varint);
                data.Apply(this, x);
                break;
            default:
                x.WriteTag(2,WireFormat.WireType.LengthDelimited);
                data.Apply(this, x);
                break;
        }
    }
    
    public void Accept(DBool type, CodedOutputStream x)
    {
        x.WriteBool(type.Value);
    }

    public void Accept(DByte type, CodedOutputStream x)
    {
        x.WriteInt32(type.Value);
    }

    public void Accept(DShort type, CodedOutputStream x)
    {
        x.WriteInt32(type.Value);
    }

    public void Accept(DFshort type, CodedOutputStream x)
    {
        x.WriteInt32(type.Value);
    }

    public void Accept(DInt type, CodedOutputStream x)
    {
        x.WriteInt32(type.Value);
    }

    public void Accept(DFint type, CodedOutputStream x)
    {
        x.WriteSFixed32(type.Value);
    }

    public void Accept(DLong type, CodedOutputStream x)
    {
        x.WriteInt64(type.Value);
    }

    public void Accept(DFlong type, CodedOutputStream x)
    {
        x.WriteSFixed64(type.Value);
    }

    public void Accept(DFloat type, CodedOutputStream x)
    {
        x.WriteFloat(type.Value);
    }

    public void Accept(DDouble type, CodedOutputStream x)
    {
        x.WriteDouble(type.Value);
    }

    public void Accept(DEnum type, CodedOutputStream x)
    {
        x.WriteInt32(type.Value);
    }

    public void Accept(DString type, CodedOutputStream x)
    {
        x.WriteString(type.Value);
    }

    public void Accept(DText type, CodedOutputStream x)
    {
        x.WriteString(type.TextOfCurrentAssembly);
    }

    public void Accept(DBytes type, CodedOutputStream x)
    {
        x.WriteBytes(ByteString.CopyFrom(type.Value));
    }

    public void Accept(DVector2 type, CodedOutputStream x)
    {
        x.WriteString(type.StringValue);
    }

    public void Accept(DVector3 type, CodedOutputStream x)
    {
        x.WriteString(type.StringValue);
    }

    public void Accept(DVector4 type, CodedOutputStream x)
    {
        x.WriteString(type.StringValue);
    }

    public void Accept(DDateTime type, CodedOutputStream x)
    {
        x.WriteInt32(type.UnixTimeOfCurrentAssembly);
    }

    public void Accept(DBean type, CodedOutputStream x)
    {
        x.WriteString(type.Value);
    }

    public void Accept(DArray type, CodedOutputStream x)
    {
        x.WriteString(type.Value);
    }

    public void Accept(DList type, CodedOutputStream x)
    {
        x.WriteString(type.Value);
    }

    public void Accept(DSet type, CodedOutputStream x)
    {
        throw new System.NotImplementedException();
    }

    public void Accept(DMap type, CodedOutputStream x)
    {
        throw new System.NotImplementedException();
    }
    
}