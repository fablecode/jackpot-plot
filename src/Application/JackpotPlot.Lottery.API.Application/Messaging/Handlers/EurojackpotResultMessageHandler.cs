using JackpotPlot.Domain.Messaging;
using JackpotPlot.Domain.Messaging.Processors;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JackpotPlot.Lottery.API.Application.Messaging.Handlers;

public sealed class EurojackpotResultMessageHandler : IRequestHandler<MessageHandler<Message<EurojackpotResult>>, Result<Message<EurojackpotResult>>>
{
    private const string LotteryName = "Eurojackpot";
    private int? _lotteryId;
    private readonly ILogger<EurojackpotResultMessageHandler> _logger;
    private readonly ILotteryRepository _lotteryRepository;
    private readonly IDrawRepository _drawRepository;

    public EurojackpotResultMessageHandler(ILogger<EurojackpotResultMessageHandler> logger, ILotteryRepository lotteryRepository, IDrawRepository drawRepository, IDrawResultRepository drawResultRepository)
    {
        _logger = logger;
        _lotteryRepository = lotteryRepository;
        _drawRepository = drawRepository;
    }

    public async Task<Result<Message<EurojackpotResult>>> Handle(MessageHandler<Message<EurojackpotResult>> request, CancellationToken cancellationToken)
    {
        _lotteryId ??= await _lotteryRepository.GetLotteryIdByName(LotteryName);

        if (!_drawRepository.DrawExist(_lotteryId.Value, request.Message.Data.Date, request.Message.Data.MainNumbers, request.Message.Data.EuroNumbers))
        {
            await _drawRepository.Add(_lotteryId.Value, request.Message.Data);

            return Result<Message<EurojackpotResult>>.Success(request.Message);
        }

        return Result<Message<EurojackpotResult>>.Failure($"For event {request.Message.Event} and lottery {LotteryName} with the id {_lotteryId}, draw already exists.");
    }
}