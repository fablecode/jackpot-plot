using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace JackpotPlot.Domain.Messaging.Processors;

public sealed class EurojackpotResultMessageProcessor : IMessageProcessor<Message<EurojackpotResult>>
{
    private const string LotteryName = "Eurojackpot";
    private int? _lotteryId;
    private readonly ILogger<EurojackpotResultMessageProcessor> _logger;
    private readonly ILotteryRepository _lotteryRepository;
    private readonly IDrawRepository _drawRepository;

    public EurojackpotResultMessageProcessor(ILogger<EurojackpotResultMessageProcessor> logger, ILotteryRepository lotteryRepository, IDrawRepository drawRepository)
    {
        _logger = logger;
        _lotteryRepository = lotteryRepository;
        _drawRepository = drawRepository;
    }
    public async Task<Result<Message<EurojackpotResult>>> ProcessAsync(Message<EurojackpotResult> message, CancellationToken cancellationToken)
    {
        _lotteryId ??= await _lotteryRepository.GetLotteryIdByName(LotteryName);

        if (!await _drawRepository.DrawExist(_lotteryId.Value, message.Data.Date, message.Data.MainNumbers, message.Data.EuroNumbers))
        {
            await _drawRepository.Add(_lotteryId.Value, message.Data);
            return Result<Message<EurojackpotResult>>.Success(message);
        }

        return Result<Message<EurojackpotResult>>.Failure($"For Eurojackpot event {message.Event}, with lottery id {_lotteryId}, draw already exists.");
    }
}