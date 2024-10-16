using Core;
using System.Net;
using System.Text.Json;

namespace API.Middlewares
{
	public class GlobalErrorHandler
	{
		private readonly RequestDelegate next;
		private readonly ILogger<GlobalErrorHandler> logger;

		public GlobalErrorHandler(RequestDelegate next, ILogger<GlobalErrorHandler> logger)
        {
			this.next = next;
			this.logger = logger;
		}

		public async Task Invoke(HttpContext context)
		{
			try
			{
				await next(context);
			}
			catch (Exception ex)
			{
				var response = context.Response;
				response.ContentType = "application/json";

				switch (ex)
				{
					case DomainException e:
						response.StatusCode = (int)HttpStatusCode.BadRequest; break;
					case KeyNotFoundException e:
						response.StatusCode= (int)HttpStatusCode.NotFound; break;
					default:
						logger.LogError(ex, ex.Message);
						response.StatusCode = (int)HttpStatusCode.InternalServerError; break;
				}

				var result = JsonSerializer.Serialize(new { message = ex?.Message });
				await response.WriteAsync(result);
			}
		}
    }
}
