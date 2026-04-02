using JackpotPlot.Desktop.UI.Models.Menu;
using JackpotPlot.Desktop.UI.Services.Navigation;

namespace JackpotPlot.Desktop.UI.Configuration;

public static class MenuConfiguration
{
    public static List<MenuItem> GetMenuItems()
    {
        return new List<MenuItem>
        {
            CreateDashboardMenu(),
            CreateLotteryMenu(),
            CreatePredictionsMenu(),
            CreateDrawInsightsMenu(),
            CreateWinningStrategiesMenu(),
            CreateComparisonToolMenu(),
            CreateUserToolsMenu(),
            CreateCommunityMenu()
        };
    }

    private static MenuItem CreateDashboardMenu()
    {
        var menu = new MenuItem(
            id: NavigationKeys.Dashboard,
            title: "Dashboard",
            icon: "ViewDashboard",
            order: 0);

        menu.AddChild(new MenuItem(
            id: "dashboard-overview",
            title: "Overview",
            icon: "Home",
            navigationKey: NavigationKeys.Dashboard,
            order: 0));

        menu.AddChild(new MenuItem(
            id: "dashboard-trends",
            title: "Trends",
            icon: "ChartLine",
            navigationKey: "trends",
            order: 1));

        menu.AddChild(new MenuItem(
            id: "dashboard-hot-cold",
            title: "Hot & Cold Numbers",
            icon: "Fire",
            navigationKey: "hot-cold-numbers",
            order: 2));

        menu.AddChild(new MenuItem(
            id: "dashboard-patterns",
            title: "Winning Patterns",
            icon: "Grid",
            navigationKey: "winning-patterns",
            order: 3));

        return menu;
    }

    private static MenuItem CreateLotteryMenu()
    {
        var menu = new MenuItem(
            id: "lottery",
            title: "Lottery",
            icon: "Receipt",
            order: 1);

        menu.AddChild(new MenuItem(
            id: "lottery-historical",
            title: "Historical Results",
            icon: "History",
            navigationKey: "historical-results",
            order: 0));

        menu.AddChild(new MenuItem(
            id: "lottery-frequency",
            title: "Number Frequency",
            icon: "ChartBar",
            navigationKey: "number-frequency",
            order: 1));

        menu.AddChild(new MenuItem(
            id: "lottery-draw-history",
            title: "Draw History",
            icon: "ClockOutline",
            navigationKey: NavigationKeys.DrawHistory,
            order: 2));

        return menu;
    }

    private static MenuItem CreatePredictionsMenu()
    {
        var menu = new MenuItem(
            id: "predictions",
            title: "Predictions",
            icon: "ChartLineVariant",
            order: 2);

        menu.AddChild(new MenuItem(
            id: "predictions-generator",
            title: "Number Generator",
            icon: "AutoFix",
            navigationKey: NavigationKeys.NumberGenerator,
            order: 0));

        menu.AddChild(new MenuItem(
            id: "predictions-probability",
            title: "Statistical Probability",
            icon: "ChartBellCurve",
            navigationKey: "statistical-probability",
            order: 1));

        menu.AddChild(new MenuItem(
            id: "predictions-custom",
            title: "Custom Number Selection",
            icon: "CursorPointer",
            navigationKey: "custom-selection",
            order: 2));

        return menu;
    }

    private static MenuItem CreateDrawInsightsMenu()
    {
        var menu = new MenuItem(
            id: "draw-insights",
            title: "Draw Insights",
            icon: "ChartLineUp",
            order: 3);

        menu.AddChild(new MenuItem(
            id: "insights-analysis",
            title: "Past Draw Analysis",
            icon: "FileChart",
            navigationKey: "past-draw-analysis",
            order: 0));

        menu.AddChild(new MenuItem(
            id: "insights-pairs",
            title: "Common Number Pairs",
            icon: "LinkVariant",
            navigationKey: "common-pairs",
            order: 1));

        menu.AddChild(new MenuItem(
            id: "insights-trends",
            title: "Trends",
            icon: "TrendingUp",
            navigationKey: "insights-trends",
            order: 2));

        return menu;
    }

    private static MenuItem CreateWinningStrategiesMenu()
    {
        var menu = new MenuItem(
            id: "winning-strategies",
            title: "Winning Strategies",
            icon: "RouteVariant",
            order: 4);

        menu.AddChild(new MenuItem(
            id: "strategies-mathematical",
            title: "Mathematical Strategies",
            icon: "Calculator",
            navigationKey: "mathematical-strategies",
            order: 0));

        menu.AddChild(new MenuItem(
            id: "strategies-guides",
            title: "User Guides",
            icon: "BookOpenVariant",
            navigationKey: "user-guides",
            order: 1));

        menu.AddChild(new MenuItem(
            id: "strategies-wins",
            title: "Past Wins",
            icon: "Trophy",
            navigationKey: "past-wins",
            order: 2));

        return menu;
    }

    private static MenuItem CreateComparisonToolMenu()
    {
        var menu = new MenuItem(
            id: "comparison-tool",
            title: "Comparison Tool",
            icon: "CompareHorizontal",
            order: 5);

        menu.AddChild(new MenuItem(
            id: "comparison-lotteries",
            title: "Compare Lotteries",
            icon: "ScaleBalance",
            navigationKey: "compare-lotteries",
            order: 0));

        menu.AddChild(new MenuItem(
            id: "comparison-prizes",
            title: "Prize Structures",
            icon: "CurrencyUsd",
            navigationKey: "prize-structures",
            order: 1));

        menu.AddChild(new MenuItem(
            id: "comparison-odds",
            title: "Best Odds",
            icon: "PercentOutline",
            navigationKey: "best-odds",
            order: 2));

        return menu;
    }

    private static MenuItem CreateUserToolsMenu()
    {
        var menu = new MenuItem(
            id: "user-tools",
            title: "User Tools",
            icon: "Cog",
            order: 6);

        menu.AddChild(new MenuItem(
            id: "tools-tickets",
            title: "Tickets",
            icon: "TicketOutline",
            navigationKey: "tickets",
            order: 0));

        menu.AddChild(new MenuItem(
            id: "tools-kanban",
            title: "Kanban Board",
            icon: "ViewKanban",
            navigationKey: "kanban-board",
            order: 1));

        menu.AddChild(new MenuItem(
            id: "tools-picker",
            title: "Custom Number Picker",
            icon: "Picker",
            navigationKey: "custom-picker",
            order: 2));

        menu.AddChild(new MenuItem(
            id: "tools-favorites",
            title: "Favorite Numbers",
            icon: "Star",
            navigationKey: "favorite-numbers",
            order: 3));

        return menu;
    }

    private static MenuItem CreateCommunityMenu()
    {
        var menu = new MenuItem(
            id: "community",
            title: "Community",
            icon: "AccountGroup",
            order: 7);

        menu.AddChild(new MenuItem(
            id: "community-forum",
            title: "Discussion Forum",
            icon: "Forum",
            navigationKey: "discussion-forum",
            order: 0));

        menu.AddChild(new MenuItem(
            id: "community-predictions",
            title: "User Predictions",
            icon: "AccountMultiple",
            navigationKey: "user-predictions",
            order: 1));

        menu.AddChild(new MenuItem(
            id: "community-stories",
            title: "Winning Stories",
            icon: "MessageText",
            navigationKey: "winning-stories",
            order: 2));

        return menu;
    }
}
