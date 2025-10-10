using JackpotPlot.Domain.Models;
using MediatR;

namespace JackpotPlot.Infrastructure.Messaging;

public record MessageHandler<T> (T Message) : IRequest<Result<T>>;