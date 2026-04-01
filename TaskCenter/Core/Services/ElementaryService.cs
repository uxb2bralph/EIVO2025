using AutoMapper;
using Microsoft.Extensions.Localization;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Services
{
    /// <summary>
    /// AccountingDiary service implementation
    /// </summary>
    public class ElementaryService : IElementaryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<IElementaryService> _logger;
        private readonly ILoggerFactory _loggerFactory;

        /// <inheritdoc />
        public ElementaryService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILoggerFactory loggerFactory,
            ILogger<IElementaryService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        /// <inheritdoc />
        public IUnitOfWork UnitOfWork => _unitOfWork;

        /// <inheritdoc />
        public IMapper Mapper => _mapper;

        /// <inheritdoc/>
        public ILogger<IElementaryService> Logger => _logger;

        /// <inheritdoc/>
        public ILoggerFactory LoggerFactory => _loggerFactory;
    }

}
