﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SAM.Geometry.Spatial;

using Autodesk.Revit.DB;


namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static List<Triangle3D> ToSAM_Triangle3Ds(this Mesh mesh)
        {
            List<Triangle3D> result = new List<Triangle3D>();
            for (int i = 0; i < mesh.NumTriangles; i++)
            {
                Triangle3D triangle3D = mesh.get_Triangle(i).ToSAM();
                result.Add(triangle3D);
            }
            return result;
        }
    }
}
