﻿using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static ConvertSettings ConvertSettings()
        {
            return new ConvertSettings(true, true, ConvertType.New);
        }
    }
}
