namespace Shared.Dtos
{
    /// <summary>
    /// Standard API response wrapper for success responses
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? TraceId { get; set; }

        public ApiResponse() { }

        public ApiResponse(T data, string message = "", string? traceId = null)
        {
            Success = true;
            Data = data;
            Message = message;
            TraceId = traceId;
        }

        public static ApiResponse<T> SuccessResponse(T data, string message = "")
        {
            return new ApiResponse<T>(data, message);
        }
    }

    /// <summary>
    /// Standard API error response
    /// </summary>
    public class ApiError
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? TraceId { get; set; }
        public Dictionary<string, string[]>? ValidationErrors { get; set; }

        public ApiError() { }

        public ApiError(string message, string? errorCode = null, string? traceId = null)
        {
            Message = message;
            ErrorCode = errorCode;
            TraceId = traceId;
        }

        public static ApiError BadRequest(string message, string? traceId = null)
        {
            return new ApiError(message, "BAD_REQUEST", traceId);
        }

        public static ApiError NotFound(string message, string? traceId = null)
        {
            return new ApiError(message, "NOT_FOUND", traceId);
        }

        public static ApiError ValidationError(Dictionary<string, string[]> validationErrors, string? traceId = null)
        {
            return new ApiError("Validation failed", "VALIDATION_ERROR", traceId)
            {
                ValidationErrors = validationErrors
            };
        }

        public static ApiError InternalServerError(string message, string? traceId = null)
        {
            return new ApiError(message, "INTERNAL_SERVER_ERROR", traceId);
        }
    }

    /// <summary>
    /// Paginated API response
    /// </summary>
    public class PaginatedApiResponse<T> : ApiResponse<IEnumerable<T>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PaginatedApiResponse(
            IEnumerable<T> data,
            int pageNumber,
            int pageSize,
            int totalRecords,
            string message = "")
            : base(data, message)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalRecords = totalRecords;
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
        }
    }
}