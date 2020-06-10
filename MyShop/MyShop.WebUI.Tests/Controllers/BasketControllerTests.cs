using System;
using System.Linq;
using System.Security.Principal;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyShop.Core.Contracts;
using MyShop.Core.Models;
using MyShop.Core.ViewModels;
using MyShop.WebUI.Controllers;
using MyShop.WebUI.Tests.Mocks;
using MyShopServices;

namespace MyShop.WebUI.Tests.Controllers
{
    [TestClass]
    public class BasketControllerTests
    {
        [TestMethod]
        public void CanAddBasketItem()
        {
            //-----------SETTING UP TESTS-----------//
            IRepository<Basket> baskets = new MockContext<Basket>();
            IRepository<Product> products = new MockContext<Product>();
            IRepository<Order> orders = new MockContext<Order>();
            IRepository<Customer> customers = new MockContext<Customer>();

            var httpContext = new MockHttpContext();

            IBasketService basketService = new BasketService(products, baskets);
            IOrderService orderService = new OrderService(orders);
            var controller = new BasketController(basketService, orderService, customers);
            controller.ControllerContext = new System.Web.Mvc.ControllerContext(httpContext, new System.Web.Routing.RouteData(), controller);


            //-----------ACTING ON YOUR TESTS-----------//
            //This is to test the service
            //basketService.AddToBasket(httpContext, "1"); 
            controller.AddToBasket("1");

            Basket basket = baskets.Collection().FirstOrDefault();


            //-----------ASSERTING YOUR TESTS-----------//
            Assert.IsNotNull(basket);
            Assert.AreEqual(1, basket.BasketItems.Count);
            Assert.AreEqual("1", basket.BasketItems.ToList().FirstOrDefault().ProductId);

        }
        
        [TestMethod]
        public void CanGetSummaryViewModel()
        {
            //-----------SETTING UP TESTS-----------//
            IRepository<Basket> baskets = new MockContext<Basket>();
            IRepository<Product> products = new MockContext<Product>();
            IRepository<Order> orders = new MockContext<Order>();
            IRepository<Customer> customers = new MockContext<Customer>();

            //Manually adding in products to the db
            products.Insert(new Product() { Id = "1", Price = 10.00m });
            products.Insert(new Product() { Id = "2", Price = 40.00m });
            products.Insert(new Product() { Id = "3", Price = 20.00m });
            //Manually creating the basket
            Basket basket = new Basket();
            basket.BasketItems.Add(new BasketItem() { ProductId = "1", Quantity = 2 });
            basket.BasketItems.Add(new BasketItem() { ProductId = "2", Quantity = 1 });
            basket.BasketItems.Add(new BasketItem() { ProductId = "3", Quantity = 3 });
            baskets.Insert(basket);

            IBasketService basketService = new BasketService(products, baskets);
            IOrderService orderService = new OrderService(orders);

            var controller = new BasketController(basketService, orderService, customers);
            var httpContext = new MockHttpContext();
            httpContext.Request.Cookies.Add(new System.Web.HttpCookie("eCommerceBasket") { Value = basket.Id });
            controller.ControllerContext = new System.Web.Mvc.ControllerContext(httpContext, new System.Web.Routing.RouteData(), controller);
            
            //-----------ACTING ON YOUR TESTS---------- -//
            var result = controller.BasketSummary() as PartialViewResult;
            var basketSummary = (BasketSummaryViewModel)result.ViewData.Model;
            
            //-----------ASSERTING YOUR TESTS-----------//
            Assert.AreEqual(6, basketSummary.BasketCount);
            Assert.AreEqual(120.00m, basketSummary.BasketTotal);
            
        }
        [TestMethod]
        public void CanCheckOutAndCreateOrder()
        {
            IRepository<Customer> customers = new MockContext<Customer>();
            IRepository<Product> products = new MockContext<Product>();
            products.Insert(new Product() { Id = "1", Price = 10.00m });
            products.Insert(new Product() { Id = "2", Price = 40.00m });
            products.Insert(new Product() { Id = "3", Price = 20.00m });

            IRepository<Basket> baskets = new MockContext<Basket>();
            Basket basket = new Basket();
            basket.BasketItems.Add(new BasketItem() { ProductId = "1", Quantity = 2, BasketId = basket.Id });
            basket.BasketItems.Add(new BasketItem() { ProductId = "2", Quantity = 1, BasketId = basket.Id });
            basket.BasketItems.Add(new BasketItem() { ProductId = "3", Quantity = 3, BasketId = basket.Id });

            baskets.Insert(basket);

            IBasketService basketService = new BasketService(products, baskets);

            IRepository<Order> orders = new MockContext<Order>();
            IOrderService orderService = new OrderService(orders);

            customers.Insert(new Customer() { Id = "1", Email = "Birkan.parlar26@gmail.com", PostCode = "SE16 2XJ" });
            IPrincipal FakeUser = new GenericPrincipal(new GenericIdentity("Birkan.parlar26@Gmail.com","Forms"), null);

            var controller = new BasketController(basketService, orderService, customers);
            //Injecting fake context so it can read and write cookies.
            var httpContext = new MockHttpContext();
            httpContext.User = FakeUser;
            //Creating the cookie its self manually
            httpContext.Request.Cookies.Add(new System.Web.HttpCookie("eCommerceBasket")
            {
                Value = basket.Id
            });
            controller.ControllerContext = new ControllerContext(httpContext, new System.Web.Routing.RouteData(), controller);
            //Act
            Order order = new Order();
            controller.CheckOut(order);

            //Assert
            Assert.AreEqual(3, order.OrderItems.Count);
            Assert.AreEqual(0, basket.BasketItems.Count);

            Order orderInRep = orders.Find(order.Id);
            Assert.AreEqual(3, orderInRep.OrderItems.Count); 
        }
    }
}
