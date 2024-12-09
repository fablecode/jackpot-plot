using FluentAssertions;
using LotteryDataCollector.Service.Infrastructure.Services;
using LotteryDataCollector.Service.Infrastructure.WebPages;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System.Globalization;

namespace JackpotPlot.Infrastructure.Integration.Tests.ServicesTests.EurojackpotServiceTests;

[TestFixture]
public class GetDrawDetailsTests
{
    private EurojackpotService _sut;

    [SetUp]
    public void SetUp()
    {
        _sut = new EurojackpotService(Substitute.For<ILogger<EurojackpotService>>(), new HtmlWebPage());
    }

    [TestCaseSource(nameof(_extractDrawDateCases))]
    public void Given_A_Eurojackpot_Draw_Url_Should_Extract_Draw_DateTime(string drawUrl, DateTime expected)
    {
        // Arrange
        // Act
        var result = _sut.GetDrawDetails(drawUrl);

        // Assert
        result.Date.Should().Be(expected);
    }

    [TestCase("https://www.euro-jackpot.net/results/19-10-2012", 8)]
    [TestCase("https://www.euro-jackpot.net/results/04-09-2015", 0)]
    public void Given_A_Eurojackpot_Draw_Url_Should_Extract_Draw_Rollover(string drawUrl, int expected)
    {
        // Arrange
        // Act
        var result = _sut.GetDrawDetails(drawUrl);

        // Assert
        result.Rollover.Should().Be(expected);
    }

    [TestCaseSource(nameof(_extractWinningNumbersCases))]
    public void Given_A_Eurojackpot_Draw_Url_Should_Extract_Draw_Main_Numbers(string drawUrl, List<int> expected)
    {
        // Arrange
        // Act
        var result = _sut.GetDrawDetails(drawUrl);

        // Assert
        result.MainNumbers.Should().BeEquivalentTo(expected);
    }

    [TestCaseSource(nameof(_extractEuroNumbersCases))]
    public void Given_A_Eurojackpot_Draw_Url_Should_Extract_Draw_Euro_Numbers(string drawUrl, List<int> expected)
    {
        // Arrange
        // Act
        var result = _sut.GetDrawDetails(drawUrl);

        // Assert
        result.EuroNumbers.Should().BeEquivalentTo(expected);
    }

    [TestCase("https://www.euro-jackpot.net/results/04-09-2015", 1)]
    public void Given_A_Eurojackpot_Draw_Url_Should_Extract_Draw_Jackpot_Winners(string drawUrl, int expected)
    {
        // Arrange
        // Act
        var result = _sut.GetDrawDetails(drawUrl);

        // Assert
        result.JackpotWinners.Should().Be(expected);
    }

    [TestCase("https://www.euro-jackpot.net/results/04-09-2015", 712310)]
    public void Given_A_Eurojackpot_Draw_Url_Should_Extract_Draw_Total_Winners(string drawUrl, int expected)
    {
        // Arrange
        // Act
        var result = _sut.GetDrawDetails(drawUrl);

        // Assert
        result.TotalWinners.Should().Be(expected);
    }

    [TestCase("https://www.euro-jackpot.net/results/04-09-2015", 16)]
    public void Given_A_Eurojackpot_Draw_Url_Should_Extract_Draw_Prize_Breakdown(string drawUrl, int expected)
    {
        // Arrange
        // Act
        var result = _sut.GetDrawDetails(drawUrl);

        // Assert
        result.PrizeBreakdown.Should().HaveCount(expected);
    }

    #region Test Data

    private static object[] _extractDrawDateCases =
    [
        new object[] { "https://www.euro-jackpot.net/results/23-03-2012", DateTime.ParseExact("23-03-2012", "dd-MM-yyyy", CultureInfo.InvariantCulture)},
        new object[] { "https://www.euro-jackpot.net/results/04-09-2015", DateTime.ParseExact("04-09-2015", "dd-MM-yyyy", CultureInfo.InvariantCulture)}
    ];

    private static object[] _extractWinningNumbersCases =
    [
        new object[] { "https://www.euro-jackpot.net/results/23-03-2012", new List<int> { 5, 8, 21, 37, 46 }}
    ]; 

    private static object[] _extractEuroNumbersCases =
    [
        new object[] { "https://www.euro-jackpot.net/results/23-03-2012", new List<int> { 6, 8 }}
    ]; 

    #endregion
}