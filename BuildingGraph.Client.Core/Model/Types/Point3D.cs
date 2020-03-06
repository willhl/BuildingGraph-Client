using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingGraph.Client.Model.Types
{
    public class Point3D : IPoint3D
    {
        private double[] coordinates = new double[3] { 0, 0, 0 };

        public Point3D(double x, double y, double z)
        {
            coordinates = new double[3] { x, y, z };
        }

        public double X { get { return coordinates[0]; } }
        public double Y { get { return coordinates[1]; } }
        public double Z { get { return coordinates[2]; } }

    }
}
