﻿using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace FileWebApi.Attributes;

[AttributeUsage(validOn: AttributeTargets.Method | AttributeTargets.Class)]
public class ApiKeyAuthorizeAttribute : Attribute, IAsyncActionFilter
{
    public bool Disabled { get; set; } = false;

    private const string APIKEYNAME = "ApiKey";
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (Disabled) await next();
        if (!context.HttpContext.Request.Headers.TryGetValue(APIKEYNAME, out StringValues extractedApiKey))
        {
            context.Result = new ContentResult()
            {
                StatusCode = 401,
                Content = "Api Key was not provided."
            };
            return;
        }
        string? apiKey = Environment.GetEnvironmentVariable(APIKEYNAME);
        if (apiKey == null)
        {
            context.Result = new ContentResult()
            {
                StatusCode = 401,
                Content = "Api Key was not provided by server."
            };
            return;
        }
        if (!apiKey.Equals(extractedApiKey))
        {
            context.Result = new ContentResult()
            {
                StatusCode = 401,
                Content = "Api Key is not valid."
            };
            return;
        }
        await next();
    }
}
