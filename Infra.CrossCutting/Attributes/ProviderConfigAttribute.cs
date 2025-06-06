namespace Infra.CrossCutting.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ProviderConfigAttribute(string section) : Attribute
{
    public string Section { get; private set; } = section;
}