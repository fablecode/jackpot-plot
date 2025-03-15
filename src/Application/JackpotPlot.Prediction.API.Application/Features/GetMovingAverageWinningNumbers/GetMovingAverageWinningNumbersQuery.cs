using JackpotPlot.Domain.Models;
using MediatR;

namespace JackpotPlot.Prediction.API.Application.Features.GetMovingAverageWinningNumbers;

public record GetMovingAverageWinningNumbersQuery(int LotteryId, int WindowSize) : IRequest<WinningNumberMovingAverageResult>;