namespace ProductAPI.API.DTOs;

public record CreateProductDto(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string Category
);

public record UpdateProductDto(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string Category
);

public record ProductResponseDto(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string Category,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
