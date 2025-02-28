using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Configuration.AddAzureTableStorage(options =>
{
    options.ConfigurationKeys = new[] { "SFA.DAS.Funding.ApprenticeshipEarnings" };
    options.StorageConnectionString = builder.Configuration["ConfigurationStorageConnectionString"];
    options.EnvironmentName = builder.Configuration["EnvironmentName"];
    options.PreFixConfigurationKeys = false;
});

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var applicationSettings = new ApplicationSettings();
builder.Configuration.Bind(nameof(ApplicationSettings), applicationSettings);
builder.Services.AddEntityFrameworkForApprenticeships(applicationSettings, NotLocal(builder.Configuration));
builder.Services.AddSingleton(x => applicationSettings);
builder.Services.AddQueryServices().AddCommandDependencies().AddEventServices();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/ping");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static bool NotLocal(IConfiguration configuration)
{
    return !configuration!["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase);
}