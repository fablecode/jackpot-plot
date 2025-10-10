using JackpotPlot.Application.Abstractions.Messaging;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JackpotPlot.Lottery.API.Infrastructure.Messaging.Handlers;

public sealed class EurojackpotResultMessageHandler : IRequestHandler<JackpotPlot.Infrastructure.Messaging.MessageHandler<Message<EurojackpotResult>>, Result<Message<EurojackpotResult>>>
{
    private const string LotteryName = "Eurojackpot";
    private int? _lotteryId;
    private readonly ILogger<EurojackpotResultMessageHandler> _logger;
    private readonly ILotteryRepository _lotteryRepository;
    private readonly IDrawRepository _drawRepository;
    private readonly IDrawResultRepository _drawResultRepository;
    private readonly IQueueWriter<Message<LotteryDrawnEvent>> _queueWriter;

    public EurojackpotResultMessageHandler
        (
            ILogger<EurojackpotResultMessageHandler> logger, 
            ILotteryRepository lotteryRepository, 
            IDrawRepository drawRepository, 
            IDrawResultRepository drawResultRepository,
            IQueueWriter<Message<LotteryDrawnEvent>> queueWriter
        )
    {
        _logger = logger;
        _lotteryRepository = lotteryRepository; _drawRepository = drawRepository;
        _drawRepository = drawRepository;
        _drawResultRepository = drawResultRepository;
        _queueWriter = queueWriter;
    }

    public async Task<Result<Message<EurojackpotResult>>> Handle(JackpotPlot.Infrastructure.Messaging.MessageHandler<Message<EurojackpotResult>> request, CancellationToken cancellationToken)
    {
        _lotteryId ??= await _lotteryRepository.GetLotteryIdByName(LotteryName);

        if (!await _drawRepository.DrawExist(_lotteryId.Value, request.Message.Data.Date, request.Message.Data.MainNumbers, request.Message.Data.EuroNumbers))
        {
            var drawId = await _drawRepository.Add(_lotteryId.Value, request.Message.Data);
            await _drawResultRepository.Add(drawId, request.Message.Data);

            var routingKey = string.Join('.', RoutingKeys.LotteryDbUpdate, EventTypes.LotteryDrawn);

            var lotteryDrawEvent = new LotteryDrawnEvent
            {
                LotteryId = _lotteryId.Value,
                DrawDate = request.Message.Data.Date,
                WinningNumbers = request.Message.Data.MainNumbers,
                BonusNumbers = request.Message.Data.EuroNumbers
            };

            await _queueWriter.Publish(new Message<LotteryDrawnEvent>(EventTypes.LotteryDrawn, lotteryDrawEvent), routingKey, cancellationToken);

            return Result<Message<EurojackpotResult>>.Success(request.Message);
        }

        return Result<Message<EurojackpotResult>>.Failure($"For event {request.Message.Event} and lottery {LotteryName} with the id {_lotteryId}, draw already exists.");
    }
}