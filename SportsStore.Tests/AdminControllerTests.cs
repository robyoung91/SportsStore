using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Moq;
using SportsStore.Models;
using System.Linq;
using SportsStore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SportsStore.Tests
{
    public class AdminControllerTests
    {
        [Fact]
        public void Index_Contains_All_Products()
        {
            // Arrange - create a mock repository
            var mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
            {
                new Product {ProductId = 1, Name = "P1"},
                new Product {ProductId = 2, Name = "P2"},
                new Product {ProductId = 3, Name = "P3"},
            }.AsQueryable<Product>());

            // Arrange - create a controller
            var target = new AdminController(mock.Object);

            // Act
            Product[] result = GetViewModel<IEnumerable<Product>>(target.Index())?.ToArray();
        }

        private T GetViewModel<T>(IActionResult result) where T : class
        {
            return (result as ViewResult)?.ViewData.Model as T;
        }

        [Fact]
        public void Can_Edit_Product()
        {
            // Arrange - create mock repository
            var mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
            {
                new Product {ProductId = 1, Name = "P1"},
                new Product {ProductId = 2, Name = "P2"},
                new Product {ProductId = 3, Name = "P3"},
            }.AsQueryable<Product>());

            // Arrange - create a controller
            var target = new AdminController(mock.Object);

            // Act
            var p1 = GetViewModel<Product>(target.Edit(1));
            var p2 = GetViewModel<Product>(target.Edit(2));
            var p3 = GetViewModel<Product>(target.Edit(3));

            // Assert
            Assert.Equal(1, p1.ProductId);
            Assert.Equal(2, p2.ProductId);
            Assert.Equal(3, p3.ProductId);
        }

        [Fact]
        public void Cannot_Edit_Nonexistent_Product()
        {
            // Arrange - create mock repository
            var mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
            {
                new Product {ProductId = 1, Name = "P1"},
                new Product {ProductId = 2, Name = "P2"},
                new Product {ProductId = 3, Name = "P3"},
            }.AsQueryable<Product>());

            // Arrange - create a controller
            var target = new AdminController(mock.Object);

            // Act
            var result = GetViewModel<Product>(target.Edit(4));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Can_Save_Valid_Changes()
        {
            // Arrange - create mock repository
            var mock = new Mock<IProductRepository>();

            // Arrange - create mock temp data
            var tempData = new Mock<ITempDataDictionary>();

            // Arrange - create the controller
            var target = new AdminController(mock.Object)
            {
                TempData = tempData.Object
            };

            // Arrange - create a product
            var product = new Product { Name = "Test" };

            // Act - try to save the product
            var result = target.Edit(product);

            // Assert - check that the repository was called
            mock.Verify(m => m.SaveProduct(product));

            // Assert - check that the result type is a redirection
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
        }

        [Fact]
        public void Cannot_Save_Invalid_Changes()
        {
            // Arrange - create a mock repository
            var mock = new Mock<IProductRepository>();

            // Arrange - create the controller
            var target = new AdminController(mock.Object);

            // Arrange - create the new product
            var product = new Product { Name = "Test" };

            // Arrange - add an error to the model state
            target.ModelState.AddModelError("error", "error");

            // Act - try to save the product
            var result = target.Edit(product);

            // Assert - check that repository not called
            mock.Verify(m => m.SaveProduct(It.IsAny<Product>()), Times.Never);

            // Assert - check method result type
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Can_Delete_Valid_Products()
        {
            // Arrange - create a product
            var prod = new Product { ProductId = 2, Name = "Test" };

            // Arrange - create a mock repository
            var mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] 
            {   new Product {ProductId = 1, Name = "P1"},
                prod,
                new Product {ProductId = 3, Name = "P3"},
            }.AsQueryable<Product>());

            // Arrange - create the controller
            var target = new AdminController(mock.Object);

            // Act - delete the product
            target.Delete(prod.ProductId);

            // Assert - ensure repository delete method called w/ correct product
            mock.Verify(m => m.DeleteProduct(prod.ProductId), Times.Once);

        }
    }
}
