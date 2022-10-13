using Luban.Job.Cfg.RawDefs;
using System;
using System.Collections.Generic;

namespace Luban.Job.Cfg.Defs
{

    public class DefType : CfgDefTypeBase
    {
        public DefType(CfgType b)
        {
            Name = b.Name;
            Alias = b.Alias;
            Namespace = b.Namespace;
        }

        public string Alias { get; }
        public override void Compile()
        {
            
        }
    }
}