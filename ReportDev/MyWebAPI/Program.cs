using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new OpenApiInfo {Title="iText SDK Example API", Version="v8"} ); 
});

var app = builder.Build();
if(app.Environment.IsDevelopment()) 
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "iText SDK Example API v8"));
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
