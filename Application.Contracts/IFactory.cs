using Microsoft.Extensions.Hosting;

namespace Application.Contracts;

public interface IFactory
{
    void AddFactory(IHostApplicationBuilder builder);
}