using Luban.Common.Protos;
using Luban.Job.Cfg.Cache;
using Luban.Job.Cfg.Utils;
using Luban.Job.Common.Generate;
using Luban.Job.Common.Utils;
using System.Threading.Tasks;

namespace Luban.Job.Cfg.Generate
{
    [Render("data_protobuf_customize_bin")]
    class DataCustomizeRender : DataRenderBase
    {
        public override void Render(GenContext ctx)
        {
            string genType = ctx.GenType;
            foreach (var table in ctx.ExportTables)
            {
                ctx.Tasks.Add(Task.Run(() =>
                {
                    string file = table.OutputDataFile;
                    string dataFile =  RenderFileUtil.GetOutputFileName(genType,  file, ctx.GenArgs.OutputDataFileExtension);
                    string structureFile =RenderFileUtil.GetOutputFileName(genType,  file + "Bidx", ctx.GenArgs.OutputDataFileExtension);
                    
                    var records = ctx.Assembly.GetTableExportDataList(table);
                    string md5Bidx = "";
                    if (!FileRecordCacheManager.Ins.TryGetRecordOutputData(table, records, genType, out string md5))
                    {
                        var contentList = DataExporterUtil.ToOutputDataList(table, records, genType);
                        md5 = CacheFileUtil.GenStringOrBytesMd5AndAddCache(dataFile, contentList[0]);
                        md5Bidx = CacheFileUtil.GenStringOrBytesMd5AndAddCache(structureFile, contentList[1]);
                        FileRecordCacheManager.Ins.AddCachedRecordOutputData(table, records, genType, md5);
                        FileRecordCacheManager.Ins.AddCachedRecordOutputData(table, records, genType, md5Bidx);
                    }
                    ctx.GenDataFilesInOutputDataDir.Add(new FileInfo() { FilePath = dataFile, MD5 = md5 });
                    ctx.GenDataFilesInOutputDataDir.Add(new FileInfo() { FilePath = structureFile, MD5 = md5Bidx });
                }));
            }
        }
    }
}