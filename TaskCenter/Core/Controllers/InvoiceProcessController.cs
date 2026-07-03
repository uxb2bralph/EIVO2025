using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelCore.DataEntity;
using ModelCore.Models.ViewModel;
using ModelCore.InvoiceManagement;
using CommonLib.Utility;
using TaskCenter.Helper.RequestAction;
using System.Linq;
using ApplicationResource;
using ModelCore.DTOs;
using ModelCore.Helper;

namespace TaskCenter.Core.Controllers
{
    /// <summary>
    /// Invoice Query API Controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class InvoiceProcessController : ApiBaseController
    {
        public InvoiceProcessController(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) 
            : base(serviceProvider, loggerFactory)
        {
        }

        private Organization? ValidateAndGetOrganization(InvoiceDataQueryViewModel viewModel)
        {
            /// TODO: Validate organization and authentication, �U�z��k�ܤ@
            /// 1. �ϥ� viewModel ������ƶi������
            /// 2. �ϥ� HttpContext �����{�Ҹ�T�i������
            /// 3. Header ���� API Key ����, Authorization: Bearer {token}
            return null;
        }

        /// <summary>
        /// Query invoice data
        /// </summary>
        /// <param name="viewModel">Invoice query view model</param>
        /// <returns>Invoice data result</returns>
        [HttpPost("invoice")]
        [ProducesResponseType(typeof(ResponseDto<object>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 401)]
        public IActionResult QueryInvoice([FromBody] InvoiceDataQueryViewModel viewModel)
        {
            try
            {
                // Validate request and get organization
                Organization? agent = ValidateAndGetOrganization(viewModel);

                if (agent == null)
                {
                    if (!ModelState.IsValid)
                    {
                        var errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage);
                        return BadRequest(new BaseResponseDto
                        {
                            Success = false,
                            Message = "Request validation failed",
                            Errors = errors
                        });
                    }
                    return Unauthorized(new BaseResponseDto
                    {
                        Success = false,
                        Message = "Invalid organization or authentication"
                    });
                }

                // Get invoice items - using models from SampleController
                IQueryable<InvoiceItem> items = models!.GetInvoiceByAgent(models!.GetTable<InvoiceItem>(), agent.CompanyID);

                bool effective = false;
                items = items.InquireInvoice(viewModel, models, ref effective);

                // Handle count query
                if (viewModel.QueryType == DataQueryType.CountInvoice)
                {
                    return Ok(new ResponseDto<object>
                    {
                        Success = true,
                        Message = "Query completed successfully",
                        Data = new { TotalCount = items.Count() }
                    });
                }

                // Handle data query with pagination
                if (viewModel.PageIndex > 0 && viewModel.PageSize > 0)
                {
                    items = items.Skip((viewModel.PageIndex.Value - 1) * viewModel.PageSize.Value)
                        .Take(viewModel.PageSize.Value);
                }

                var dataItems = items.AsNoTracking().ToList().Select(c => c.CreateF0401(true)).ToList();

                return Ok(new ResponseDto<object>
                {
                    Success = true,
                    Message = "Invoice data retrieved successfully",
                    Data = dataItems
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error querying invoice data");
                return StatusCode(500, new BaseResponseDto
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new[] { ex.Message }
                });
            }
        }

        /// <summary>
        /// Query void invoice data
        /// </summary>
        /// <param name="viewModel">Invoice query view model</param>
        /// <returns>Void invoice data result</returns>
        [HttpPost("void-invoice")]
        [ProducesResponseType(typeof(ResponseDto<object>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 401)]
        public IActionResult QueryVoidInvoice([FromBody] InvoiceDataQueryViewModel viewModel)
        {
            try
            {
                Organization? agent = ValidateAndGetOrganization(viewModel);

                if (agent == null)
                {
                    if (!ModelState.IsValid)
                    {
                        var errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage);
                        return BadRequest(new BaseResponseDto
                        {
                            Success = false,
                            Message = "Request validation failed",
                            Errors = errors
                        });
                    }
                    return Unauthorized(new BaseResponseDto
                    {
                        Success = false,
                        Message = "Invalid organization or authentication"
                    });
                }

                IQueryable<InvoiceItem> items = models!.GetInvoiceByAgent(models!.GetTable<InvoiceItem>(), agent.CompanyID);

                bool effective = false;
                items = items.InquireVoidInvoice(viewModel, models, ref effective);

                if (viewModel.QueryType == DataQueryType.CountVoidInvoice)
                {
                    return Ok(new ResponseDto<object>
                    {
                        Success = true,
                        Message = "Query completed successfully",
                        Data = new { TotalCount = items.Count() }
                    });
                }

                if (viewModel.PageIndex > 0 && viewModel.PageSize > 0)
                {
                    items = items.Skip((viewModel.PageIndex.Value - 1) * viewModel.PageSize.Value)
                        .Take(viewModel.PageSize.Value);
                }

                var dataItems = items.AsNoTracking().ToList().Select(c => c.CreateCancelInvoiceMIG(true)).ToList();

                return Ok(new ResponseDto<object>
                {
                    Success = true,
                    Message = "Void invoice data retrieved successfully",
                    Data = dataItems
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error querying void invoice data");
                return StatusCode(500, new BaseResponseDto
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new[] { ex.Message }
                });
            }
        }

        /// <summary>
        /// Query allowance data
        /// </summary>
        /// <param name="viewModel">Invoice query view model</param>
        /// <returns>Allowance data result</returns>
        [HttpPost("allowance")]
        [ProducesResponseType(typeof(ResponseDto<object>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 401)]
        public IActionResult QueryAllowance([FromBody] InvoiceDataQueryViewModel viewModel)
        {
            try
            {
                Organization? agent = ValidateAndGetOrganization(viewModel);

                if (agent == null)
                {
                    if (!ModelState.IsValid)
                    {
                        var errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage);
                        return BadRequest(new BaseResponseDto
                        {
                            Success = false,
                            Message = "Request validation failed",
                            Errors = errors
                        });
                    }
                    return Unauthorized(new BaseResponseDto
                    {
                        Success = false,
                        Message = "Invalid organization or authentication"
                    });
                }

                IQueryable<InvoiceAllowance> items = models!.GetAllowanceByAgent(agent.CompanyID);
                bool effective = false;
                items = items.InquireAllowance(viewModel, models!, ref effective);

                if (viewModel.QueryType == DataQueryType.CountAllowance)
                {
                    return Ok(new ResponseDto<object>
                    {
                        Success = true,
                        Message = "Query completed successfully",
                        Data = new { TotalCount = items.Count() }
                    });
                }

                if (viewModel.PageIndex > 0 && viewModel.PageSize > 0)
                {
                    items = items.Skip((viewModel.PageIndex.Value - 1) * viewModel.PageSize.Value)
                        .Take(viewModel.PageSize.Value);
                }

                var dataItems = items.AsNoTracking().ToList().Select(c => c.CreateAllowanceMIG(models, true)).ToList();

                return Ok(new ResponseDto<object>
                {
                    Success = true,
                    Message = "Allowance data retrieved successfully",
                    Data = dataItems
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error querying allowance data");
                return StatusCode(500, new BaseResponseDto
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new[] { ex.Message }
                });
            }
        }

        /// <summary>
        /// Query void allowance data
        /// </summary>
        /// <param name="viewModel">Invoice query view model</param>
        /// <returns>Void allowance data result</returns>
        [HttpPost("void-allowance")]
        [ProducesResponseType(typeof(ResponseDto<object>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 401)]
        public IActionResult QueryVoidAllowance([FromBody] InvoiceDataQueryViewModel viewModel)
        {
            try
            {
                Organization? agent = ValidateAndGetOrganization(viewModel);

                if (agent == null)
                {
                    if (!ModelState.IsValid)
                    {
                        var errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage);
                        return BadRequest(new BaseResponseDto
                        {
                            Success = false,
                            Message = "Request validation failed",
                            Errors = errors
                        });
                    }
                    return Unauthorized(new BaseResponseDto
                    {
                        Success = false,
                        Message = "Invalid organization or authentication"
                    });
                }

                IQueryable<InvoiceAllowance> items = models!.GetAllowanceByAgent(agent.CompanyID);

                bool effective = false;
                items = items.InquireVoidAllowance(viewModel, models!, ref effective);

                if (viewModel.QueryType == DataQueryType.CountVoidAllowance)
                {
                    return Ok(new ResponseDto<object>
                    {
                        Success = true,
                        Message = "Query completed successfully",
                        Data = new { TotalCount = items.Count() }
                    });
                }

                if (viewModel.PageIndex > 0 && viewModel.PageSize > 0)
                {
                    items = items.Skip((viewModel.PageIndex.Value - 1) * viewModel.PageSize.Value)
                        .Take(viewModel.PageSize.Value);
                }

                var dataItems = items.AsNoTracking().ToList().Select(c => c.CreateCancelAllowanceMIG(true)).ToList();

                return Ok(new ResponseDto<object>
                {
                    Success = true,
                    Message = "Void allowance data retrieved successfully",
                    Data = dataItems
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error querying void allowance data");
                return StatusCode(500, new BaseResponseDto
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new[] { ex.Message }
                });
            }
        }

        /// <summary>
        /// Query invoice number allocation
        /// </summary>
        /// <param name="viewModel">Invoice query view model</param>
        /// <returns>Invoice number allocation result</returns>
        [HttpPost("invoice-no-allocation")]
        [ProducesResponseType(typeof(ResponseDto<object>), 200)]
        [ProducesResponseType(typeof(BaseResponseDto), 400)]
        [ProducesResponseType(typeof(BaseResponseDto), 401)]
        public IActionResult QueryInvoiceNoAllocation([FromBody] InvoiceDataQueryViewModel viewModel)
        {
            try
            {
                Organization? agent = ValidateAndGetOrganization(viewModel);

                if (agent == null)
                {
                    if (!ModelState.IsValid)
                    {
                        var errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage);
                        return BadRequest(new BaseResponseDto
                        {
                            Success = false,
                            Message = "Request validation failed",
                            Errors = errors
                        });
                    }
                    return Unauthorized(new BaseResponseDto
                    {
                        Success = false,
                        Message = "Invalid organization or authentication"
                    });
                }

                IQueryable<InvoiceTrackCodeAssignment> assignments = models!.GetTable<InvoiceTrackCodeAssignment>();
                IQueryable<InvoiceNoInterval> items = models.GetTable<InvoiceNoInterval>();

                viewModel.IssuerNo = viewModel.IssuerNo.GetEfficientString();
                if (viewModel.IssuerNo != null)
                {
                    var orgItems = models.GetTable<Organization>().Where(c => c.ReceiptNo == viewModel.IssuerNo);
                    var issuers = models.GetTable<InvoiceIssuerAgent>().Where(x => x.AgentID == agent.CompanyID)
                                            .Where(x => orgItems.Any(o => o.CompanyID == x.IssuerID));
                    assignments = assignments.Where(a => issuers.Any(x => x.IssuerID == a.SellerID));
                }
                else
                {
                    var issuers = models.GetTable<InvoiceIssuerAgent>().Where(x => x.AgentID == agent.CompanyID);
                    assignments = assignments.Where(a => a.SellerID == agent.CompanyID
                                    || issuers.Any(x => x.IssuerID == a.SellerID));
                }

                IQueryable<InvoiceTrackCode> trackItems = models.GetTable<InvoiceTrackCode>();
                if (viewModel.Year.HasValue)
                {
                    trackItems = trackItems.Where(x => x.Year == viewModel.Year);
                }

                if (viewModel.PeriodNo.HasValue)
                {
                    trackItems = trackItems.Where(x => x.PeriodNo == viewModel.PeriodNo);
                }

                assignments = assignments.Where(a => trackItems.Any(x => x.TrackID == a.TrackID));
                items = items.Where(i => assignments.Any(a => a.SellerID == i.SellerID && a.TrackID == i.TrackID));

                var dataItems = items.AsNoTracking().ToList();

                return Ok(new ResponseDto<object>
                {
                    Success = true,
                    Message = "Invoice number allocation retrieved successfully",
                    Data = dataItems
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error querying invoice number allocation");
                return StatusCode(500, new BaseResponseDto
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new[] { ex.Message }
                });
            }
        }
    }
}

