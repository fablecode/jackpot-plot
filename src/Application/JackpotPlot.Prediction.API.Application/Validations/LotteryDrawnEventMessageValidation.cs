using FluentValidation;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Messaging;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Prediction.API.Application.Validations;

public sealed class LotteryDrawnEventMessageValidation : AbstractValidator<Message<LotteryDrawnEvent>>
{
    public LotteryDrawnEventMessageValidation()
    {
        RuleFor(x => x.Event)
            .Equals(EventTypes.LotteryDrawn);
        RuleFor(x => x.Data.LotteryId)
            .GreaterThan(0);
    }
}