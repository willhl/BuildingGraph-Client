//using System.Net.Http;

namespace BuildingGraph.Client
{
    public class MutationArg
    {
        public MutationArg(string typeName, string argName, object value)
        {
            TypeName = typeName;
            Name = argName;
            Value = value;
        }

        public string TypeName;
        public string Name;
        public object Value;
        public string FullArgName
        {
            get
            {
                return $"${Name}:{TypeName}";
            }
        }
    }

}
