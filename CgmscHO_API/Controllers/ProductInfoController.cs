using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CgmscHO_API.Models;
using System.Xml;

namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductInfoController : ControllerBase
    {
        private readonly OraDbContext _context;

        public ProductInfoController(OraDbContext context)
        {
            _context = context;
        }

        // GET: api/ProductInfo
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductsInfo>>> GetProducts()
        {
          if (_context.Products == null)
          {
              return NotFound();
          }
            return await _context.Products.ToListAsync();
        }



        // GET: api/ProductInfo/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductsInfo>> GetProductsInfo(int id)
        {
          if (_context.Products == null)
          {
              return NotFound();
          }
            var productsInfo = await _context.Products.FindAsync(id);

            if (productsInfo == null)
            {
                return NotFound();
            }

            return productsInfo;
        }

        // PUT: api/ProductInfo/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductsInfo(int id, ProductsInfo productsInfo)
        {
            if (id != productsInfo.PRODUCTRECORDID)
            {
                return BadRequest();
            }

            _context.Entry(productsInfo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductsInfoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ProductInfo
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProductsInfo>> PostProductsInfo(ProductsInfo productsInfo)
        {
          if (_context.Products == null)
          {
              return Problem("Entity set 'OraDbContext.Products'  is null.");
          }
            _context.Products.Add(productsInfo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProductsInfo", new { id = productsInfo.PRODUCTRECORDID }, productsInfo);
        }

        // DELETE: api/ProductInfo/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductsInfo(int id)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }
            var productsInfo = await _context.Products.FindAsync(id);
            if (productsInfo == null)
            {
                return NotFound();
            }

            _context.Products.Remove(productsInfo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductsInfoExists(int id)
        {
            return (_context.Products?.Any(e => e.PRODUCTRECORDID == id)).GetValueOrDefault();
        }

       
    }
}
