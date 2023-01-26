using System.Collections.Generic;
using System.Linq;

namespace AIQXCommon.Auth
{
    public static class StaticRights
    {
        // Use Case
        public const string READ_USE_CASE = "read:use_case";
        public const string CREATE_USE_CASE = "create:use_case";
        public const string UPDATE_USE_CASE = "update:use_case";
        public const string DELETE_USE_CASE = "delete:use_case";
        public const string UPDATE_USE_CASE_STATUS = "update:use_case_status";

        // Use Case Form
        public const string UPDATE_USE_CASE_FORM_INITIAL_INFORMATION = "update:use_case_form_initial_information";
        public const string UPDATE_USE_CASE_FORM_FEASIBILITY_CHECK = "update:use_case_form_feasibility_check";
        public const string UPDATE_USE_CASE_FORM_DETAILS = "update:use_case_form_details";
        public const string UPDATE_USE_CASE_FORM_HARDWARE_DETAILS = "update:use_case_form_hardware_details";
        public const string UPDATE_USE_CASE_FORM_ORDERING = "update:use_case_form_ordering";

        // File
        public const string READ_FILE = "read:file";
        public const string CREATE_FILE = "create:file";
        public const string UPDATE_FILE = "update:file";
        public const string DELETE_FILE = "delete:file";

        public static IList<string> ALL_RIGHTS = new List<string>()
        {
            READ_USE_CASE,
            CREATE_USE_CASE,
            UPDATE_USE_CASE,
            DELETE_USE_CASE,
            UPDATE_USE_CASE_STATUS,
            UPDATE_USE_CASE_FORM_INITIAL_INFORMATION,
            UPDATE_USE_CASE_FORM_FEASIBILITY_CHECK,
            UPDATE_USE_CASE_FORM_DETAILS,
            UPDATE_USE_CASE_FORM_HARDWARE_DETAILS,
            UPDATE_USE_CASE_FORM_ORDERING,
            READ_FILE,
            CREATE_FILE,
            UPDATE_FILE,
            DELETE_FILE
        };

        public static List<string> ALL_RIGHTS_REFLECTION = typeof(StaticRights)
            .GetFields()
            .Where(field => field.FieldType == typeof(string))
            .Select(field => field.GetValue(field).ToString())
            .ToList();

        public static List<string> CheckRightsAndReturnInvalid(List<string> rights)
        {
            var invalidRights = rights.Where(r => !ALL_RIGHTS.Contains(r));
            return invalidRights.ToList();
        }
    }
}
