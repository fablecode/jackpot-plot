using JackpotPlot.Domain.Models;
using MediatR;

namespace JackpotPlot.Lottery.API.Application.Messaging.Handlers;

public record MessageHandler<T> (T Message) : IRequest<Result<T>>;