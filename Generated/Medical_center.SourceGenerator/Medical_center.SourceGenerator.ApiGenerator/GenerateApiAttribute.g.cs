#nullable enable
using System;

namespace Medical_center.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class GenerateApiAttribute : Attribute
    {
        public string Version { get; set; } = "1.0";
        public bool IncludeNavigations { get; set; } = false;
    }
}