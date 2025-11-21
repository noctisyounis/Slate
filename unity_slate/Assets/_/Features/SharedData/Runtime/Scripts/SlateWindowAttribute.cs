using System;

namespace SharedData.Runtime
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class SlateWindowAttribute : Attribute
    {
        public string categoryName { get; set; }
        public string entry { get; set; }

        public SlateWindowAttribute() { }

        public SlateWindowAttribute(string categoryName, string entry)
        {
            this.categoryName = categoryName;
            this.entry = entry;
        }
    }
}