namespace BestStoreApi.Services;

public static class OrderHelper
{
    public static decimal ShippingFee { get; } = 5;

    public static Dictionary<string, string> PaymentMethods { get; } = new()
    {
        { "Cash", "Cash on delivery" },
        { "PayPal", "PayPal" },
        { "Credit Card", "Credit Card" },
    };

    public static List<string> PaymentStatuses { get; } = new()
    {
        "Pending", "Accepted", "Cancelled"
    };

    public static List<string> OrderStatuses { get; } = new()
    {
        "Created", "Accepted", "Cancelled", "Shipped", "Delivered", "Returned"
    };
    
    public static Dictionary<int, int> GetProductDictionary(string productIdentifiers)
    {
        var productDic = new Dictionary<int, int>();
        if (productIdentifiers.Length > 0)
        {
            string[] productIdArray = productIdentifiers.Split('-');
            foreach (var productId in productIdArray)
            {
                try
                {
                    int id = int.Parse(productId);
                    if (productDic.ContainsKey(id))
                    {
                        productDic[id]++;
                    }
                    else
                    {
                        productDic.Add(id, 1);
                    }
                }
                catch (Exception){ }
            }
        }
        return productDic;
    } 
}