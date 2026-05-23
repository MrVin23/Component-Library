using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Server.Models
{
    /// <summary>
    /// Parameters for pagination requests
    /// </summary>
    public class PaginationParameters
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;
        
        /// <summary>
        /// Current page number (1-based)
        /// </summary>
        public int PageNumber { get; set; } = 1;
        
        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize 
        { 
            get => _pageSize; 
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value; 
        }
        
        /// <summary>
        /// Optional sort property name
        /// </summary>
        public string? SortBy { get; set; }
        
        /// <summary>
        /// Sort direction (true for ascending, false for descending)
        /// </summary>
        public bool SortAscending { get; set; } = true;
    }
    
    /// <summary>
    /// Generic paginated response for API endpoints
    /// </summary>
    /// <typeparam name="T">Type of items being paginated</typeparam>
    public class PagedResponse<T> where T : class
    {
        /// <summary>
        /// Current page number
        /// </summary>
        public int PageNumber { get; set; }
        
        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; set; }
        
        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; }
        
        /// <summary>
        /// Total count of items
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// Flag indicating if there's a previous page
        /// </summary>
        public bool HasPrevious => PageNumber > 1;
        
        /// <summary>
        /// Flag indicating if there's a next page
        /// </summary>
        public bool HasNext => PageNumber < TotalPages;
        
        /// <summary>
        /// Collection of items in the current page
        /// </summary>
        public IEnumerable<T> Items { get; set; } = new List<T>();
    }

    /// <summary>
    /// Extension methods for pagination
    /// </summary>
    public static class PaginationExtensions
    {
        /// <summary>
        /// Paginates the query with a stable sort before Skip/Take (required by EF Core).
        /// Uses <see cref="PaginationParameters.SortBy"/> when set; otherwise <c>Id</c>.
        /// </summary>
        /// <typeparam name="T">Type of items being paginated</typeparam>
        /// <param name="query">The queryable to paginate</param>
        /// <param name="parameters">Pagination parameters</param>
        /// <returns>Paginated response</returns>
        public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
            this IQueryable<T> query,
            PaginationParameters parameters) where T : class
        {
            var orderedQuery = ApplyOrdering(query, parameters);

            var totalCount = await orderedQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize);

            var items = await orderedQuery
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResponse<T>
            {
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items
            };
        }

        private static IQueryable<T> ApplyOrdering<T>(IQueryable<T> query, PaginationParameters parameters)
            where T : class
        {
            var sortKey = string.IsNullOrWhiteSpace(parameters.SortBy) ? "Id" : parameters.SortBy.Trim();
            return parameters.SortAscending
                ? query.OrderBy(e => EF.Property<object>(e, sortKey))
                : query.OrderByDescending(e => EF.Property<object>(e, sortKey));
        }
    }
} 