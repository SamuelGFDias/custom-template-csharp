using System.Reflection;
using Infra.CrossCutting.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Infra.IoC;

public static class IoCManager
{
    private static readonly List<object> ExcludesInterfaces = new();

    #region Public Methods

    public static void AddServices(
        this IServiceCollection services,
        Type typeContractInterface,
        Type typeContractImplementation
    )
    {
        ExcludesInterfaces.Add(typeContractInterface);
        ExcludesInterfaces.Add(typeContractImplementation);

        List<Type> interfaces = GetInterfacesAssignableFrom(typeContractInterface);

        foreach (Type currentInterface in interfaces)
            services.AddImplementations(currentInterface, typeContractImplementation);

        ExcludesInterfaces.Clear();
    }

    public static void AddServices<TInterface, TImplementation>(this IServiceCollection services)
    {
        services.AddServices(typeof(TInterface), typeof(TImplementation));
    }

    public static void AddServicesByInterface(this IServiceCollection services, Type typeContractInterface)
    {
        ExcludesInterfaces.Add(typeContractInterface);

        List<Type> implementationTypes = GetImplementations(typeContractInterface, typeContractInterface);

        foreach (Type impl in implementationTypes)
            services.InjectService(impl);

        ExcludesInterfaces.Clear();
    }

    public static void AddServicesByInterface<TInterface>(this IServiceCollection services)
    {
        services.AddServicesByInterface(typeof(TInterface));
    }

    public static void AddProviders(this IHostApplicationBuilder builder, Type interfaceProviderType)
    {
        List<Type> providers = interfaceProviderType.GetProviders();

        foreach (Type provider in providers)
            builder.InjectProvider(provider);
    }

    public static void AddProviders<TInterfaceProvider>(this IHostApplicationBuilder builder)
    {
        builder.AddProviders(typeof(TInterfaceProvider));
    }

    #endregion

    #region Private Methods

    private static void AddImplementations(
        this IServiceCollection services,
        Type typeInterface,
        Type typeImplementation
    )
    {
        List<Type> implementations = GetImplementations(typeInterface, typeImplementation);

        if (implementations.Count == 0) return;

        foreach (Type currentImplementation in implementations)
            services.InjectService(currentImplementation);
    }

    private static List<Type> GetInterfacesAssignableFrom(Type typeInterface)
    {
        return typeInterface.Assembly.GetTypes().Where(x => x.IsInterface && !ExcludesInterfaces.Contains(x)).ToList();
    }

    private static List<Type> GetImplementations(Type typeInterface, Type typeContractImplementation)
    {
        Type[] assemblyTypes = typeContractImplementation.Assembly.GetTypes();

        List<Type> implementTypes = assemblyTypes.Where(x => x is
        {
            IsInterface: false, IsGenericType: false,
            IsAbstract: false,
            IsClass: true
        }
                                                          && typeInterface.IsAssignableFrom(x))
                                                 .ToList();

        if (typeInterface.IsGenericTypeDefinition)
            implementTypes.AddRange(
                assemblyTypes.Where(x => x is { IsClass: true, IsAbstract: false, IsGenericType: false }
                                      && x.GetInterfaces()
                                          .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeInterface)));
        else
            implementTypes.AddRange(
                assemblyTypes.Where(x => x is { IsClass: true, IsAbstract: false }
                                      && x.GetInterfaces()
                                          .Any(i => i == typeInterface
                                                 || (i.IsGenericType
                                                  && typeInterface.IsGenericType
                                                  && i.GetGenericTypeDefinition()
                                                  == typeInterface.GetGenericTypeDefinition()))));


        return implementTypes.Distinct().ToList();
    }

    private static void InjectService(this IServiceCollection services, Type typeImplementation)
    {
        var lifetime = ServiceLifetime.Scoped;

        if (typeImplementation.GetCustomAttribute<ServiceInjectAttribute>() is { } attribute)
            lifetime = attribute.Lifetime;

        List<Type> interfaces = typeImplementation.GetInterfaces().Where(i => !ExcludesInterfaces.Contains(i)).ToList();

        foreach (Type type in interfaces)
        {
            services.TryAdd(new ServiceDescriptor(type, typeImplementation, lifetime));
        }

        if (interfaces.Count == 0)
        {
            services.TryAdd(new ServiceDescriptor(typeImplementation, typeImplementation, lifetime));
        }
    }

    private static List<Type> GetProviders(this Type interfaceProviderType)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        return assemblies.SelectMany(a => a.GetTypes())
                         .Where(t => t is { IsClass: true, IsAbstract: false, IsInterface: false, IsGenericType: false }
                                  && interfaceProviderType.IsAssignableFrom(t))
                         .ToList();
    }

    private static ProviderConfigAttribute? GetProviderConfigAttribute(this Type obj)
    {
        return obj.GetCustomAttributes(typeof(ProviderConfigAttribute), false).FirstOrDefault() as
                   ProviderConfigAttribute;
    }

    private static MethodInfo GetConfigureMethod()
    {
        return typeof(OptionsConfigurationServiceCollectionExtensions).GetMethods()
                                                                      .First(m => m is
                                                                      {
                                                                          Name: "Configure", IsGenericMethod: true
                                                                      });
    }

    private static void InjectProvider(this IHostApplicationBuilder builder, Type providerType)
    {
        ProviderConfigAttribute? section = providerType.GetProviderConfigAttribute();

        if (section is null) return;

        MethodInfo configureMethod = GetConfigureMethod();

        MethodInfo genericMethod = configureMethod.MakeGenericMethod(providerType);

        genericMethod.Invoke(null, [builder.Services, builder.Configuration.GetSection(section.Section)]);
    }

    #endregion
}