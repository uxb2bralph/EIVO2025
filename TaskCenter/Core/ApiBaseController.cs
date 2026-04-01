using CommonLib.Core.DataWork;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DataEntity;
using ModelCore.DTOs;

namespace TaskCenter.Core
{
    public class ApiBaseController : ControllerBase
    {
        //private MemoryStream? _responseBodyStream;
        //private Stream? _originalResponseBody;
        protected internal ModelSource? _dataSource;

        protected internal GenericDbContext<ApplicationDbContext>? models;
        protected ApplicationDbContext? db;

        public ApiBaseController(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base()
        {
            Logger = loggerFactory.CreateLogger(this.GetType());
            db = serviceProvider.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;
            models = new GenericDbContext<ApplicationDbContext>(db);
        }

        public ILogger Logger
        {
            get;
            private set;
        }

        /// <summary>
        /// 建立成功回應
        /// </summary>
        /// <typeparam name="T">資料型別</typeparam>
        /// <param name="data">回應資料</param>
        /// <param name="message">訊息</param>
        /// <returns>成功回應</returns>
        protected IActionResult CreateSuccessResponse<T>(T data, string message)
        {
            return Ok(new ResponseDto<T>
            {
                Success = true,
                Message = message,
                Data = data
            });
        }

        /// <summary>
        /// 建立成功回應（無資料）
        /// </summary>
        /// <param name="message">訊息</param>
        /// <returns>成功回應</returns>
        protected IActionResult CreateSuccessResponse(string message)
        {
            return Ok(new BaseResponseDto
            {
                Success = true,
                Message = message
            });
        }

        /// <summary>
        /// 建立錯誤回應
        /// </summary>
        /// <param name="statusCode">HTTP 狀態碼</param>
        /// <param name="message">錯誤訊息</param>
        /// <param name="errors">詳細錯誤訊息</param>
        /// <returns>錯誤回應</returns>
        protected IActionResult CreateErrorResponse(int statusCode, string message, IEnumerable<string>? errors = null)
        {
            return StatusCode(statusCode, new BaseResponseDto
            {
                Success = false,
                Message = message,
                Errors = errors
            });
        }

        /// <summary>
        /// 建立 NotFound 回應
        /// </summary>
        /// <param name="message">錯誤訊息</param>
        /// <returns>NotFound 回應</returns>
        protected IActionResult CreateNotFoundResponse(string message)
        {
            return NotFound(new BaseResponseDto
            {
                Success = false,
                Message = message
            });
        }

        /// <summary>
        /// 建立 BadRequest 回應
        /// </summary>
        /// <param name="message">錯誤訊息</param>
        /// <param name="errors">詳細錯誤訊息</param>
        /// <returns>BadRequest 回應</returns>
        protected IActionResult CreateBadRequestResponse(string message, IEnumerable<string>? errors = null)
        {
            return BadRequest(new BaseResponseDto
            {
                Success = false,
                Message = message,
                Errors = errors
            });
        }

        /// <summary>
        /// 建立 Unauthorized 回應
        /// </summary>
        /// <param name="message">錯誤訊息</param>
        /// <returns>Unauthorized 回應</returns>
        protected IActionResult CreateUnauthorizedResponse(string message)
        {
            return Unauthorized(new BaseResponseDto
            {
                Success = false,
                Message = message
            });
        }

        /// <summary>
        /// 建立 Created 回應
        /// </summary>
        /// <typeparam name="T">資料型別</typeparam>
        /// <param name="data">回應資料</param>
        /// <param name="message">訊息</param>
        /// <param name="location">資源位置</param>
        /// <returns>Created 回應</returns>
        protected IActionResult CreateCreatedResponse<T>(T data, string message, string? location = null)
        {
            return Created(location ?? "", new ResponseDto<T>
            {
                Success = true,
                Message = message,
                Data = data
            });
        }
    }

}
