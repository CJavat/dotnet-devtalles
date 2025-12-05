using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Models.Dtos.Responses;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersionNeutral]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        // Eliminado IMapper, Mapster se usa de forma estática

        public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProducts()
        {
            var products = _productRepository.GetProducts();
            var productsDto = products.Adapt<List<ProductDto>>();

            return Ok(productsDto);
        }

        [AllowAnonymous]
        [HttpGet("{productId:int}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProduct(int productId)
        {
            var product = _productRepository.GetProduct(productId);
            if (product == null) return NotFound($"El producto con el id {productId} no existe.");

            var productDto = product.Adapt<ProductDto>();

            return Ok(productDto);
        }

        [AllowAnonymous]
        [HttpGet("Paged", Name = "GetProductsInPage")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductsInPage([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("El número de página y el tamaño de página deben ser mayores que cero.");

            var totalProducts = _productRepository.GetTotalProducts();
            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            if (pageNumber > totalPages)
                return BadRequest($"El número de página excede el total de páginas disponibles ({totalPages}).");

            var products = _productRepository.GetProductsInPages(pageNumber, pageSize);
            var productsDto = products.Adapt<List<ProductDto>>();

            var paginationResponse = new PaginationResponse<ProductDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                Items = productsDto

            };

            return Ok(paginationResponse);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateProduct([FromForm] CreateProductDto createProductDto)
        {
            if (createProductDto == null) return BadRequest(ModelState);

            if (_productRepository.ProductExists(createProductDto.Name))
            {
                ModelState.AddModelError("CustomError", "El producto ya existe.");
                return BadRequest(ModelState);
            }

            if (!_categoryRepository.CategoryExists(createProductDto.CategoryId))
            {
                ModelState.AddModelError("CustomError", $"La categoría con el id {createProductDto.CategoryId} no existe.");
                return BadRequest(ModelState);
            }

            //? Agregando imagen.
            var product = createProductDto.Adapt<Product>();
            if (createProductDto.Image != null)
            {
                UploadProductImage(createProductDto, product);
            }
            else
            {
                product.ImageUrl = "https://placehold.co/300x300";
            }

            if (!_productRepository.CreateProduct(product))
            {
                ModelState.AddModelError("CustomError", $"Algo salió mal al guardar el producto {product.Name}");
                return StatusCode(500, ModelState);
            }

            var createdProduct = _productRepository.GetProduct(product.ProductId);
            var productDto = createdProduct.Adapt<ProductDto>();
            return CreatedAtRoute("GetProduct", new { productId = product.ProductId }, productDto);
        }

        [HttpGet("searchProductByCategory/{categoryId:int}", Name = "GetProductsForCategory")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductsForCategory(int categoryId)
        {
            var product = _productRepository.GetProductsForCategory(categoryId);
            if (product.Count == 0) return NotFound($"El producto con la categoría {categoryId} no existe.");

            var productDto = product.Adapt<List<ProductDto>>();

            return Ok(productDto);
        }

        [HttpGet("searchProductByNameDescription/{searchTem}", Name = "SearchProducts")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult SearchProducts(string searchTem)
        {
            var products = _productRepository.SearchProducts(searchTem);
            if (products.Count == 0) return NotFound($"El producto con el nombre o descripción '{searchTem}' no existe.");

            var productDto = products.Adapt<List<ProductDto>>();

            return Ok(productDto);
        }

        [HttpPatch("buyProduct/{name}/{quantity:int}", Name = "BuyProduct")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult BuyProduct(string name, int quantity)
        {
            if (string.IsNullOrEmpty(name) || quantity <= 0) return BadRequest("El nombre del producto o la cantidad no son válidos.");

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

        [HttpPut("{productId:int}", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateProduct(int productId, [FromForm] UpdateProductDto updateProductDto)
        {
            if (updateProductDto == null) return BadRequest(ModelState);

            if (!_productRepository.ProductExists(productId))
            {
                ModelState.AddModelError("CustomError", "El producto no existe.");
                return BadRequest(ModelState);
            }

            if (!_categoryRepository.CategoryExists(updateProductDto.CategoryId))
            {
                ModelState.AddModelError("CustomError", $"La categoría con el id {updateProductDto.CategoryId} no existe.");
                return BadRequest(ModelState);
            }

            var product = updateProductDto.Adapt<Product>();
            product.ProductId = productId;

            //? Agregando imagen.
            if (updateProductDto.Image != null)
            {
                UploadProductImage(updateProductDto, product);
            }
            else
            {
                product.ImageUrl = "https://placehold.co/300x300";
            }

            if (!_productRepository.UpdateProduct(product))
            {
                ModelState.AddModelError("CustomError", $"Algo salió mal al actualizar el producto {product.Name}");
                return StatusCode(500, ModelState);
            }


            return NoContent();
        }

        [HttpDelete("{productId:int}", Name = "DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult DeleteProduct(int productId)
        {
            if (productId == 0) return BadRequest(ModelState);

            var product = _productRepository.GetProduct(productId);
            if (product == null) return NotFound($"El producto con el id {productId} no existe.");

            if (!_productRepository.DeleteProduct(product))
            {
                ModelState.AddModelError("CustomError", "Algo salió mal al eliminar el registro.");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        private void UploadProductImage(dynamic productDto, Product product)
        {
            string fileName = product.ProductId + Guid.NewGuid().ToString() + Path.GetExtension(productDto.Image.FileName);

            var imagesFolder = Path.Combine(Directory.GetCurrentDirectory() + "/wwwroot", "ProductsImages");
            if (!Directory.Exists(imagesFolder))
            {//? Crear un directorio sino existe.
                Directory.CreateDirectory(imagesFolder);
            }

            var filePath = Path.Combine(imagesFolder, fileName);

            FileInfo file = new FileInfo(filePath);
            if (file.Exists)
            {
                file.Delete();
            }

            using var fileStream = new FileStream(filePath, FileMode.Create);
            productDto.Image.CopyTo(fileStream);

            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
            product.ImageUrl = $"{baseUrl}/ProductsImages/{fileName}";
            product.ImageUrlLocal = filePath;
        }

    }
}
