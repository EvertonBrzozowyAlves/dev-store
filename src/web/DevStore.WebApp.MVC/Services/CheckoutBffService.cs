using DevStore.Core.Communication;
using DevStore.WebApp.MVC.Extensions;
using DevStore.WebApp.MVC.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DevStore.WebApp.MVC.Services
{
    public interface ICheckoutBffService
    {
        // Shopping cart
        Task<ShoppingCartViewModel> GetShoppingCart();
        Task<int> GetShoppingCartItemsQuantity();
        Task<ResponseResult> AddShoppingCartItem(ShoppingCartItemViewModel carrinho);
        Task<ResponseResult> UpdateShoppingCartItem(Guid produtoId, ShoppingCartItemViewModel carrinho);
        Task<ResponseResult> RemoverItemFromShoppingCart(Guid produtoId);
        Task<ResponseResult> ApplyVoucher(string voucher);

        // Order
        Task<ResponseResult> FinishOrder(TransactionViewModel transaction);
        Task<OrderViewModel> GetLastOrder();
        Task<IEnumerable<OrderViewModel>> GetCustomersById();
        TransactionViewModel MapToOrder(ShoppingCartViewModel shoppingCart, AddressViewModel address);
    }

    public class CheckoutBffService : Service, ICheckoutBffService
    {
        private readonly HttpClient _httpClient;

        public CheckoutBffService(HttpClient httpClient, IOptions<AppSettings> settings)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(settings.Value.CheckoutBffUrl);
        }

        #region ShoppingCart

        public async Task<ShoppingCartViewModel> GetShoppingCart()
        {
            var response = await _httpClient.GetAsync("/orders/shopping-cart/");

            ManageResponseErrors(response);

            return await DeserializeResponse<ShoppingCartViewModel>(response);
        }
        public async Task<int> GetShoppingCartItemsQuantity()
        {
            var Response = await _httpClient.GetAsync("/orders/shopping-cart/quantity/");

            ManageResponseErrors(Response);

            return await DeserializeResponse<int>(Response);
        }
        public async Task<ResponseResult> AddShoppingCartItem(ShoppingCartItemViewModel carrinho)
        {
            var itemContent = GetContent(carrinho);

            var Response = await _httpClient.PostAsync("/orders/shopping-cart/items/", itemContent);

            if (!ManageResponseErrors(Response)) return await DeserializeResponse<ResponseResult>(Response);

            return RetornoOk();
        }
        public async Task<ResponseResult> UpdateShoppingCartItem(Guid produtoId, ShoppingCartItemViewModel shoppingCartItem)
        {
            var itemContent = GetContent(shoppingCartItem);

            var Response = await _httpClient.PutAsync($"/orders/shopping-cart/items/{produtoId}", itemContent);

            if (!ManageResponseErrors(Response)) return await DeserializeResponse<ResponseResult>(Response);

            return RetornoOk();
        }
        public async Task<ResponseResult> RemoverItemFromShoppingCart(Guid produtoId)
        {
            var Response = await _httpClient.DeleteAsync($"/orders/shopping-cart/items/{produtoId}");

            if (!ManageResponseErrors(Response)) return await DeserializeResponse<ResponseResult>(Response);

            return RetornoOk();
        }
        public async Task<ResponseResult> ApplyVoucher(string voucher)
        {
            var itemContent = GetContent(voucher);

            var Response = await _httpClient.PostAsync("/orders/shopping-cart/aplicar-voucher/", itemContent);

            if (!ManageResponseErrors(Response)) return await DeserializeResponse<ResponseResult>(Response);

            return RetornoOk();
        }

        #endregion

        #region Order

        public async Task<ResponseResult> FinishOrder(TransactionViewModel transaction)
        {
            var ordderContent = GetContent(transaction);

            var response = await _httpClient.PostAsync("/orders", ordderContent);

            if (!ManageResponseErrors(response)) return await DeserializeResponse<ResponseResult>(response);

            return RetornoOk();
        }

        public async Task<OrderViewModel> GetLastOrder()
        {
            var response = await _httpClient.GetAsync("/orders/last");

            ManageResponseErrors(response);

            return await DeserializeResponse<OrderViewModel>(response);
        }

        public async Task<IEnumerable<OrderViewModel>> GetCustomersById()
        {
            var response = await _httpClient.GetAsync("/orders/customers");

            ManageResponseErrors(response);

            return await DeserializeResponse<IEnumerable<OrderViewModel>>(response);
        }

        public TransactionViewModel MapToOrder(ShoppingCartViewModel shoppingCart, AddressViewModel address)
        {
            var order = new TransactionViewModel
            {
                Amount = shoppingCart.Total,
                Items = shoppingCart.Items,
                Discount = shoppingCart.Discount,
                HasVoucher = shoppingCart.HasVoucher,
                Voucher = shoppingCart.Voucher?.Voucher
            };

            if (address != null)
            {
                order.Address = new AddressViewModel
                {
                    StreetAddress = address.StreetAddress,
                    BuildingNumber = address.BuildingNumber,
                    Neighborhood = address.Neighborhood,
                    ZipCode = address.ZipCode,
                    SecondaryAddress = address.SecondaryAddress,
                    City = address.City,
                    State = address.State
                };
            }

            return order;
        }

        #endregion
    }
}