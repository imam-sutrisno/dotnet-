namespace ProductAPI.API.DTOs;

public record CreateCustomerDto(
    string FullName,
    string Email,
    string Phone,
    string Address
);

public record UpdateCustomerDto(
    int CustomerId,
    string FullName,
    string Email,
    string Phone,
    string Address
);

public record CustomerResponseDto(
    int CustomerId,
    string FullName,
    string Email,
    string Phone,
    string Address,
    DateTime CreatedAt
);
