﻿using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

using SAM.Geometry.Revit;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static List<Panel> ToSAM(this HostObject hostObject)
        {
            Construction construction = ((HostObjAttributes)hostObject.Document.GetElement(hostObject.GetTypeId())).ToSAM();

            List<Geometry.Spatial.Face3D> face3Ds = hostObject.Profiles();

            List<Panel> result = new List<Panel>();

            foreach (Geometry.Spatial.Face3D face3D in face3Ds)
            {
                Panel panel = new Panel(construction, Query.PanelType(hostObject), face3D);
                panel.Add(Core.Revit.Query.ParameterSet(hostObject));
                result.Add(panel);
            }

            return result;
        }

        public static List<Panel> ToSAM_Panels(this RevitLinkInstance revitLinkInstance)
        {
            Document document_Source = revitLinkInstance.GetLinkDocument();

            LogicalOrFilter logicalOrFilter = new LogicalOrFilter((new List<System.Type> { typeof(Wall), typeof(Floor), typeof(RoofBase) }).ConvertAll(x => (ElementFilter)(new ElementClassFilter(x))));

            IEnumerable<HostObject> hostObjects = new FilteredElementCollector(document_Source).WherePasses(logicalOrFilter).Cast<HostObject>();
            if (hostObjects == null || hostObjects.Count() == 0)
                return null;

            List<Panel> result = new List<Panel>();
            foreach(HostObject hostObject in hostObjects)
            {
                List<Panel> panelList = hostObject.ToSAM();
                if (panelList != null && panelList.Count > 0)
                    result.AddRange(panelList);
            }

            return result;
        }
    }
}
