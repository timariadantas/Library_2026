using FluentValidation;
using Library.Application.Services;
using Library.Domain.Repositories;
using Library.Domain.Interfaces;
using Library.Infrastructure.Repositories;
using Library.Infrastructure.DependencyInjection;
using Library.Application.Validators;
using Library.API.Middlewares;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;





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
        Title = "Library API ",
        Version = "v1",
        Description = "API para gerenciamento de livros da Maria Dantas"
    });
});


// Infrastructure (Escolhe qual banco )
builder.Services.AddInfrastructure(builder.Configuration);


// Services

builder.Services.AddScoped<IBookService, BookService>();


// Build App
var app = builder.Build();


/// Global Exception Middleware 
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


