using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProducts()
        {
            var products = _productRepository.GetProducts();
            var productsDto = _mapper.Map<List<ProductDto>>(products);

            return Ok(productsDto);
        }

        [AllowAnonymous]
        [HttpGet("{productId:int}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProduct(int productId)
        {
            var product = _productRepository.GetProduct(productId);
            if (product == null) return NotFound($"El producto con el ID {productId} no existe.");

            var productDto = _mapper.Map<ProductDto>(product);

            return Ok(productDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public IActionResult CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            if (createProductDto == null) return BadRequest(ModelState);
            if (_productRepository.ProductExists(createProductDto.Name))
            {
                ModelState.AddModelError("CustomError", "El producto ya existe");
                return BadRequest(ModelState);
            }
            if (!_categoryRepository.CategoryExists(createProductDto.CategoryId))
            {
                ModelState.AddModelError("CustomError", $"La categoría con el ID {createProductDto.CategoryId} no existe");
                return BadRequest(ModelState);
            }

            var product = _mapper.Map<Product>(createProductDto);
            if (!_productRepository.CreateProduct(product))
            {
                ModelState.AddModelError("CustomError", $"Algo salió mal al guardar el registro {product.Name}");
                return StatusCode(500, ModelState);
            }

            var createdProduct = _productRepository.GetProduct(product.ProductId);
            var productDto = _mapper.Map<ProductDto>(createdProduct);
            return CreatedAtRoute("GetProduct", new { productId = product.ProductId }, productDto);
        }

        [HttpGet("searchProductByCategory/{categoryId:int}", Name = "GetProductsForCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductsForCategory(int categoryId)
        {
            var products = _productRepository.GetProductsForCategory(categoryId);
            if (products.Count == 0) return NotFound($"Los productos con la categoría {categoryId} no existe.");

            var productsDto = _mapper.Map<List<ProductDto>>(products);

            return Ok(productsDto);
        }

        [HttpGet("searchProductByNameDescription/{searchTerm}", Name = "SearchProducts")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult SearchProducts(string searchTerm)
        {
            var products = _productRepository.SearchProducts(searchTerm);
            if (products.Count == 0) return NotFound($"Los productos con el nombre o descripción '{searchTerm}' no existe.");

            var productsDto = _mapper.Map<List<ProductDto>>(products);

            return Ok(productsDto);
        }

        [HttpPatch("buyProduct/{name}/{quantity:int}", Name = "buyProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult BuyProduct(string name, int quantity)
        {
            if (string.IsNullOrEmpty(name) || quantity <= 0) return BadRequest("El nombre o la cantidad no son válidos");

            var foundProduct = _productRepository.ProductExists(name);
            if (!foundProduct) return NotFound($"El producto con el nombre {name} no existe.");

            if (!_productRepository.BuyProduct(name, quantity))
            {
                ModelState.AddModelError("CustomError", $"No se pudo comprar el producto {name} o la cantidad solicitada es mayor al stock disponible.");
                return BadRequest(ModelState);
            }

            var units = quantity == 1 ? "unidad" : "unidades";
            return Ok($"Se compró {quantity} {units} del producto '{name}'");
        }

        [HttpPut("{productId:int}", Name = "UpadteProduct")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public IActionResult UpadteProduct(int productId, [FromBody] UpdateProductDto updateProductDto)
        {
            if (updateProductDto == null) return BadRequest(ModelState);
            if (!_productRepository.ProductExists(productId)) return NotFound("El producto que intentas actualizar no existe.");
            if (!_categoryRepository.CategoryExists(updateProductDto.CategoryId))
            {
                ModelState.AddModelError("CustomError", $"La categoría con el ID {updateProductDto.CategoryId} no existe");
                return BadRequest(ModelState);
            }

            var product = _mapper.Map<Product>(updateProductDto);
            product.ProductId = productId;
            if (!_productRepository.UpdateProduct(product))
            {
                ModelState.AddModelError("CustomError", $"Algo salió mal al actualizar el producto {product.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [HttpDelete("{productId:int}", Name = "DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult DeleteProduct(int productId)
        {
            if (productId == 0) return BadRequest(ModelState);

            var product = _productRepository.GetProduct(productId);
            if (product == null) return NotFound($"El producto con el ID {productId} no existe.");

            if (!_productRepository.DeleteProduct(product))
            {
                ModelState.AddModelError("CustomError", $"Algo salió mal al elminar el registro {product.Name}.");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

    }
}
