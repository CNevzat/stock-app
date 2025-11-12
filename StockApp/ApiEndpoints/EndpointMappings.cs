namespace StockApp.ApiEndpoints;

public static class EndpointMappings
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        // ==================== CATEGORIES ====================
        app.MapCategories();

        // ==================== LOCATIONS ====================
        app.MapLocations();

        // ==================== PRODUCTS ====================
        app.MapProducts();

        // ==================== PRODUCT ATTRIBUTES ====================
        app.MapProductAttributes();

        // ==================== STOCK MOVEMENTS ====================
        app.MapStockMovements();

        // ==================== TODOS ====================
        app.MapTodos();

        // ==================== DASHBOARD ====================
        app.MapDashboard();

        // ==================== REPORTS ====================
        app.MapReports();

        // ==================== CHAT ====================
        app.MapChat();
    }
}
