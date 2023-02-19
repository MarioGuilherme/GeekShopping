using GeekShopping.ProductAPI.Data.ValueObjects;
using GeekShopping.ProductAPI.Repository;
using GeekShopping.ProductAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.ProductAPI.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class ProductController : ControllerBase {
    private IProductRepository _repository;

	public ProductController(IProductRepository repository) {
		this._repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductVO>>> FindAll() {
        IEnumerable<ProductVO> products = await this._repository.FindAll();
        return Ok(products);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<ProductVO?>> FindById(long id) {
		ProductVO? product = await this._repository.FindById(id);
		if (product == null) return NotFound();
		return Ok(product);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ProductVO?>> Create([FromBody] ProductVO? product) {
        if (product == null) return BadRequest();
        ProductVO? productMapped = await this._repository.Create(product);
        return Ok(productMapped);
    }

    [HttpPut]
    [Authorize]
    public async Task<ActionResult<ProductVO?>> Update([FromBody] ProductVO? product) {
        if (product == null) return BadRequest();
        ProductVO? productMapped = await this._repository.Update(product);
        return Ok(productMapped);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Role.Admin)]
    public async Task<ActionResult> Delete(long id) {
        var status = await this._repository.Delete(id);
        return status ? Ok(status) : BadRequest();
    }
}