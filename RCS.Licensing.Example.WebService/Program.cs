using System;
using System.IO;
using RCS.Licensing.Example.WebService.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RCS.Licensing.Example.Provider;
using RCS.Licensing.Provider.Shared;
using System.Diagnostics;

namespace RCS.Licensing.Example.WebService;

public class Program
{
	public const string TextSerializeDateFormat = "s";

	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);
		var asm = typeof(Program).Assembly;
		builder.Services.AddCors(options =>
		{
			options.AddDefaultPolicy(policy =>
			{
				policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
			});
		});
		builder.Services.AddControllers(opt =>
		{
			opt.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
			opt.OutputFormatters.RemoveType<StringOutputFormatter>();
			opt.OutputFormatters.Add(new TextPlainOutputFormatter());
			opt.InputFormatters.Add(new TextPlainInputFormatter());
		})
		.AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true);
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new OpenApiInfo
			{
				Title = builder.Configuration["LicensingService:SwaggerTitle"],
				Version = "v1",
				Description = string.Format(builder.Configuration["LicensingService:SwaggerDescription"]!, asm.GetName().Version),
				Contact = new OpenApiContact()
				{
					Name = builder.Configuration["LicensingService:SwaggerContactName"],
					Url = new Uri(builder.Configuration["LicensingService:SwaggerContactUrl"]!),
					Email = builder.Configuration["LicensingService:SwaggerContactEmail"]
				}
			});
			var dir = new DirectoryInfo(AppContext.BaseDirectory);
			foreach (var file in dir.GetFiles("RCS.*.xml"))
			{
				c.IncludeXmlComments(file.FullName);
				Trace.WriteLine($"Include XML file {file.FullName}");
			}
			c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
			{
				Name = ExampleLicensingServiceClient.ApiKeyHeaderName,
				Type = SecuritySchemeType.ApiKey,
				Scheme = "basic",
				In = ParameterLocation.Header,
				Description = "Simple authorisation using a request header with an API Key."
			});
			c.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = "ApiKey"
						}
					},
					Array.Empty<string>()
				}
			});
		});

		var licprov = new ExampleLicensingProvider(
			builder.Configuration["LicensingService:AdoConnect"]!,
			builder.Configuration["LicensingService:SubscriptionId"],
			builder.Configuration["LicensingService:TenantId"],
			builder.Configuration["LicensingService:ApplicationId"],
			builder.Configuration["LicensingService:ClientSecret"]
		);
		builder.Services.AddSingleton(typeof(ILicensingProvider), licprov);

		var app = builder.Build();
		app.UseCors();
		app.UseSwagger();
		app.UseSwaggerUI();
		if (app.Environment.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
			// Use the following to see live error handling when debugging
			//app.UseExceptionHandler("/error");
		}
		else
		{
			app.UseExceptionHandler("/error");
		}
		//app.UseHttpsRedirection();
		app.UseAuthorization();
		app.MapControllers();
		app.Run();
	}
}
