using BestStoreApi.Models;

namespace BestStoreApi.Dtos;

public class CartItemDto
{
    public Product Product { get; set; } = new Product();
    public int Quantity { get; set; }
}