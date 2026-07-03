using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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
        [JsonPropertyName("items")]
        public IEnumerable<T> Items { get; set; } = [];

        /// <summary>
        /// Total number of items
        /// </summary>
        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }

        /// <summary>
        /// Page size
        /// </summary>
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        [JsonPropertyName("totalPages")]
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// Whether there is a previous page
        /// </summary>
        [JsonPropertyName("hasPrevious")]
        public bool HasPrevious => PageNumber > 1;

        /// <summary>
        /// Whether there is a next page
        /// </summary>
        [JsonPropertyName("hasNext")]
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
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Error details if any
        /// </summary>
        [JsonPropertyName("errors")]
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
        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }
}
