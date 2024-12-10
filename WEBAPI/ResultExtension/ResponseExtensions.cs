using DOM.Results;
using Microsoft.AspNetCore.Mvc;

namespace WEBAPI.ResultExtension;

public static class ResponseExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        if (result.FailureType == ResultFailureType.NotFound) return new NotFoundObjectResult(result.ErrorMessage);
        
        return new BadRequestObjectResult(new { ErrorMessage = result.ErrorMessage });
    }
}