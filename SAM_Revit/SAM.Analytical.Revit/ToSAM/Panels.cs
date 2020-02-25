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

                //LogicalOrFilter logicalOrFilter = new LogicalOrFilter(new List<ElementFilter>() { new ElementCategoryFilter(BuiltInCategory.OST_Windows), new ElementCategoryFilter(BuiltInCategory.OST_Doors), new ElementCategoryFilter(BuiltInCategory.OST_CurtainWallPanels) });
                LogicalOrFilter logicalOrFilter = new LogicalOrFilter(new List<ElementFilter>() { new ElementCategoryFilter(BuiltInCategory.OST_Windows), new ElementCategoryFilter(BuiltInCategory.OST_Doors) });

                IEnumerable<ElementId> elementIds = hostObject.GetDependentElements(logicalOrFilter);

                if(hostObject is Wall)
                {
                    List<Autodesk.Revit.DB.Panel> panels = Query.Panels((Wall)hostObject);
                    if(panels != null && panels.Count > 0)
                    {
                        List<ElementId> elementIds_Temp =  panels.ConvertAll(x => x.Id);
                        if (elementIds != null && elementIds.Count() > 0)
                            elementIds_Temp.AddRange(elementIds);

                        elementIds = elementIds_Temp;
                    }
                }

                if(elementIds != null && elementIds.Count() > 0)
                {
                    Geometry.Spatial.Plane plane = panel.PlanarBoundary3D.Plane;

                    foreach (ElementId elementId in elementIds)
                    {
                        Element element = hostObject.Document.GetElement(elementId);
                        if (element == null)
                            continue;

                        if (!(element is FamilyInstance))
                            continue;

                        List<Geometry.Spatial.Face3D> face3Ds_Dependent = Geometry.Revit.Convert.ToSAM_Face3Ds(element);
                        if (face3Ds_Dependent == null || face3Ds_Dependent.Count == 0)
                            continue;

                        List<Geometry.Planar.Point2D> point2Ds = new List<Geometry.Planar.Point2D>();
                        foreach(Geometry.Spatial.Face3D face3D_Dependent in face3Ds_Dependent)
                        {
                            Geometry.Spatial.IClosedPlanar3D closedPlanar3D = face3D_Dependent.GetExternalEdge();
                            if(closedPlanar3D is Geometry.Spatial.ICurvable3D)
                            {
                                List<Geometry.Spatial.ICurve3D> curve3Ds = ((Geometry.Spatial.ICurvable3D)closedPlanar3D).GetCurves();
                                foreach(Geometry.Spatial.ICurve3D curve3D in curve3Ds)
                                {
                                    Geometry.Spatial.ICurve3D curve3D_Temp = plane.Project(curve3D);
                                    point2Ds.Add(plane.Convert(curve3D_Temp.GetStart()));
                                    point2Ds.Add(plane.Convert(curve3D_Temp.GetEnd()));
                                }
                            }
                        }

                        Geometry.Planar.Rectangle2D rectangle2D = Geometry.Planar.Point2D.GetRectangle2D(point2Ds);

                        Aperture aperture = Modify.AddAperture(panel, element.FullName(), element.ApertureType(), plane.Convert(rectangle2D));
                        if(aperture != null)
                            aperture.Add(Core.Revit.Query.ParameterSet(element));
                    }
                }

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
