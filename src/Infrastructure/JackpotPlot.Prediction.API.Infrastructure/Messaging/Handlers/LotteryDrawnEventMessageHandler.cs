using FluentValidation;
using JackpotPlot.Application.Abstractions.Messaging;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Models;
using MediatR;

namespace JackpotPlot.Prediction.API.Infrastructure.Messaging.Handlers;

public sealed class LotteryDrawnEventMessageHandler : IRequestHandler<JackpotPlot.Infrastructure.Messaging.MessageHandler<Message<LotteryDrawnEvent>>, Result<Message<LotteryDrawnEvent>>>
{
    private readonly IValidator<Message<LotteryDrawnEvent>> _validator;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public LotteryDrawnEventMessageHandler(IValidator<Message<LotteryDrawnEvent>> validator, ILotteryHistoryRepository lotteryHistoryRepository)
    {
        _validator = validator;
        _lotteryHistoryRepository = lotteryHistoryRepository;
    }
    public async Task<Result<Message<LotteryDrawnEvent>>> Handle(JackpotPlot.Infrastructure.Messaging.MessageHandler<Message<LotteryDrawnEvent>> request, CancellationToken cancellationToken)
    {
        var validationResults = await _validator.ValidateAsync(request.Message, cancellationToken);

        if (validationResults.IsValid)
        {
            await _lotteryHistoryRepository.Add(request.Message.Data);

            return Result<Message<LotteryDrawnEvent>>.Success(request.Message);
        }

        return Result<Message<LotteryDrawnEvent>>.Failure(validationResults.Errors.Select(er => er.ErrorMessage).ToArray());
    }
}