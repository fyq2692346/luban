using Luban.Job.Cfg.Datas;
using Luban.Job.Cfg.Defs;
using Luban.Job.Cfg.Utils;
using Luban.Job.Common.RawDefs;
using Luban.Job.Common.Types;
using Luban.Job.Common.TypeVisitors;
using System;
using System.Collections.Generic;

namespace Luban.Job.Cfg.DataCreators
{
    class StringDataCreator : ITypeFuncVisitor<string, DType>
    {
        public static StringDataCreator Ins { get; } = new StringDataCreator();

        public DType Accept(TBool type, string x)
        {
            if (bool.TryParse(x, out var b))
            {
                return DBool.ValueOf(b);
            }
            else
            {
                throw new Exception($"{x} 不是bool类型");
            }
        }

        public DType Accept(TByte type, string x)
        {
            if (byte.TryParse(x, out var b))
            {
                return DByte.ValueOf(b);
            }
            else
            {
                throw new Exception($"{x} 不是byte类型");
            }
        }

        public DType Accept(TShort type, string x)
        {
            if (short.TryParse(x, out var b))
            {
                return DShort.ValueOf(b);
            }
            else
            {
                throw new Exception($"{x} 不是short类型");
            }
        }

        public DType Accept(TFshort type, string x)
        {
            if (short.TryParse(x, out var b))
            {
                return DFshort.ValueOf(b);
            }
            else
            {
                throw new Exception($"{x} 不是short类型");
            }
        }

        public DType Accept(TInt type, string x)
        {
            if (int.TryParse(x, out var b))
            {
                return DInt.ValueOf(b);
            }
            else
            {
                throw new Exception($"{x} 不是int类型");
            }
        }

        public DType Accept(TFint type, string x)
        {
            if (int.TryParse(x, out var b))
            {
                return DFint.ValueOf(b);
            }
            else
            {
                throw new Exception($"{x} 不是int类型");
            }
        }

        public DType Accept(TLong type, string x)
        {
            if (long.TryParse(x, out var b))
            {
                return DLong.ValueOf(b);
            }
            else
            {
                throw new Exception($"{x} 不是long类型");
            }
        }

        public DType Accept(TFlong type, string x)
        {
            if (long.TryParse(x, out var b))
            {
                return DFlong.ValueOf(b);
            }
            else
            {
                throw new Exception($"{x} 不是long类型");
            }
        }

        public DType Accept(TFloat type, string x)
        {
            if (float.TryParse(x, out var b))
            {
                return DFloat.ValueOf(b);
            }
            else
            {
                throw new Exception($"{x} 不是float类型");
            }
        }

        public DType Accept(TDouble type, string x)
        {
            if (double.TryParse(x, out var b))
            {
                return DDouble.ValueOf(b);
            }
            else
            {
                throw new Exception($"{x} 不是double类型");
            }
        }

        public DType Accept(TEnum type, string x)
        {
            return new DEnum(type, x);
        }

        public DType Accept(TString type, string x)
        {
            return DString.ValueOf(x);
        }

        public DType Accept(TBytes type, string x)
        {
            throw new NotSupportedException();
        }

        public DType Accept(TText type, string x)
        {
            var (key, text) = DataUtil.ExtractText(x);
            return new DText(key, text);
            //throw new NotSupportedException();
        }

        private static string[] TrySep(TType type, string x)
        {
            string[] strings = new[] { x };
            string sep = type.GetTag("sep");
            if (!string.IsNullOrEmpty(sep))
            {
                strings = DataUtil.SplitStringByAnySepChar(x, sep);
            }
            return strings;
        }
        public DType Accept(TBean type, string x)
        {
            string[] strings;
            var originBean = (DefBean)type.Bean;
            if (!string.IsNullOrEmpty(originBean.Sep))
            {
                strings = DataUtil.SplitStringByAnySepChar(x, originBean.Sep);
            }
            else
            {
                strings = TrySep(type, x);
            }
            return new DBean(type, originBean, CreateBeanFields(originBean, strings),x);
        }

        public DType Accept(TArray type, string x)
        {
            return new DArray(type, ReadList(type, type.ElementType, x),x);
        }

        public DType Accept(TList type, string x)
        {
            return new DList(type, ReadList(type, type.ElementType, x),x);
        }

        public DType Accept(TSet type, string x)
        {
            return new DSet(type, ReadList(type, type.ElementType, x));
        }

        public DType Accept(TMap type, string x)
        {
            string[] strings = TrySep(type, x);
            var datas = new Dictionary<DType, DType>();
            for (int i = 0; i < strings.Length; )
            {
                var key = type.KeyType.Apply(this, strings[i]);
                i++;
                var value = type.ValueType.Apply(this, strings[i]);
                i++;
                if (!datas.TryAdd(key, value))
                {
                    throw new InvalidExcelDataException($"map 的 key:{key} 重复");
                }
            }
            return new DMap(type, datas);
        }

        public DType Accept(TVector2 type, string x)
        {
            return DataUtil.CreateVector(type, x);
        }

        public DType Accept(TVector3 type, string x)
        {
            return DataUtil.CreateVector(type, x);
        }

        public DType Accept(TVector4 type, string x)
        {
            return DataUtil.CreateVector(type, x);
        }

        public DType Accept(TDateTime type, string x)
        {
            return DataUtil.CreateDateTime(x);
        }

        private List<DType> CreateBeanFields(DefBean bean, string[] strings)
        {
            var list = new List<DType>();
            for (int i = 0; i < bean.HierarchyFields.Count; i++)
            {
                DefField f = (DefField)bean.HierarchyFields[i];
                try
                {
                    list.Add(f.CType.Apply(this,strings[i]));
                }
                catch (DataCreateException dce)
                {
                    dce.Push(bean, f);
                    throw;
                } catch (Exception e)
                {
                    var dce = new DataCreateException(e, strings[i]);
                    dce.Push(bean, f);
                    throw dce;
                }
            }
            return list;
        }

        public List<DType> ReadList(TType type, TType eleType, string x)
        {
            var datas = new List<DType>();
            string[] strings = TrySep(type, x);
            for (int i = 0; i < strings.Length; i++)
            {
                datas.Add(eleType.Apply(this,strings[i]));
            }

            return datas;
        }
    }
}
