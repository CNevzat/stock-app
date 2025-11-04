# Stock App Frontend

Modern React frontend application for Stock Management System.

## Tech Stack

- **React 18** - UI Library
- **TypeScript** - Type Safety
- **Vite** - Build Tool & Dev Server
- **TanStack Query (React Query)** - Server State Management
- **React Router** - Client-side Routing
- **Axios** - HTTP Client
- **Tailwind CSS** - Styling

## Features

### Categories Management
- ✅ List all categories with pagination
- ✅ Create new category
- ✅ Update existing category (Partial Update)
- ✅ Delete category
- ✅ View product count per category

### Products Management
- ✅ List all products with pagination
- ✅ Filter by category
- ✅ Search by name or description
- ✅ Create new product
- ✅ Update existing product (Partial Update)
- ✅ Delete product
- ✅ View stock quantity and category

### Product Attributes Management
- ✅ List all attributes with pagination
- ✅ Filter by product
- ✅ Search by key
- ✅ Create new attribute
- ✅ Update existing attribute (Partial Update)
- ✅ Delete attribute

## Project Structure

```
frontend/
├── src/
│   ├── pages/
│   │   ├── CategoriesPage.tsx
│   │   ├── ProductsPage.tsx
│   │   └── ProductAttributesPage.tsx
│   ├── services/
│   │   ├── api.ts
│   │   ├── categoryService.ts
│   │   ├── productService.ts
│   │   └── productAttributeService.ts
│   ├── types/
│   │   └── index.ts
│   ├── App.tsx
│   ├── main.tsx
│   └── index.css
├── package.json
└── README.md
```

## Installation

```bash
# Install dependencies
npm install
```

## Development

### 1. Start Backend API

Make sure backend is running on `http://localhost:5132`:

```bash
cd ../StockApp
dotnet run
```

### 2. Start Frontend

```bash
npm run dev
```

Frontend will run on `http://localhost:5173`

## API Configuration

API base URL is configured in `src/services/api.ts`:

```typescript
const API_BASE_URL = 'http://localhost:5132/api';
```

## Building for Production

```bash
npm run build
```

Build output will be in `dist/` directory.

## Features in Detail

### Pagination

All list pages support pagination with the following features:
- Page navigation (Previous/Next)
- Page size: 10 items per page (configurable)
- Total pages and current page display
- Total records count

### Filtering & Search

**Products Page:**
- Filter by category (dropdown)
- Search by product name or description

**Attributes Page:**
- Filter by product (dropdown)
- Search by attribute key

### Partial Updates

All update operations support partial updates:
- Only send fields you want to update
- Unchanged fields keep their current values
- Perfect for quick edits (e.g., just update stock quantity)

### Real-time Data

- Uses React Query for automatic cache invalidation
- Optimistic updates for better UX
- Automatic refetching after mutations

## Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Run ESLint

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## API Endpoints

### Categories
- GET `/api/categories` - List categories (paginated)
- GET `/api/categories/{id}` - Get category by ID
- POST `/api/categories` - Create category
- PUT `/api/categories/{id}` - Update category
- DELETE `/api/categories/{id}` - Delete category

### Products
- GET `/api/products` - List products (paginated, filterable, searchable)
- GET `/api/products/{id}` - Get product by ID
- POST `/api/products` - Create product
- PUT `/api/products/{id}` - Update product
- DELETE `/api/products/{id}` - Delete product

### Product Attributes
- GET `/api/product-attributes` - List attributes (paginated, filterable, searchable)
- GET `/api/product-attributes/{id}` - Get attribute by ID
- POST `/api/product-attributes` - Create attribute
- PUT `/api/product-attributes/{id}` - Update attribute
- DELETE `/api/product-attributes/{id}` - Delete attribute

## Troubleshooting

### CORS Errors

Make sure backend has CORS enabled for `http://localhost:5173`:

```csharp
// In Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ...

app.UseCors("AllowFrontend");
```

### API Connection Issues

1. Check if backend is running
2. Verify API base URL in `src/services/api.ts`
3. Check browser console for errors

### Build Errors

```bash
# Clear node_modules and reinstall
rm -rf node_modules package-lock.json
npm install
```

## License

MIT
