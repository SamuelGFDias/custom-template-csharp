using Api.Factories;
using Api.Factory;

var builder = BuilderFactory.GenerateWebApplicationBuilder(args);
var app = builder.CustomBuild();

app.MapGet("/", () => "Hello World!");

app.Run();
