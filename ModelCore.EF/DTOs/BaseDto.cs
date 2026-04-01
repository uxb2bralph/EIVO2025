using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelCore.DTOs
{
    /// <summary>
    /// Base DTO for paged results
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public class PagedResultDto<T>
    {
        /// <summary>
        /// Items in the current page
        /// </summary>
        public IEnumerable<T> Items { get; set; } = [];

        /// <summary>
        /// Total number of items
        /// </summary>
        public int TotalCount { get; set; }

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
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// Whether there is a previous page
        /// </summary>
        public bool HasPrevious => PageNumber > 1;

        /// <summary>
        /// Whether there is a next page
        /// </summary>
        public bool HasNext => PageNumber < TotalPages;
    }

    /// <summary>
    /// Base response DTO
    /// </summary>
    public class BaseResponseDto
    {
        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Error details if any
        /// </summary>
        public IEnumerable<string>? Errors { get; set; }
    }

    /// <summary>
    /// Generic response DTO with data
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    public class ResponseDto<T> : BaseResponseDto
    {
        /// <summary>
        /// Response data
        /// </summary>
        public T? Data { get; set; }
    }
}
