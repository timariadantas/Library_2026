using FluentValidation;
using Library.Application.Services;
using Library.Domain.Repositories;
using Library.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;
using Library.Application.Validators;
using Library.API.Middlewares;
using FluentValidation.AspNetCore;
using Library.Infrastructure.Data;
using Library.Domain.Interfaces;



var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateBookRequestValidator>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Library API",
        Version = "v1",
        Description = "API para gerenciamento de livros"
    });
});


// Dependency Injection
builder.Services.AddScoped<IDbConnectionFactory>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    return new NpgsqlConnectionFactory(connectionString!);
});

builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();
builder.Services.AddScoped <IBookService,BookService>();

// Build App
var app = builder.Build();

// Global Exception Middleware 
app.UseMiddleware<GlobalExceptionMiddleware>();
//Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


