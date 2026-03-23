namespace StockApp.App.Chat;

public enum ChatIntent
{
    Unknown = 0,

    // Finans & stok soruları
    TopStockInProducts,
    TopStockOutProducts,
    MostProfitableCategory,
    InventoryValue,
    StockMovementSummary,
    StockMovementByType,
    AveragePrices,
    SalesPotential,
    ProductCurrentStatus,
    CategoryInventorySummary,
    TopStockQuantityProduct,

    // Rehber & kullanım
    HowToAddProduct,
    HowToUseDashboard,
    GeneralAppHelp,
    AiAssistantInfo,
    HowToUpdateProduct,
    HowToDeleteProduct,
    HowToManageCategory,
    HowToManageLocation,
    HowToAddAttribute,
    ExplainAttributePurpose,
    HowToManageAttribute,
    HowToViewStockMovements,
    HowToUseTodos,
    HowToExportProductsExcel,

    // Küçük konuşma
    SmallTalk
}


