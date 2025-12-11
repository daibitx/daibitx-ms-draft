using System;

namespace Daibitx.DynamicApi.Abstraction.Attributes
{
    /// <summary>
    /// API document generate
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class ApiExplorerSettingsAttribute : Attribute
    {
        /// <summary>
        /// when set to true, the API will be ignored in the API documentation.
        /// </summary>
        public bool IgnoreApi { get; }

        /// <summary>
        /// get the group name for the API description.
        /// </summary>
        public string GroupName { get; }

        public ApiExplorerSettingsAttribute(bool ignoreApi, string groupName)
        {
            IgnoreApi = ignoreApi;
            GroupName = groupName;
        }
    }
}
