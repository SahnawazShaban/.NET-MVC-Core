using Mango.Services.ShoppingCartAPI.Models.Dto;

namespace Mango.Services.ShoppingCartAPI.Service.IService
{
    // load all the product from ProductAPI
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetProducts();
    }
}