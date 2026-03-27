using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace StockApp.Services;

// Rol yetki yönetimi için yardımcı servis
// Tüm yetkileri Admin rolüne otomatik atar, manual eklemeye gerek kalınmaması için

public class RolePermissionService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    
    // Sistemdeki tüm yetkiler
    private static readonly List<string> AllPermissions = new()
    {
        // User Management
        "CanCreateUser",
        "CanManageUsers",
        // Role Management
        "CanViewRoles",
        "CanManageRoles",
        // Categories
        "CanViewCategories",
        "CanManageCategories",
        // Products
        "CanViewProducts",
        "CanManageProducts",
        // Product Attributes
        "CanViewProductAttributes",
        "CanManageProductAttributes",
        // Stock Movements
        "CanViewStockMovements",
        "CanManageStockMovements",
        // Todos
        "CanViewTodos",
        "CanManageTodos",
        // Locations
        "CanViewLocations",
        "CanManageLocations",
        // Dashboard
        "CanViewDashboard",
        // Reports
        "CanViewReports",
        // Chat
        "CanUseChat"
    };

    public RolePermissionService(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    // Tüm yetkileri admin rolüne atar
    public async Task EnsureAdminHasAllPermissionsAsync()
    {
        var adminRole = await _roleManager.FindByNameAsync("Admin");
        if (adminRole == null)
        {
            return;
        }

        var existingClaims = await _roleManager.GetClaimsAsync(adminRole);
        var existingPermissionValues = existingClaims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToHashSet();

        foreach (var permission in AllPermissions)
        {
            if (!existingPermissionValues.Contains(permission))
            {
                await _roleManager.AddClaimAsync(adminRole, new Claim("Permission", permission));
            }
        }
    }

    
    // Yeni bir yetki eklendiğinde Admin rolüne otomatik atar
    
    public async Task AddPermissionToAdminAsync(string permissionValue)
    {
        var adminRole = await _roleManager.FindByNameAsync("Admin");
        if (adminRole == null)
        {
            return;
        }

        var existingClaims = await _roleManager.GetClaimsAsync(adminRole);
        var alreadyExists = existingClaims.Any(c => c.Type == "Permission" && c.Value == permissionValue);

        if (!alreadyExists)
        {
            await _roleManager.AddClaimAsync(adminRole, new Claim("Permission", permissionValue));
        }
    }

    // Sistemdeki tüm yetkileri döndürür
    public static List<string> GetAllPermissions() => AllPermissions.ToList();

    
    // Yeni bir yetki ekler
    public static void AddPermission(string permission)
    {
        if (!AllPermissions.Contains(permission))
        {
            AllPermissions.Add(permission);
        }
    }
}

