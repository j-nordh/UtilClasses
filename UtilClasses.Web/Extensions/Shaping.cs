using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Web;
using UtilClasses.Extensions.Strings;
using UtilClasses.Web.ResponseBroker;

namespace Hogia.TW.API.Extensions
{
    public static class Shaping
    {
        public static object Shape<T>(this T dto, string fields)
        {
            List<string> lstOfFields = new List<string>();

            if (fields != null)
            {
                lstOfFields = fields.ToLower().Split(',').ToList();
            }
            return dto.Shape(lstOfFields);
        }
        public static object Shape<T>(this T dto, List<string> lstOfFields)
        {
            if (lstOfFields == null || !lstOfFields.Any())
            {
                return dto;
            }
            else
            {
                // create a new ExpandoObject & dynamically create the properties for this object
                ExpandoObject objectToReturn = new ExpandoObject();
                foreach (var field in lstOfFields)
                {
                    // need to include public and instance, b/c specifying a binding flag overwrites the
                    // already-existing binding flags.
                    var fieldValue = dto.GetType()
                        .GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                        .GetValue(dto, null);
                    var fieldName = dto.GetType()
                        .GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                        .Name;

                    // add the field to the ExpandoObject
                    ((IDictionary<String, Object>)objectToReturn).Add(fieldName, fieldValue);
                }

                return objectToReturn;
            }
        }
    }
}