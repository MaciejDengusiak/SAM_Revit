﻿using Autodesk.Revit.DB;
using SAM.Core.Revit;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static Construction ToSAM(this HostObjAttributes hostObjAttributes, ConvertSettings convertSettings)
        {
            if (hostObjAttributes == null)
                return null;

            Construction result = convertSettings?.GetObject<Construction>(hostObjAttributes.Id);
            if (result != null)
                return result;

            string name = hostObjAttributes.Name;
            PanelType panelType = hostObjAttributes.PanelType();
            if (panelType == PanelType.Undefined)
                panelType = Query.PanelType((BuiltInCategory)hostObjAttributes.Category.Id.IntegerValue);

            Construction construction = Analytical.Query.DefaultConstruction(panelType);
            if(construction != null && (name.Equals(construction.Name) || name.Equals(construction.UniqueName())))
                result = new Construction(construction);
            else
                result = new Construction(hostObjAttributes.Name);

            result.UpdateParameterSets(hostObjAttributes, ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));
            
            //result.Add(Core.Revit.Query.ParameterSet(hostObjAttributes));

            convertSettings?.Add(hostObjAttributes.Id, result);

            if (panelType != PanelType.Undefined)
                result.SetValue(ConstructionParameter.DefaultPanelType, panelType.Text());
            else
                result.SetValue(ConstructionParameter.DefaultPanelType, null);

            return result;
        }

        public static Construction ToSAM_Construction(this ElementType elementType, ConvertSettings convertSettings)
        {
            if (elementType == null || elementType.Category == null)
            {
                return null;
            }

            if((BuiltInCategory)elementType.Category.Id.IntegerValue != BuiltInCategory.OST_Cornices)
            {
                return null;
            }

            Construction result = convertSettings?.GetObject<Construction>(elementType.Id);
            if (result != null)
                return result;

            PanelType panelType = PanelType.Wall;

            string name = elementType.Name;

            Construction construction = Analytical.Query.DefaultConstruction(panelType);
            if (construction != null && (name.Equals(construction.Name) || name.Equals(construction.UniqueName())))
                result = new Construction(construction);
            else
                result = new Construction(elementType.Name);

            result.UpdateParameterSets(elementType, ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));
            result.SetValue(ConstructionParameter.DefaultPanelType, panelType.Text());

            convertSettings?.Add(elementType.Id, result);
            return result;

        }
    }
}