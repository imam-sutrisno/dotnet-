namespace ProductAPI.API.DTOs;

public record CreateOrderDto(
    int CustomerId,
    List<CreateOrderItemDto> Items
);

public record CreateOrderItemDto(
    int ProductId,
    int Quantity,
    decimal UnitPrice
);

public record OrderResponseDto(
    int OrderId,
    int CustomerId,
    DateTime OrderDate,
    decimal TotalAmount,
    string Status,
    CustomerResponseDto? Customer,
    List<OrderItemResponseDto> Items
);

public record OrderItemResponseDto(
    int OrderItemId,
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);

public record UpdateOrderStatusDto(
    int OrderId,
    string Status
);
