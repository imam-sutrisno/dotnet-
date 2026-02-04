using Microsoft.AspNetCore.Mvc;
using ProductAPI.API.DTOs;
using ProductAPI.Domain.Entities;
using ProductAPI.Domain.Repositories;

namespace ProductAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        ILogger<OrdersController> logger)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetAll()
    {
        try
        {
            var orders = await _orderRepository.GetAllAsync();
            
            var response = orders.Select(o => new OrderResponseDto(
                o.OrderId,
                o.CustomerId,
                o.OrderDate,
                o.TotalAmount,
                o.Status,
                null,
                new List<OrderItemResponseDto>()
            ));
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all orders");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponseDto>> GetById(int id)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(id);
            
            if (order == null)
                return NotFound($"Order with ID {id} not found");
            
            var response = new OrderResponseDto(
                order.OrderId,
                order.CustomerId,
                order.OrderDate,
                order.TotalAmount,
                order.Status,
                null,
                new List<OrderItemResponseDto>()
            );
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get order by ID with full details (customer and items)
    /// </summary>
    [HttpGet("{id}/details")]
    public async Task<ActionResult<OrderResponseDto>> GetByIdWithDetails(int id)
    {
        try
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(id);
            
            if (order == null)
                return NotFound($"Order with ID {id} not found");
            
            var customerDto = order.Customer != null 
                ? new CustomerResponseDto(
                    order.Customer.CustomerId,
                    order.Customer.FullName,
                    order.Customer.Email,
                    order.Customer.Phone,
                    order.Customer.Address,
                    order.Customer.CreatedAt
                )
                : null;
            
            var itemsDto = order.Items.Select(item => new OrderItemResponseDto(
                item.OrderItemId,
                item.ProductId,
                item.ProductName,
                item.Quantity,
                item.UnitPrice,
                item.TotalPrice
            )).ToList();
            
            var response = new OrderResponseDto(
                order.OrderId,
                order.CustomerId,
                order.OrderDate,
                order.TotalAmount,
                order.Status,
                customerDto,
                itemsDto
            );
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order details {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get orders by customer ID
    /// </summary>
    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetByCustomerId(int customerId)
    {
        try
        {
            var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
            
            var response = orders.Select(o => new OrderResponseDto(
                o.OrderId,
                o.CustomerId,
                o.OrderDate,
                o.TotalAmount,
                o.Status,
                null,
                new List<OrderItemResponseDto>()
            ));
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for customer {CustomerId}", customerId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderResponseDto>> Create([FromBody] CreateOrderDto dto)
    {
        try
        {
            // Validate customer exists
            var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
            if (customer == null)
                return BadRequest($"Customer with ID {dto.CustomerId} not found");
            
            // Validate products and calculate total
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();
            
            foreach (var itemDto in dto.Items)
            {
                var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                    return BadRequest($"Product with ID {itemDto.ProductId} not found");
                
                if (product.Stock < itemDto.Quantity)
                    return BadRequest($"Insufficient stock for product {product.Name}");
                
                var orderItem = new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    ProductName = product.Name,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    TotalPrice = itemDto.Quantity * itemDto.UnitPrice
                };
                
                orderItems.Add(orderItem);
                totalAmount += orderItem.TotalPrice;
            }
            
            var order = new Order
            {
                CustomerId = dto.CustomerId,
                TotalAmount = totalAmount,
                Status = "Pending",
                Items = orderItems
            };
            
            var created = await _orderRepository.CreateAsync(order);
            
            var response = new OrderResponseDto(
                created.OrderId,
                created.CustomerId,
                created.OrderDate,
                created.TotalAmount,
                created.Status,
                null,
                orderItems.Select(item => new OrderItemResponseDto(
                    item.OrderItemId,
                    item.ProductId,
                    item.ProductName,
                    item.Quantity,
                    item.UnitPrice,
                    item.TotalPrice
                )).ToList()
            );
            
            return CreatedAtAction(nameof(GetById), new { id = created.OrderId }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update order status
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        try
        {
            if (id != dto.OrderId)
                return BadRequest("ID mismatch");
            
            var existing = await _orderRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound($"Order with ID {id} not found");
            
            var updated = await _orderRepository.UpdateStatusAsync(id, dto.Status);
            
            if (!updated)
                return StatusCode(500, "Failed to update order status");
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
