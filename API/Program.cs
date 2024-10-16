using API.Extensions;
using API.Middlewares;
using Core.Entities.Auth;
using System.Text.Json.Serialization;

namespace API
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.ConfigureDbContext(builder.Configuration);
			builder.Services.RegisterServices();
			builder.Services.RegisterCors();
			builder.Services.AddControllers()
					.AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
			builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseCors("EnableCors");

			app.UseMiddleware<GlobalErrorHandler>();

			app.UseMiddleware<JwtTokenMiddleware>();

			app.MapControllers();

			app.Run();
		}
	}
}
