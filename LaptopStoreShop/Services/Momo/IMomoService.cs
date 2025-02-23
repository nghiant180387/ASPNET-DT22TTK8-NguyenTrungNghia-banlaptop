using LaptopStoreShop.Models;
using LaptopStoreShop.Models.Momo;

namespace LaptopStoreShop.Services.Momo
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfo model);
        Task<MomoExecuteResponseModel> PaymentExecuteAsync(IQueryCollection collection);
    }
}
