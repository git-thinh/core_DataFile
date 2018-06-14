using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using ProtoBuf;

namespace Core
{
    [ProtoContract]
    [ProtoInclude(1, typeof(Field))]
    public class Model
    {
        [ProtoMember(1)]
        public Field[] Fields { get; set; }

        [ProtoMember(2)]
        public string Namespace { get; set; }

        [ProtoMember(3)]
        public string ClassName { get; set; }

        private string TypeFormat(string type)
        {
            string t = type;
            switch (type)
            {
                case "Int32":
                    t = "int";
                    break;
                case "Int64":
                    t = "long";
                    break;
                case "String":
                    t = "string";
                    break;
                case "":
                    break;
            }
            return t;
        }

        public override string ToString()
        {
            StringBuilder fi = new StringBuilder();
            foreach (var it in Fields)
                fi.Append(" public " + TypeFormat(it.Type) + " " + it.Name + " { get; set; } " + Environment.NewLine);
            return "using System; namespace " + Namespace + Environment.NewLine +
                " { [Serializable] " + Environment.NewLine +
                " public class " + ClassName + " { " + Environment.NewLine +
                fi.ToString() + Environment.NewLine + " } } ";
        }
    }

    [ProtoContract]
    public class Field
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Type { get; set; }
    }

    public static class DbType
    {
        private static ConcurrentDictionary<string, Type> storeType = null;

        static DbType()
        {
            if (storeType == null) 
                storeType = new ConcurrentDictionary<string, Type>(Environment.ProcessorCount * 2, 101);
        }

        public static bool Exist(string type_name)
        {
            return storeType.ContainsKey(type_name);
        }

        public static bool Add(Type type) 
        {
            return false;
        }

        public static bool Add(string source, string type_name, ILog log)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(type_name)) return false;

            try
            {
                if (DbType.Exist(type_name)) return false;

                CSharpCodeProvider provider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } });
                //CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");  
                CompilerParameters parameter = new CompilerParameters();
                // True - memory generation, false - external file generation
                parameter.GenerateInMemory = true;
                // True - exe file generation, false - dll file generation
                parameter.GenerateExecutable = false;
                parameter.ReferencedAssemblies.Add(@"System.dll");
                parameter.IncludeDebugInformation = false;

                CompilerResults result = provider.CompileAssemblyFromSource(parameter, source);
                if (result.Errors.HasErrors)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (CompilerError error in result.Errors)
                        sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
                    string err = sb.ToString();
                }
                else
                {
                    Assembly asm = result.CompiledAssembly;
                    Type type = asm.GetTypes()[0];
                    return storeType.TryAdd(type_name, type);
                }
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        public static Type Get(string type_Name)
        {
            Type type;
            storeType.TryGetValue(type_Name, out type);
            return type;
        }

    }
}
