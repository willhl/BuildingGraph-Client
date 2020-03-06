using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuildingGraph.Client.Model.Types;
using Neo4j.Driver;

namespace BuildingGraph.Client.Neo4j
{
    internal sealed class Convert
    {
        public static object ToNeo4jType(object value)
        {

            if (value is IPoint3D)
            {
                var pt = value as IPoint3D;
                return new Point(9157, pt.X, pt.Y, pt.Z);
            }

            return value;

        }
    }
}
