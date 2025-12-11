using FluentValidation;
using JackpotPlot.Application.Abstractions.Messaging;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Prediction.API.Application.Validations;

public sealed class LotteryDrawnEventMessageValidation : AbstractValidator<Message<LotteryDrawnEvent>>
{
    public LotteryDrawnEventMessageValidation()
    {
        RuleFor(x => x.Event)
            .Equal(EventTypes.LotteryDrawn);

        RuleFor(x => x.Data.LotteryId)
            .GreaterThan(0);
    }
}