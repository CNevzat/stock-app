using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockApp.Entities;

namespace StockApp.Services;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DatabaseSeeder(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        // Veritabanının oluşturulduğundan emin ol
        await _context.Database.EnsureCreatedAsync();

        // Rolleri oluştur
        await SeedRolesAsync();

        // Admin kullanıcısı oluştur
        await SeedAdminUserAsync();

        // Eğer zaten veri varsa, seed yapma
        if (await _context.Categories.AnyAsync())
        {
            Console.WriteLine("Veritabanında zaten veri var. Seed işlemi atlandı.");
            return;
        }

        Console.WriteLine("Veritabanı seed işlemi başlatılıyor...");

        // Kategoriler
        var categories = new List<Category>
        {
            new Category { Name = "Elektronik", CreatedAt = DateTime.UtcNow },
            new Category { Name = "Bilgisayar", CreatedAt = DateTime.UtcNow },
            new Category { Name = "Telefon", CreatedAt = DateTime.UtcNow },
            new Category { Name = "Ofis Malzemeleri", CreatedAt = DateTime.UtcNow },
            new Category { Name = "Yazılım", CreatedAt = DateTime.UtcNow }
        };

        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();
        Console.WriteLine($"{categories.Count} kategori eklendi.");

        // Lokasyonlar
        var locations = new List<Location>
        {
            new Location { Name = "Ana Depo", Description = "Merkez depo, A blok", CreatedAt = DateTime.UtcNow },
            new Location { Name = "Şube 1", Description = "Kadıköy şubesi", CreatedAt = DateTime.UtcNow },
            new Location { Name = "Şube 2", Description = "Beşiktaş şubesi", CreatedAt = DateTime.UtcNow },
            new Location { Name = "Showroom", Description = "Mağaza vitrin", CreatedAt = DateTime.UtcNow }
        };

        await _context.Locations.AddRangeAsync(locations);
        await _context.SaveChangesAsync();
        Console.WriteLine($"{locations.Count} lokasyon eklendi.");

        // Ürünler
        var products = new List<Product>
        {
            new Product
            {
                Name = "MacBook Pro 16\"",
                StockCode = "MBP16-001",
                Description = "Apple MacBook Pro 16 inç, M3 Pro çip, 18GB RAM, 512GB SSD",
                StockQuantity = 5,
                LowStockThreshold = 3,
                CategoryId = categories[1].Id, // Bilgisayar
                LocationId = locations[0].Id, // Ana Depo
                CurrentPurchasePrice = 45000,
                CurrentSalePrice = 55000,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "iPhone 15 Pro",
                StockCode = "IP15P-001",
                Description = "Apple iPhone 15 Pro, 256GB, Titanium",
                StockQuantity = 12,
                LowStockThreshold = 5,
                CategoryId = categories[2].Id, // Telefon
                LocationId = locations[3].Id, // Showroom
                CurrentPurchasePrice = 42000,
                CurrentSalePrice = 48000,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Samsung Galaxy S24 Ultra",
                StockCode = "SGS24U-001",
                Description = "Samsung Galaxy S24 Ultra, 256GB, S Pen dahil",
                StockQuantity = 8,
                LowStockThreshold = 4,
                CategoryId = categories[2].Id, // Telefon
                LocationId = locations[1].Id, // Şube 1
                CurrentPurchasePrice = 38000,
                CurrentSalePrice = 45000,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Dell XPS 15",
                StockCode = "DXP15-001",
                Description = "Dell XPS 15, Intel i7, 16GB RAM, 1TB SSD, OLED Ekran",
                StockQuantity = 3,
                LowStockThreshold = 2,
                CategoryId = categories[1].Id, // Bilgisayar
                LocationId = locations[0].Id, // Ana Depo
                CurrentPurchasePrice = 35000,
                CurrentSalePrice = 42000,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Logitech MX Master 3S",
                StockCode = "LMX3S-001",
                Description = "Kablosuz ergonomik mouse, 8000 DPI",
                StockQuantity = 25,
                LowStockThreshold = 10,
                CategoryId = categories[0].Id, // Elektronik
                LocationId = locations[0].Id, // Ana Depo
                CurrentPurchasePrice = 1200,
                CurrentSalePrice = 1800,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Keychron K8 Pro",
                StockCode = "KCK8P-001",
                Description = "Mekanik klavye, RGB, Bluetooth, Gateron Brown switch",
                StockQuantity = 15,
                LowStockThreshold = 5,
                CategoryId = categories[0].Id, // Elektronik
                LocationId = locations[0].Id, // Ana Depo
                CurrentPurchasePrice = 2500,
                CurrentSalePrice = 3500,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "HP LaserJet Pro",
                StockCode = "HPLJP-001",
                Description = "Lazer yazıcı, A4, WiFi, USB",
                StockQuantity = 7,
                LowStockThreshold = 3,
                CategoryId = categories[3].Id, // Ofis Malzemeleri
                LocationId = locations[2].Id, // Şube 2
                CurrentPurchasePrice = 4500,
                CurrentSalePrice = 6500,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Visual Studio Code Lisansı",
                StockCode = "VSCODE-001",
                Description = "VS Code Enterprise Edition, 1 yıllık lisans",
                StockQuantity = 50,
                LowStockThreshold = 20,
                CategoryId = categories[4].Id, // Yazılım
                LocationId = null, // Dijital ürün
                CurrentPurchasePrice = 0,
                CurrentSalePrice = 500,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "iPad Air",
                StockCode = "IPADA-001",
                Description = "Apple iPad Air, 11 inç, M2 çip, 128GB",
                StockQuantity = 2,
                LowStockThreshold = 2,
                CategoryId = categories[1].Id, // Bilgisayar
                LocationId = locations[3].Id, // Showroom
                CurrentPurchasePrice = 18000,
                CurrentSalePrice = 22000,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Sony WH-1000XM5",
                StockCode = "SNYXM5-001",
                Description = "Kablosuz kulaklık, gürültü önleme, 30 saat pil",
                StockQuantity = 18,
                LowStockThreshold = 8,
                CategoryId = categories[0].Id, // Elektronik
                LocationId = locations[1].Id, // Şube 1
                CurrentPurchasePrice = 5500,
                CurrentSalePrice = 7500,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();
        Console.WriteLine($"{products.Count} ürün eklendi.");

        // Ürün Öznitelikleri
        var attributes = new List<ProductAttribute>
        {
            // MacBook Pro öznitelikleri
            new ProductAttribute { ProductId = products[0].Id, Key = "İşlemci", Value = "M3 Pro", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[0].Id, Key = "RAM", Value = "18GB", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[0].Id, Key = "Depolama", Value = "512GB SSD", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[0].Id, Key = "Ekran", Value = "16.2 inç Liquid Retina XDR", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[0].Id, Key = "Renk", Value = "Uzay Grisi", CreatedAt = DateTime.UtcNow },

            // iPhone 15 Pro öznitelikleri
            new ProductAttribute { ProductId = products[1].Id, Key = "Depolama", Value = "256GB", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[1].Id, Key = "Renk", Value = "Titanium", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[1].Id, Key = "Ekran", Value = "6.1 inç Super Retina XDR", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[1].Id, Key = "Kamera", Value = "48MP Ana, 12MP Ultra Geniş, 12MP Tele", CreatedAt = DateTime.UtcNow },

            // Samsung Galaxy S24 Ultra öznitelikleri
            new ProductAttribute { ProductId = products[2].Id, Key = "Depolama", Value = "256GB", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[2].Id, Key = "RAM", Value = "12GB", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[2].Id, Key = "Ekran", Value = "6.8 inç Dynamic AMOLED 2X", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[2].Id, Key = "Kamera", Value = "200MP Ana, 50MP Tele, 12MP Ultra Geniş", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[2].Id, Key = "S Pen", Value = "Dahil", CreatedAt = DateTime.UtcNow },

            // Dell XPS 15 öznitelikleri
            new ProductAttribute { ProductId = products[3].Id, Key = "İşlemci", Value = "Intel Core i7-13700H", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[3].Id, Key = "RAM", Value = "16GB DDR5", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[3].Id, Key = "Depolama", Value = "1TB NVMe SSD", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[3].Id, Key = "Ekran", Value = "15.6 inç OLED 3.5K", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[3].Id, Key = "Grafik", Value = "NVIDIA RTX 4050", CreatedAt = DateTime.UtcNow },

            // Logitech Mouse öznitelikleri
            new ProductAttribute { ProductId = products[4].Id, Key = "Bağlantı", Value = "Bluetooth, USB Receiver", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[4].Id, Key = "DPI", Value = "8000", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[4].Id, Key = "Pil", Value = "70 gün", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[4].Id, Key = "Renk", Value = "Gri", CreatedAt = DateTime.UtcNow },

            // Keychron Klavye öznitelikleri
            new ProductAttribute { ProductId = products[5].Id, Key = "Switch", Value = "Gateron Brown", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[5].Id, Key = "Bağlantı", Value = "Bluetooth, USB-C", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[5].Id, Key = "Aydınlatma", Value = "RGB", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[5].Id, Key = "Layout", Value = "TKL (87 tuş)", CreatedAt = DateTime.UtcNow },

            // HP Yazıcı öznitelikleri
            new ProductAttribute { ProductId = products[6].Id, Key = "Tip", Value = "Lazer", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[6].Id, Key = "Kağıt Boyutu", Value = "A4", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[6].Id, Key = "Bağlantı", Value = "WiFi, USB", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[6].Id, Key = "Yazdırma Hızı", Value = "22 sayfa/dakika", CreatedAt = DateTime.UtcNow },

            // iPad Air öznitelikleri
            new ProductAttribute { ProductId = products[8].Id, Key = "İşlemci", Value = "M2", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[8].Id, Key = "Depolama", Value = "128GB", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[8].Id, Key = "Ekran", Value = "11 inç Liquid Retina", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[8].Id, Key = "Renk", Value = "Mavi", CreatedAt = DateTime.UtcNow },

            // Sony Kulaklık öznitelikleri
            new ProductAttribute { ProductId = products[9].Id, Key = "Bağlantı", Value = "Bluetooth 5.2", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[9].Id, Key = "Pil Ömrü", Value = "30 saat", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[9].Id, Key = "Gürültü Önleme", Value = "Aktif (ANC)", CreatedAt = DateTime.UtcNow },
            new ProductAttribute { ProductId = products[9].Id, Key = "Renk", Value = "Siyah", CreatedAt = DateTime.UtcNow }
        };

        await _context.ProductAttributes.AddRangeAsync(attributes);
        await _context.SaveChangesAsync();
        Console.WriteLine($"{attributes.Count} ürün özniteliği eklendi.");

        // Fiyat Geçmişi
        var priceHistory = new List<ProductPrice>
        {
            new ProductPrice
            {
                ProductId = products[0].Id,
                PurchasePrice = 44000,
                SalePrice = 54000,
                EffectiveDate = DateTime.UtcNow.AddMonths(-2),
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            },
            new ProductPrice
            {
                ProductId = products[1].Id,
                PurchasePrice = 40000,
                SalePrice = 46000,
                EffectiveDate = DateTime.UtcNow.AddMonths(-1),
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new ProductPrice
            {
                ProductId = products[2].Id,
                PurchasePrice = 36000,
                SalePrice = 43000,
                EffectiveDate = DateTime.UtcNow.AddMonths(-1),
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            }
        };

        await _context.ProductPrices.AddRangeAsync(priceHistory);
        await _context.SaveChangesAsync();
        Console.WriteLine($"{priceHistory.Count} fiyat geçmişi kaydı eklendi.");

        // Stok Hareketleri
        var stockMovements = new List<StockMovement>
        {
            new StockMovement
            {
                ProductId = products[0].Id,
                CategoryId = products[0].CategoryId,
                Type = StockMovementType.In,
                Quantity = 5,
                UnitPrice = 45000,
                Description = "İlk stok girişi",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new StockMovement
            {
                ProductId = products[1].Id,
                CategoryId = products[1].CategoryId,
                Type = StockMovementType.In,
                Quantity = 15,
                UnitPrice = 42000,
                Description = "Toplu alım",
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new StockMovement
            {
                ProductId = products[1].Id,
                CategoryId = products[1].CategoryId,
                Type = StockMovementType.Out,
                Quantity = 3,
                UnitPrice = 48000,
                Description = "Satış",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new StockMovement
            {
                ProductId = products[2].Id,
                CategoryId = products[2].CategoryId,
                Type = StockMovementType.In,
                Quantity = 10,
                UnitPrice = 38000,
                Description = "Yeni ürün girişi",
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new StockMovement
            {
                ProductId = products[2].Id,
                CategoryId = products[2].CategoryId,
                Type = StockMovementType.Out,
                Quantity = 2,
                UnitPrice = 45000,
                Description = "Satış",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new StockMovement
            {
                ProductId = products[4].Id,
                CategoryId = products[4].CategoryId,
                Type = StockMovementType.In,
                Quantity = 30,
                UnitPrice = 1200,
                Description = "Toplu alım",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new StockMovement
            {
                ProductId = products[4].Id,
                CategoryId = products[4].CategoryId,
                Type = StockMovementType.Out,
                Quantity = 5,
                UnitPrice = 1800,
                Description = "Satış",
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new StockMovement
            {
                ProductId = products[9].Id,
                CategoryId = products[9].CategoryId,
                Type = StockMovementType.In,
                Quantity = 20,
                UnitPrice = 5500,
                Description = "Yeni ürün girişi",
                CreatedAt = DateTime.UtcNow.AddDays(-12)
            },
            new StockMovement
            {
                ProductId = products[9].Id,
                CategoryId = products[9].CategoryId,
                Type = StockMovementType.Out,
                Quantity = 2,
                UnitPrice = 7500,
                Description = "Satış",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        await _context.StockMovements.AddRangeAsync(stockMovements);
        await _context.SaveChangesAsync();
        Console.WriteLine($"{stockMovements.Count} stok hareketi eklendi.");

        // Yapılacaklar
        var todos = new List<TodoItem>
        {
            new TodoItem
            {
                Title = "Yeni ürün kataloğu hazırla",
                Description = "Q1 2024 ürün kataloğunu güncelle ve yayınla",
                Status = TodoStatus.InProgress,
                Priority = TodoPriority.High,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new TodoItem
            {
                Title = "Düşük stoklu ürünleri kontrol et",
                Description = "iPad Air stok seviyesi kritik, sipariş verilmeli",
                Status = TodoStatus.Todo,
                Priority = TodoPriority.High,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new TodoItem
            {
                Title = "Fiyat güncellemelerini yap",
                Description = "MacBook Pro ve iPhone fiyatlarını güncelle",
                Status = TodoStatus.Todo,
                Priority = TodoPriority.Medium,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new TodoItem
            {
                Title = "Aylık rapor hazırla",
                Description = "Kasım ayı satış ve stok raporunu hazırla",
                Status = TodoStatus.Completed,
                Priority = TodoPriority.Medium,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new TodoItem
            {
                Title = "Yeni lokasyon ekle",
                Description = "Şişli şubesi için yeni lokasyon kaydı oluştur",
                Status = TodoStatus.Todo,
                Priority = TodoPriority.Low,
                CreatedAt = DateTime.UtcNow
            },
            new TodoItem
            {
                Title = "Ürün görsellerini güncelle",
                Description = "Tüm ürünlerin görsellerini yüksek çözünürlüklü versiyonlarla değiştir",
                Status = TodoStatus.InProgress,
                Priority = TodoPriority.Medium,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        await _context.TodoItems.AddRangeAsync(todos);
        await _context.SaveChangesAsync();
        Console.WriteLine($"{todos.Count} yapılacak görev eklendi.");

        Console.WriteLine("✅ Veritabanı seed işlemi tamamlandı!");
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[] { "Admin", "Manager", "User" };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
                Console.WriteLine($"Rol oluşturuldu: {role}");
            }
        }

        // Admin rolüne tüm yetkileri ata
        var rolePermissionService = new RolePermissionService(_roleManager);
        await rolePermissionService.EnsureAdminHasAllPermissionsAsync();
        Console.WriteLine("Admin rolüne tüm yetkiler atandı.");
    }

    private async Task SeedAdminUserAsync()
    {
        var adminEmail = "admin@stockapp.com";
        var adminUser = await _userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                IsActive = true,
                MustChangePassword = true, // İlk girişte şifre değiştirmesi gerekiyor
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine("Admin kullanıcısı oluşturuldu: admin@stockapp.com / Admin123!");
            }
            else
            {
                Console.WriteLine($"Admin kullanıcısı oluşturulamadı: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}

