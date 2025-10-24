using PosHubApi.Data.DataAccess;
using PosHubApi.Data.Interfaces;
using PosHubApi.Data.Repositories;
using PosHubApi.Mapper;
using PosHubApi.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddHttpClient<IPosHubAuthRepository, PosHubAuthRepository>();
builder.Services.AddHttpClient<ICatalogRepository, CatalogRepository>();
builder.Services.AddHttpClient<IWebhookEventRepository, WebhookEventRepository>();
builder.Services.AddHttpClient<ILocationOrdersRepository, LocationOrdersRepository>();
builder.Services.AddScoped<PosHubAuthDA>();
builder.Services.AddScoped<ApiErrorDA>();
builder.Services.AddScoped<CatalogDA>();
builder.Services.AddScoped<WebhookEventDA>();
builder.Services.AddScoped<OrderEventDA>();
builder.Services.AddScoped<LogsDA>();
builder.Services.AddHttpClient<IOrderEventRepository,OrderEventRepository>();


WebApplication app = builder.Build();
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
