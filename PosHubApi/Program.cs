using PosHubApi.Data.DataAccess;
using PosHubApi.Data.Interfaces;
using PosHubApi.Data.Repositories;
using PosHubApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient<IPosHubAuthRepository, PosHubAuthRepository>();
builder.Services.AddHttpClient<ICatalogRepository, CatalogRepository>();
builder.Services.AddHttpClient<IWebhookEventRepository, WebhookEventRepository>();
builder.Services.AddSingleton<PosHubAuthDA>();
builder.Services.AddSingleton<ApiErrorDA>();
builder.Services.AddSingleton<CatalogDA>();
builder.Services.AddSingleton<WebhookEventDA>();


var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseCors(x=>x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseRouting();   
app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            
//app.UseHttpsRedirection();

app.Run();
