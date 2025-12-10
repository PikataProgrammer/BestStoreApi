using BestStoreApi.Dtos;
using BestStoreApi.Models;
using BestStoreApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BestStoreApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    private readonly List<string> listCategories = new List<string>()
    {
        "Phones", "Computers", "Accessories", "Printers", "Cameras", "Other"
    };
    public ProductsController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpGet("categories")]
    public IActionResult GetCategories()
    {
        return Ok(listCategories);
    }

    [HttpGet]
    public IActionResult GetProducts(string? search, string? category, int? minPrice, int? maxPrice, string? sort, string? order, int? page)
    {
        IQueryable<Product> query = _context.Products;
        
        //search funcionality
        if (search != null)
        {
            query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
        }

        if (category != null)
        {
            query = query.Where(p => p.Category == category);
        }

        if (minPrice != null)
        {
            query = query.Where(p => p.Price >= minPrice);
        }

        if (maxPrice != null)
        {
            query = query.Where(p => p.Price <= maxPrice);
        }
        if (sort == null) sort = "id";
        if(order == null || order != "asc") order = "desc";

        if (sort.ToLower() == "name")
        {
            if (order == "asc")
            {
                query = query.OrderBy(x => x.Name);
            }
            else
            {
                query = query.OrderByDescending(x => x.Name);
            }
        }
        else if (sort.ToLower() == "brand")
        {
            if (order == "asc")
            {
                query = query.OrderBy(x => x.Brand);
            }
            else
            {
                query = query.OrderByDescending(x => x.Brand);
            }
        }
        else if (sort.ToLower() == "category")
        {
            if (order == "asc")
            {
                query = query.OrderBy(x => x.Category);
            }
            else
            {
                query = query.OrderByDescending(x => x.Category);
            }
        }
        else if (sort.ToLower() == "price")
        {
            if (order == "asc")
            {
                query = query.OrderBy(x => x.Price);
            }
            else
            {
                query = query.OrderByDescending(x => x.Price);
            }
        }
        else if (sort.ToLower() == "date")
        {
            if (order == "asc")
            {
                query = query.OrderBy(x => x.CreatedAt);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedAt);
            }
        }
        else
        {
            if (order == "asc")
            {
                query = query.OrderBy(x => x.Id);
            }
            else
            {
                query = query.OrderByDescending(x => x.Id);
            }
        }

        if (page == null || page < 1)
        {
            page = 1;
        }

        int pageSize = 5;
        int totalPages = 0;
        
        decimal count = query.Count();
        totalPages = (int)Math.Ceiling(count / pageSize);

        query = query.Skip((int)((page - 1) * pageSize)).Take(pageSize);
        
        var products = query.ToList();

        var response = new
        {
            Products = products,
            TotalPages = totalPages,
            PageSize = pageSize,
            Page = page
        };
        
        return Ok(response);
    }

    [HttpGet("{id}")]
    public IActionResult GetProduct(int id)
    {
        var product = _context.Products.Find(id);
        if (product == null)
        {
            return NotFound();
        }
        
        return Ok(product);
    }

    [HttpPost]
    public IActionResult CreateProduct([FromForm] ProductDto productDto)
    {
        if (!listCategories.Contains(productDto.Category))
        {
            ModelState.AddModelError("Category", "Category doesn't exist");
            return BadRequest(ModelState);
        }
        if (productDto.ImageFile == null)
        {
            ModelState.AddModelError("ImageFileName", "Please select an image file");
            return BadRequest(ModelState);
        }
        //save the image in the server

        string imageFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        imageFileName += Path.GetExtension(productDto.ImageFile.FileName);
        
        string imagesFolder = _env.WebRootPath + "/images/products";
        using (var stream = System.IO.File.Create(imagesFolder + imageFileName))
        {
            productDto.ImageFile.CopyTo(stream);
        }
        
        Product product = new Product()
        {
            Name = productDto.Name,
            Brand = productDto.Brand,
            Category = productDto.Category,
            Price = productDto.Price,
            Description = productDto.Description ?? string.Empty,
            ImageFileName = imageFileName,
            CreatedAt = DateTime.Now
        };
        
        _context.Products.Add(product);
        _context.SaveChanges();
        return Ok(product);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateProduct(int id, [FromForm] ProductDto productDto)
    {
        if (!listCategories.Contains(productDto.Category))
        {
            ModelState.AddModelError("Category", "Category doesn't exist");
            return BadRequest(ModelState);
        }
        var product = _context.Products.Find(id);
        if (product == null)
        {
            return NotFound();
        }
        
        string imageFileName = product.ImageFileName;
        if (productDto.ImageFile != null)
        {
            //save the image in the server
            imageFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            imageFileName += Path.GetExtension(productDto.ImageFile.FileName);
            string imagesFolder = _env.WebRootPath + "/images/products";
            using (var stream = System.IO.File.Create(imagesFolder + imageFileName))
            {
                productDto.ImageFile.CopyTo(stream);
            }
             //delete the old image
             System.IO.File.Delete(imagesFolder + imageFileName);
        }
        
        //update the product in the database
        product.Name = productDto.Name;
        product.Brand = productDto.Brand;
        product.Category = productDto.Category;
        product.Price = productDto.Price;
        product.Description = productDto.Description ?? string.Empty;
        product.ImageFileName = imageFileName;
        
        _context.SaveChanges();
        
        return Ok(product);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteProduct(int id)
    {
        var product = _context.Products.Find(id);
        if (product == null)
        {
            return NotFound();
        }
        //delete images on the server
        string imagesFolder = _env.WebRootPath + "/images/products";
        System.IO.File.Delete(imagesFolder + product.ImageFileName);
        
        _context.Products.Remove(product);
        _context.SaveChanges();
        return NoContent();
    }
    
}