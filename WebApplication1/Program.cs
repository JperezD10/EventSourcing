using ClassLibrary1;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = "mongodb+srv://jperezdemonty:f1L330pBRkZEKw4R@cluster0.y7d9k.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";
var databaseName = "EventSourcing"; 

builder.Services.AddSingleton<IMongoClient>(new MongoClient(connectionString));

builder.Services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));

builder.Services.AddScoped<EventStore>();
builder.Services.AddScoped<IPaymentIntentRepository, PaymentIntentRepository>();
builder.Services.AddScoped<IPaymentIntentService, PaymentIntentService>();
var app = builder.Build();

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
