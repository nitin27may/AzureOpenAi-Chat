using GenAI.Api.Services;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddSingleton<IChatSessionService, ChatSessionService>();
builder.Services.AddScoped<IBlobService, BlobService>();
builder.Services.AddScoped<IDocumentIntelligenceService, DocumentIntelligenceService>();
builder.Services.AddScoped<IOpenApiService, OpenApiService>();

builder.Services.Configure<CosmosDbSettings>(builder.Configuration.GetSection("CosmosDbSettings"));
builder.Services.AddSingleton<ICosmosMessageService, CosmosMessageService>();



builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddControllers().AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("AllowAll");
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();