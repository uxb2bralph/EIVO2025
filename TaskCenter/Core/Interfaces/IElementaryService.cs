using AutoMapper;
using Microsoft.Extensions.Localization;

namespace TaskCenter.Core.Interfaces
{
    /// <summary>
    /// AccountingDiary service interface
    /// </summary>
    public interface IElementaryService
    {
        /// <summary>
        /// Gets the unit of work for data access and transaction management.
        /// </summary>
        IUnitOfWork UnitOfWork { get; }

        /// <summary>
        /// Gets the AutoMapper instance for object mapping operations.
        /// </summary>
        IMapper Mapper { get; }

        /// <summary>
        /// Gets the logger instance for logging operations.
        /// </summary>
        ILogger<IElementaryService> Logger { get; }

        ILoggerFactory LoggerFactory { get; }
    }

}
