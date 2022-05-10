﻿using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static BuiltInCategory? BuiltInCategory(this IParameterizedSAMObject parameterizedSAMObject)
        {
            if (parameterizedSAMObject == null)
                return null;

            if(!parameterizedSAMObject.TryGetValue(ElementParameter.RevitId, out IntegerId integerId))
            {
                return null;
            }

            if(!integerId.TryGetValue(RevitIdParameter.CategoryId, out int id))
            {
                return null;
            }

            return (BuiltInCategory)id;

        }
    }
}