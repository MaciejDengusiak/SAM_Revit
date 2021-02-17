﻿using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static bool IsValidTagCategory(this BuiltInCategory builtInCategory_Tag, BuiltInCategory builtInCategory_Element)
        {
            if (builtInCategory_Element == BuiltInCategory.INVALID || builtInCategory_Tag == BuiltInCategory.INVALID)
                return false;

            if (builtInCategory_Tag == BuiltInCategory.OST_MultiCategoryTags)
                return true;

            switch(builtInCategory_Tag)
            {
                case BuiltInCategory.OST_AssemblyTags:
                    return builtInCategory_Element == BuiltInCategory.OST_Assemblies;
                case BuiltInCategory.OST_WallTags:
                    return builtInCategory_Element == BuiltInCategory.OST_Walls;
                case BuiltInCategory.OST_FloorTags:
                    return builtInCategory_Element == BuiltInCategory.OST_Floors;
                case BuiltInCategory.OST_MEPSpaceTags:
                    return builtInCategory_Element == BuiltInCategory.OST_MEPSpaces;
                case BuiltInCategory.OST_WindowTags:
                    return builtInCategory_Element == BuiltInCategory.OST_Windows;
            }

            return false;
        }
    }
}