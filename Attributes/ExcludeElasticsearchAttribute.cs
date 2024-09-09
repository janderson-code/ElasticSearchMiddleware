using System;

namespace elasticsearch.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ExcludeElasticsearchAttribute : Attribute
    {
    }
}