using SeidorSmallWebApi.Routes;
using SeidorSmallWebApi.Filters;

namespace SeidorSmallWebApi.Extensions;

public static class ApplicationExtensions
{
    public static void ConfigureEnvironment(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/" + ApiRoutes.Version + "/swagger.json", "SeidorAPI " + ApiRoutes.Version));
        }
        else
        {
            app.Use(async (context, next) =>
            {
                await next();
                if (context.Response.StatusCode == 404 && !Path.HasExtension(context.Request.Path.Value))
                {
                    context.Request.Path = "/index.html";
                    await next();
                }
            });

            app.UseHsts();
        }
    }

    public static void ConfigureCustomExceptionMiddleware(this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
    }
}
