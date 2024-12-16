using FluentValidation;
using JackpotPlot.Domain.Messaging;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using MediatR;

namespace JackpotPlot.Prediction.API.Application.Messaging.Handlers;

public sealed class LotteryDrawnEventMessageHandler : IRequestHandler<MessageHandler<Message<LotteryDrawnEvent>>, Result<Message<LotteryDrawnEvent>>>
{
    private readonly IValidator<Message<LotteryDrawnEvent>> _validator;
    private readonly ILotteryHistoryRepository _lotteryHistoryRepository;

    public LotteryDrawnEventMessageHandler(IValidator<Message<LotteryDrawnEvent>> validator, ILotteryHistoryRepository lotteryHistoryRepository)
    {
        _validator = validator;
        _lotteryHistoryRepository = lotteryHistoryRepository;
    }
    public async Task<Result<Message<LotteryDrawnEvent>>> Handle(MessageHandler<Message<LotteryDrawnEvent>> request, CancellationToken cancellationToken)
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