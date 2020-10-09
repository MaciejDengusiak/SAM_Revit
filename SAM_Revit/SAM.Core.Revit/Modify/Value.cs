﻿using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static bool Value(this Parameter parameter, object value)
        {
            if (parameter == null || parameter.IsReadOnly)
                return false;

            switch (parameter.StorageType)
            {
                case StorageType.Double:
                    return Value_Double(parameter, value);

                case StorageType.ElementId:
                    return Value_ElementId(parameter, value);

                case StorageType.Integer:
                    return Value_Integer(parameter, value);

                case StorageType.None:
                    return Value_None(parameter, value);

                case StorageType.String:
                    return Value_String(parameter, value);
            }

            return false;
        }

        public static bool Value(this SAMObject sAMObject, Element element, string parameterName_SAMObject, string parameterName_Element)
        {
            if (string.IsNullOrWhiteSpace(parameterName_SAMObject) || string.IsNullOrWhiteSpace(parameterName_Element) || sAMObject == null || element == null)
                return false;

            Parameter parameter = element.LookupParameter(parameterName_Element);
            if (parameter == null)
                return false;

            object value;
            if (!Core.Query.TryGetValue(sAMObject, parameterName_SAMObject, out value))
                return false;

            return Value(parameter, value);
        }

        private static bool Value_String(this Parameter parameter, object value)
        {
            if (parameter == null)
                return false;

            if (value == null)
            {
                string value_Temp = null;
                parameter.Set(value_Temp);
            }
            else if (value is string)
            {
                parameter.Set((string)value);
            }
            else
            {
                parameter.Set(value.ToString());
            }

            return true;
        }

        private static bool Value_None(this Parameter parameter, object value)
        {
            return false;
        }

        private static bool Value_Integer(this Parameter parameter, object value)
        {
            if (parameter == null || value == null)
                return false;

            if (value is int)
            {
                //Check if parameter is Workset parameter -> If Workset parameter then change only if Workset with Id exists
                if (parameter.Id.IntegerValue == (int)BuiltInParameter.ELEM_PARTITION_PARAM)
                {
                    WorksetTable worksetTable = parameter.Element?.Document?.GetWorksetTable();
                    if (worksetTable == null)
                        return false;

                    WorksetId worksetId = new WorksetId((int)value);
                    if (WorksetId.InvalidWorksetId == worksetId)
                        return false;

                    //TODO: Double check if workset is valid!
                    Workset workset = worksetTable.GetWorkset(worksetId);
                    if (workset == null || workset.Kind != WorksetKind.UserWorkset)
                        return false;
                }

                parameter.Set((int)value);
                return true;
            }
            else if (value is string)
            {
                string value_Temp = (string)value;
                int @int;
                if (int.TryParse(value_Temp, out @int))
                {
                    parameter.Set(@int);
                    return true;
                }

                //YesNo Type parameter
                if (parameter.Definition.ParameterType == Autodesk.Revit.DB.ParameterType.YesNo)
                {
                    value_Temp = value_Temp.ToUpper().Trim();

                    if (value_Temp.Equals("Y") || value_Temp.Equals("YES") || value_Temp.Equals("+") || value_Temp.Equals("TRUE"))
                    {
                        parameter.Set(1);
                        return true;
                    }

                    if (value_Temp.Equals("N") || value_Temp.Equals("NO") || value_Temp.Equals("-") || value_Temp.Equals("FALSE"))
                    {
                        parameter.Set(0);
                        return true;
                    }

                    return false;
                }
            }
            else if (value is bool)
            {
                if ((bool)value)
                    parameter.Set(1);
                else
                    parameter.Set(0);

                return true;
            }
            else if (value is IntegerId)
            {
                parameter.Set(((IntegerId)value).Id);
                return true;
            }

            return false;
        }

        private static bool Value_ElementId(this Parameter parameter, object value)
        {
            if (parameter == null)
                return false;

            if (value == null)
            {
                parameter.Set(ElementId.InvalidElementId);
                return true;
            }
            else if (value is IntegerId)
            {
                parameter.Set(((IntegerId)value).ToRevit());
                return true;
            }
            else if (value is int)
            {
                parameter.Set(new ElementId((int)value));
                return true;
            }
            else if (value is string)
            {
                string value_Temp = (string)value;
                int @int;
                if (!int.TryParse(value_Temp, out @int))
                    return false;

                parameter.Set(new ElementId(@int));
                return true;
            }

            return false;
        }

        private static bool Value_Double(this Parameter parameter, object value)
        {
            if (parameter == null || value == null)
                return false;

            double value_Temp = double.NaN;
            if (value is string)
            {
                if (!double.TryParse((string)value, out value_Temp))
                    return false;
            }
            else if (value is double)
            {
                value_Temp = (double)value;
            }
            else if (value is int)
            {
                value_Temp = System.Convert.ToDouble(value);
            }

            if (double.IsNaN(value_Temp))
                return false;

            if (parameter.Definition.ParameterType == Autodesk.Revit.DB.ParameterType.Invalid)
            {
                parameter.Set(value_Temp);
                return true;
            }

            value_Temp = Units.Revit.Convert.ToRevit(value_Temp, parameter.Definition.ParameterType);

            if (double.IsNaN(value_Temp))
                return false;

            parameter.Set(value_Temp);
            return true;
        }
    }
}