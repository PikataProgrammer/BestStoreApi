using BestStoreApi.Dtos;
using BestStoreApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public CartController(ApplicationDbContext context)
    {
        _context = context;    
    }

    [HttpGet("PaymentMethods")]
    public IActionResult GetPaymentMethods()
    {
        return Ok(OrderHelper.PaymentMethods);
    }

    [HttpGet]
    public IActionResult GetCart(string productIdentifiers)
    {
        CartDto cart = new CartDto();
        cart.CartItems = new List<CartItemDto>();
        cart.SubTotal = 0;
        cart.ShippingFee = OrderHelper.ShippingFee;
        cart.TotalPrice = 0;
        var productDictionary = OrderHelper.GetProductDictionary(productIdentifiers);

        foreach (var pair in productDictionary)
        {
            int productId = pair.Key;
            var product = _context.Products.Find(productId);
            if (product == null)
            {
                continue;
            }
            
            var cartItemDto = new CartItemDto();
            cartItemDto.Product =  product;
            cartItemDto.Quantity = pair.Value;
            
            cart.CartItems.Add(cartItemDto);
            cart.SubTotal += product.Price * pair.Value;
            cart.TotalPrice += cart.SubTotal * cart.SubTotal;
        }
        return Ok(cart);
    }
}