using Microsoft.EntityFrameworkCore;
using PlatformService.Data;
using PlatformService.Http;
using Polly;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//add automapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//add repository
builder.Services.AddScoped<IPlatformRepo, PlatformRepo>();

//add httpclient
builder.Services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>().AddTransientHttpErrorPolicy(policyBuilder =>
    policyBuilder.Or<TimeoutRejectedException>().WaitAndRetryAsync(3, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
    )).AddTransientHttpErrorPolicy(policyBuilder =>
    policyBuilder.Or<TimeoutRejectedException>().CircuitBreakerAsync(5, TimeSpan.FromSeconds(30))
).AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(5));

//add dbcontext

if (builder.Environment.IsProduction())
{
    Console.WriteLine("--> Using MSSQL Db");
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString("PlatformsConn")));
}
else
{
    Console.WriteLine("--> Using InMem Db");
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseInMemoryDatabase("InMem"));
}


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

PrepDb.PrepPopulation(app, app.Environment.IsProduction());

app.Run();
