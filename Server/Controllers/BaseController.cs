using Microsoft.AspNetCore.Mvc;
using Shared.Dtos;

namespace Server.Controllers
{
    /// <summary>
    /// Base controller with helper methods for consistent API responses
    /// </summary>
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected string TraceId => HttpContext.TraceIdentifier;

        protected ActionResult SuccessResponse<T>(T data, string message = "", int statusCode = 200)
        {
            var response = new ApiResponse<T>(data, message, TraceId);
            return StatusCode(statusCode, response);
        }

        protected ActionResult CreatedResponse<T>(T data, string location, string message = "")
        {
            Response.Headers.Location = location;
            var response = new ApiResponse<T>(data, message, TraceId);
            return StatusCode(201, response);
        }

        protected ActionResult ErrorResponse(string message, string errorCode, int statusCode)
        {
            var error = new ApiError(message, errorCode, TraceId);
            return StatusCode(statusCode, error);
        }

        protected ActionResult BadRequestResponse(string message)
        {
            return ErrorResponse(message, "BAD_REQUEST", 400);
        }

        protected ActionResult NotFoundResponse(string message)
        {
            return ErrorResponse(message, "NOT_FOUND", 404);
        }

        protected ActionResult InternalServerErrorResponse(string message)
        {
            return ErrorResponse(message, "INTERNAL_SERVER_ERROR", 500);
        }

        protected ActionResult ValidationErrorResponse(Dictionary<string, string[]> validationErrors)
        {
            var error = ApiError.ValidationError(validationErrors, TraceId);
            return StatusCode(400, error);
        }

        protected ActionResult PaginatedResponse<T>(
            IEnumerable<T> data,
            int pageNumber,
            int pageSize,
            int totalRecords,
            string message = "")
        {
            var response = new PaginatedApiResponse<T>(data, pageNumber, pageSize, totalRecords, message)
            {
                TraceId = TraceId
            };
            return Ok(response);
        }
    }
}

