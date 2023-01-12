using Luban.Common.Protos;
using Luban.Job.Cfg.Defs;
using Luban.Job.Common.Defs;
using Luban.Job.Common.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Luban.Job.Cfg.Generate
{
    abstract class LuaCodeRenderBase : TemplateCodeRenderBase
    {
        public override void Render(GenContext ctx)
        {
            // DefAssembly.LocalAssebmly.CurrentLanguage = Common.ELanguage.LUA;
            // var file = RenderFileUtil.GetFileOrDefault(ctx.GenArgs.OutputCodeMonolithicFile, "Types.lua");
            // var content = this.RenderAll(ctx.ExportTypes);
            // var md5 = CacheFileUtil.GenMd5AndAddCache(file, string.Join('\n', content));
            // ctx.GenCodeFilesInOutputCodeDir.Add(new FileInfo() { FilePath = file, MD5 = md5 });
            GenerateCodeScatter(ctx);
        }

        protected void GenerateCodeScatter(GenContext ctx)
        {
            string genType = ctx.GenType;
            ctx.Render = this;
            ctx.Lan = GetLanguage(ctx);
            DefAssembly.LocalAssebmly.CurrentLanguage =  Common.ELanguage.LUA;
            foreach (var c in ctx.ExportTypes)
            {
                ctx.Tasks.Add(Task.Run(() =>
                {
                    string body = ctx.Render.RenderAny(c);
                    if (string.IsNullOrWhiteSpace(body))
                    {
                        return;
                    }

                    var content = FileHeaderUtil.ConcatAutoGenerationHeader(body, ctx.Lan);
                    var file = RenderFileUtil.GetDefTypePath(c.FullName, ctx.Lan);
                    var md5 = CacheFileUtil.GenMd5AndAddCache(file, content);
                    ctx.GenCodeFilesInOutputCodeDir.Add(new FileInfo(){FilePath = file,MD5 = md5});
                }));
            }
        }

        public override string Render(DefEnum e)
        {
            return base.Render(e);
        }

        public override string Render(DefBean b)
        {
            return base.Render(b);
        }

        public override string Render(DefTable p)
        {
            return "";
        }
    }
}
