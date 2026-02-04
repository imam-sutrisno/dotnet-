using Microsoft.AspNetCore.Mvc;
using ProductAPI.API.DTOs;
using ProductAPI.Domain.Entities;
using ProductAPI.Domain.Repositories;

namespace ProductAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductRepository productRepository,
        ILogger<ProductsController> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetAll()
    {
        try
        {
            var products = await _productRepository.GetAllAsync();
            
            var response = products.Select(p => new ProductResponseDto(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Stock,
                p.Category,
                p.CreatedAt,
                p.UpdatedAt
            ));
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponseDto>> GetById(int id)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            
            if (product == null)
                return NotFound($"Product with ID {id} not found");
            
            var response = new ProductResponseDto(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.Stock,
                product.Category,
                product.CreatedAt,
                product.UpdatedAt
            );
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {ProductId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetByCategory(string category)
    {
        try
        {
            var products = await _productRepository.GetByCategoryAsync(category);
            
            var response = products.Select(p => new ProductResponseDto(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Stock,
                p.Category,
                p.CreatedAt,
                p.UpdatedAt
            ));
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products by category {Category}", category);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Search products by name
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ProductResponseDto>>> Search([FromQuery] string term)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest("Search term is required");
            
            var products = await _productRepository.SearchByNameAsync(term);
            
            var response = products.Select(p => new ProductResponseDto(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Stock,
                p.Category,
                p.CreatedAt,
                p.UpdatedAt
            ));
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products with term {SearchTerm}", term);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> Create([FromBody] CreateProductDto dto)
    {
        try
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                Category = dto.Category
            };
            
            var created = await _productRepository.CreateAsync(product);
            
            var response = new ProductResponseDto(
                created.Id,
                created.Name,
                created.Description,
                created.Price,
                created.Stock,
                created.Category,
                created.CreatedAt,
                created.UpdatedAt
            );
            
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch");
            
            var existing = await _productRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound($"Product with ID {id} not found");
            
            var product = new Product
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                Category = dto.Category
            };
            
            var updated = await _productRepository.UpdateAsync(product);
            
            if (!updated)
                return StatusCode(500, "Failed to update product");
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var existing = await _productRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound($"Product with ID {id} not found");
            
            var deleted = await _productRepository.DeleteAsync(id);
            
            if (!deleted)
                return StatusCode(500, "Failed to delete product");
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get total product count
    /// </summary>
    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCount()
    {
        try
        {
            var count = await _productRepository.GetTotalCountAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product count");
            return StatusCode(500, "Internal server error");
        }
    }
}
