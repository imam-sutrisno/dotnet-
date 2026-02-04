using Microsoft.AspNetCore.Mvc;
using ProductAPI.API.DTOs;
using ProductAPI.Domain.Entities;
using ProductAPI.Domain.Repositories;

namespace ProductAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(
        ICustomerRepository customerRepository,
        ILogger<CustomersController> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all customers
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerResponseDto>>> GetAll()
    {
        try
        {
            var customers = await _customerRepository.GetAllAsync();
            
            var response = customers.Select(c => new CustomerResponseDto(
                c.CustomerId,
                c.FullName,
                c.Email,
                c.Phone,
                c.Address,
                c.CreatedAt
            ));
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all customers");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerResponseDto>> GetById(int id)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            
            if (customer == null)
                return NotFound($"Customer with ID {id} not found");
            
            var response = new CustomerResponseDto(
                customer.CustomerId,
                customer.FullName,
                customer.Email,
                customer.Phone,
                customer.Address,
                customer.CreatedAt
            );
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer {CustomerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get customer by email
    /// </summary>
    [HttpGet("email/{email}")]
    public async Task<ActionResult<CustomerResponseDto>> GetByEmail(string email)
    {
        try
        {
            var customer = await _customerRepository.GetByEmailAsync(email);
            
            if (customer == null)
                return NotFound($"Customer with email {email} not found");
            
            var response = new CustomerResponseDto(
                customer.CustomerId,
                customer.FullName,
                customer.Email,
                customer.Phone,
                customer.Address,
                customer.CreatedAt
            );
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer by email {Email}", email);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CustomerResponseDto>> Create([FromBody] CreateCustomerDto dto)
    {
        try
        {
            // Check if email already exists
            var existing = await _customerRepository.GetByEmailAsync(dto.Email);
            if (existing != null)
                return BadRequest("Email already exists");
            
            var customer = new Customer
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address
            };
            
            var created = await _customerRepository.CreateAsync(customer);
            
            var response = new CustomerResponseDto(
                created.CustomerId,
                created.FullName,
                created.Email,
                created.Phone,
                created.Address,
                created.CreatedAt
            );
            
            return CreatedAtAction(nameof(GetById), new { id = created.CustomerId }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing customer
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateCustomerDto dto)
    {
        try
        {
            if (id != dto.CustomerId)
                return BadRequest("ID mismatch");
            
            var existing = await _customerRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound($"Customer with ID {id} not found");
            
            var customer = new Customer
            {
                CustomerId = dto.CustomerId,
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address
            };
            
            var updated = await _customerRepository.UpdateAsync(customer);
            
            if (!updated)
                return StatusCode(500, "Failed to update customer");
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {CustomerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a customer
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var existing = await _customerRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound($"Customer with ID {id} not found");
            
            var deleted = await _customerRepository.DeleteAsync(id);
            
            if (!deleted)
                return StatusCode(500, "Failed to delete customer");
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
