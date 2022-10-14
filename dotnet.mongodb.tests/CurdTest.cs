using dotnet.mongodb.core;
using dotnet.mongodb.models;
using MongoDB.Driver;
using Xunit.Abstractions;

namespace dotnet.mongodb.tests
{
    public class CurdTest
    {
        private readonly ITestOutputHelper _output;

        public CurdTest(ITestOutputHelper output)
        {
            this._output = output;
        }

        private static readonly MongoDbHelper _client = new("demo");

        [Fact]
        public void InsertProduct()
        {
            var product = new Product()
            {
                Name = "NAC55-1",
                Brand = "inter",
                MFGD = DateTime.Now,
                Quantity = 2,
                Details = "Conn Wire to Board"
            };
            _client.Insert("product", product);
            _output.WriteLine("----------InsertProduct----------");
            _output.WriteLine($"Product:{product.Name}");
        }

        [Fact]
        public void GetProductsList()
        {
            var productList = _client.Find<Product>("product", a => a.Name!=null).ToList();
            _output.WriteLine("----------Product list----------");
            foreach (var product in productList)
            {
                _output.WriteLine($"id:{product.Id},name:{product.Name},brand:{product.Brand}");
            }
        }

        [Theory]
        [InlineData(1,3)]
        public void GetProductsPage(int pageIndex,int pageSize)
        {
            var productList = _client.FindByPage<Product, object>("product", a => a.Name != null, a => a.MFGD, pageIndex, pageSize, out int resCount);
            _output.WriteLine("----------GetProductsPage----------");
            foreach (var product in productList)
            {
                _output.WriteLine($"id:{product.Id},name:{product.Name},brand:{product.Brand}");
            }
            _output.WriteLine($"All products count:{resCount}");
        }

        [Fact]
        public void ProductCount()
        {
            var count = _client.Count<Product>("product");
            _output.WriteLine("----------ProductCount----------");
            _output.WriteLine($"All product count:{count}");
            Assert.True(count > 0);
        }

        [Fact]
        public void UpdateProduct()
        {
            var update = Builders<Product>.Update
    .Set(p => p.Quantity, 88);
            Assert.True(_client.Update<Product>("product", a => a.Name == "NAC55", update));
            _output.WriteLine("----------UpdateProduct----------");
        }

        [Theory]
        [InlineData("NAC55")]
        public void DeleteProduct(string productName)
        {
           var result= _client.Delete<Product>("product", a => a.Name== productName);
            _output.WriteLine($"Delete success:{productName}");
            Assert.True(result);
        }
    }
}
