using Microsoft.Extensions.DependencyInjection;

namespace Infra.CrossCutting.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ServiceInjectAttribute(ServiceLifetime lifetime) : Attribute
{
    public ServiceLifetime Lifetime { get; private set; } = lifetime;
}