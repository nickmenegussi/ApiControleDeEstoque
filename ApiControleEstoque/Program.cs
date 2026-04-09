var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Injeção de Dependência
builder.Services.AddScoped<ApiControleEstoque.Repository.IMovimentacoesEstoqueRepository, ApiControleEstoque.Repository.MovimentacoesEstoqueRepository>();
builder.Services.AddScoped<ApiControleEstoque.Services.IMovimentacoesEstoqueService, ApiControleEstoque.Services.MovimentacoesEstoqueService>();

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
