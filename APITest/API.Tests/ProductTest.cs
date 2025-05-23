using API.Tests.API;
using API.Tests.Models;
using System.Net.Http.Json;

namespace API.Tests;

public class ProductTest : IAsyncLifetime
{
    private const string BASE_URL = "http://shop.qatl.ru/";
    
    private static readonly ApiClient _apiClient = new(BASE_URL);
    private static readonly List<int> _createdProductIds = [];

    // IAsyncLifetime - для асинхронной инициализации/очистки
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        foreach (var id in _createdProductIds)
        {
            try
            {
                await _apiClient.DeleteProductAsync(id);
            }
            catch
            {
                // Игнорируем ошибки при удалении
            }
        }

        _createdProductIds.Clear();
    }

    public bool IsProductListContains(List<Product>? products, int id)
    {
        return products.FirstOrDefault(p => p.Id == id) is not null;
    }

    public void IsEqual(Product product1, Product product2)
    {
        Assert.True(product1.Category_id == product2.Category_id, "Идентификатор категории некорректный");
        Assert.True(product1.Title == product2.Title, "Заголовок некорректный");
        Assert.True(product1.Alias == product2.Alias, "Алиас некорректный");
        Assert.True(product1.Content == product2.Content, "Контент некорректный");
        Assert.True(product1.Price == product2.Price, "Цена некорректная");
        Assert.True(product1.Old_price == product2.Old_price, "Старая цена некорректная");
        Assert.True(product1.Status == product2.Status, "Статус некорректный");
        Assert.True(product1.Keywords == product2.Keywords, "Ключевые слова некорректные");
        Assert.True(product1.Description == product2.Description, "Описание некорректное");
        Assert.True(product1.Hit == product2.Hit, "Значение Hit некорректное");

    }

    [Theory]
    [MemberData(nameof(GetValidAddTestData))]
    public async Task Add_Valid_Product_Should_Create_Product(Product product)
    {
        var response = await _apiClient.AddProductAsync(product);
        var createdResponse = await response.Content.ReadFromJsonAsync<AddProductResponse>();
        
        List<Product>? products = await _apiClient.GetAllProductsAsync();

        Assert.True(response.IsSuccessStatusCode);
        Assert.True(IsProductListContains(products, createdResponse.Id));

        var actual = products.FirstOrDefault(p => p.Id == createdResponse.Id);

        var expected = new Product()
        {
            Id = createdResponse.Id,
            Category_id = product.Category_id,
            Title = product.Title,
            Alias = product.Alias,
            Content = product.Content,
            Price = product.Price,
            Old_price = product.Old_price,
            Status = product.Status,
            Keywords = product.Keywords,
            Description = product.Description,
            Hit = product.Hit
        };
        IsEqual(actual, expected);
        _createdProductIds.Add(createdResponse.Id);
    }

    public static TheoryData<Product> GetValidAddTestData()
    {
        return
        [
            // 
            new()
            {
                Category_id = 1,
                Title = "title",
                Content = "content",
                Price = 10,
                Old_price = 100,
                Status = 1,
                Keywords = "keywords",
                Description = "desc",
                Hit = 0
            },
            //
            new()
            {
                Category_id = 14,
                Title = "заголовок",
                Content = "контент",
                Price = 100,
                Old_price = 100,
                Status = 0,
                Keywords = "ключ слова",
                Description = "описание",
                Hit = 1
            },
        ];
    }

    [Theory]
    [MemberData(nameof(GetInvalidAddTestData))]
    public async Task Add_Invalid_Product_Should_Not_Create_Product(Product product)
    {
        var response = await _apiClient.AddProductAsync(product);
        var createdResponse = await response.Content.ReadFromJsonAsync<AddProductResponse>();

        List<Product>? products = await _apiClient.GetAllProductsAsync();

        Assert.Equal(1, createdResponse.Status);

        if (IsProductListContains(products, createdResponse.Id))
        {
            _createdProductIds.Add(createdResponse.Id);
        }

        Assert.False(IsProductListContains(products, createdResponse.Id));

        //Assert.False(IsProductListContains(products, createdResponse.Id));
    }

    public static TheoryData<Product> GetInvalidAddTestData()
    {
        return
        [
            // category id less than 1
            new()
            {
                Category_id = 0,
                Title = "title",
                Content = "content",
                Price = 10,
                Old_price = 100,
                Status = 1,
                Keywords = "keywords",
                Description = "desc",
                Hit = 1
            },
            // category id more than 15
            new()
            {
                Category_id = 15,
                Title = "title1",
                Content = "content",
                Price = 10,
                Old_price = 100,
                Status = 0,
                Keywords = "keywords",
                Description = "desc",
                Hit = 1
            },
            // status less than 0
            new()
            {
                Category_id = 1,
                Title = "title2",
                Content = "контент",
                Price = 100,
                Old_price = 100,
                Status = -1,
                Keywords = "ключ слова",
                Description = "описание",
                Hit = 1
            },
            // status more than 1
            new()
            {
                Category_id = 1,
                Title = "title3",
                Content = "контент",
                Price = 100,
                Old_price = 100,
                Status = 2,
                Keywords = "ключ слова",
                Description = "описание",
                Hit = 1
            },
            // hit less than 0
            new()
            {
                Category_id = 1,
                Title = "title4",
                Content = "контент",
                Price = 100,
                Old_price = 100,
                Status = 0,
                Keywords = "ключ слова",
                Description = "описание",
                Hit = -1
            },
            // hit more than 1
            new()
            {
                Category_id = 1,
                Title = "title5",
                Content = "контент",
                Price = 100,
                Old_price = 100,
                Status = 1,
                Keywords = "ключ слова",
                Description = "описание",
                Hit = 2
            },
        ];
    }

    [Theory]
    [MemberData(nameof(GetAddSameTitleData))]
    public async Task Add_Products_With_Same_Title_Should_Add_Alias_Prefix(Product product)
    {
        var prefix = "-0";

        var firstResponse = await _apiClient.AddProductAsync(product);
        var secondResponse = await _apiClient.AddProductAsync(product);

        var firstCreatedResponse = await firstResponse.Content.ReadFromJsonAsync<AddProductResponse>();
        var secondCreatedResponse = await secondResponse.Content.ReadFromJsonAsync<AddProductResponse>();

        Assert.True(firstResponse.IsSuccessStatusCode);
        Assert.True(secondResponse.IsSuccessStatusCode);

        //Assert.Equal(1, firstCreatedResponse.Status);
        //Assert.Equal(1, secondCreatedResponse.Status);

        var products = await _apiClient.GetAllProductsAsync();

        var firstProd = products.FirstOrDefault(p => p.Id == firstCreatedResponse.Id);
        var secondProd = products.FirstOrDefault(p => p.Id == secondCreatedResponse.Id);

        _createdProductIds.Add(firstCreatedResponse.Id);
        _createdProductIds.Add(secondCreatedResponse.Id);

        Assert.Equal($"{firstProd.Alias}{prefix}", secondProd.Alias);
    }

    public static TheoryData<Product> GetAddSameTitleData()
    {
        return
        [
            new()
            {
                Category_id = 1,
                Title = "alias_title",
                Content = "content",
                Price = 10,
                Old_price = 100,
                Status = 1,
                Keywords = "keywords",
                Description = "desc",
                Hit = 0
            },
        ];
    }

    [Theory]
    [MemberData(nameof(GetValidEditData))]
    public async Task Edit_Valid_Product_Should_Update_Product(Product product, Product updateRequest)
    {
        var createResponse = await _apiClient.AddProductAsync(product);
        var createdProductResponse = await createResponse.Content.ReadFromJsonAsync<AddProductResponse>();
        var products = await _apiClient.GetAllProductsAsync();

        Assert.True(createResponse.IsSuccessStatusCode);
        Assert.True(IsProductListContains(products, createdProductResponse.Id));
        _createdProductIds.Add(createdProductResponse.Id);

        updateRequest.Id = createdProductResponse.Id;
        var updateResponse = await _apiClient.EditProductAsync(updateRequest);


        var updatedProductResponse = await updateResponse.Content.ReadFromJsonAsync<EditDeleteProductResponse>();
        Assert.Equal(1, updatedProductResponse.Status);
        Assert.True(IsProductListContains(products, createdProductResponse.Id));

        var actual = products.FirstOrDefault(p => p.Id == updateRequest.Id);

        var expected = new Product()
        {
            Id = updateRequest.Id,
            Category_id = updateRequest.Category_id,
            Title = updateRequest.Title,
            Alias = updateRequest.Alias,
            Content = updateRequest.Content,
            Price = updateRequest.Price,
            Old_price = updateRequest.Old_price,
            Status = updateRequest.Status,
            Keywords = updateRequest.Keywords,
            Description = updateRequest.Description,
            Hit = updateRequest.Hit
        };
        IsEqual(actual, expected);
    }

    public static TheoryData<Product, Product> GetValidEditData()
    {
        return new TheoryData<Product, Product>
        {
            { new Product
                {
                    Category_id = 1,
                    Title = "valid_title",
                    Content = "content",
                    Price = 10,
                    Old_price = 100,
                    Status = 1,
                    Keywords = "keywords",
                    Description = "desc",
                    Hit = 0
                },
                new Product
                {
                    Category_id = 1,
                    Title = "new_title",
                    Content = "new_content",
                    Price = 50,
                    Old_price = 100,
                    Status = 0,
                    Keywords = "new_keywords",
                    Description = "new_desc",
                    Hit = 1
                }
            }
        };
    }

    [Theory]
    [MemberData(nameof(GetValidEditData))]
    public async Task Edit_Not_Existing_Product_Should_Not_Update(Product product, Product updateRequest)
    {
        int notExistingId = 999999;
        var products = await _apiClient.GetAllProductsAsync();

        updateRequest.Id = notExistingId;
        var updateResponse = await _apiClient.EditProductAsync(updateRequest);

        var updatedProductResponse = await updateResponse.Content.ReadFromJsonAsync<EditDeleteProductResponse>();

        //Assert.False(updateResponse.IsSuccessStatusCode);
        Assert.Equal(0, updatedProductResponse.Status);
    }

    [Theory]
    [MemberData(nameof(GetInvalidEditData))]
    public async Task Invalid_Edit_Created_Product_Should_Not_Update_Product(Product product, Product updateRequest)
    {
        var createResponse = await _apiClient.AddProductAsync(product);
        var createdProductResponse = await createResponse.Content.ReadFromJsonAsync<AddProductResponse>();

        var products = await _apiClient.GetAllProductsAsync();

        Assert.True(createResponse.IsSuccessStatusCode);
        Assert.True(IsProductListContains(products, createdProductResponse.Id));
        _createdProductIds.Add(createdProductResponse.Id);

        updateRequest.Id = createdProductResponse.Id;
        var updateResponse = await _apiClient.EditProductAsync(updateRequest);

        var updatedProductResponse = await updateResponse.Content.ReadFromJsonAsync<EditDeleteProductResponse>();
        Assert.Equal(0, updatedProductResponse.Status);
    }

    public static TheoryData<Product, Product> GetInvalidEditData()
    {
        return new TheoryData<Product, Product>
        {
            { 
                new Product
                {
                    Category_id = 1,
                    Title = "valid_title",
                    Content = "content",
                    Price = 10,
                    Old_price = 100,
                    Status = 1,
                    Keywords = "keywords",
                    Description = "desc",
                    Hit = 0
                },
                new Product
                {
                    Category_id = 16,
                    Title = "new_title",
                    Content = "new_content",
                    Price = 50,
                    Old_price = 100,
                    Status = 0,
                    Keywords = "new_keywords",
                    Description = "new_desc",
                    Hit = 1
                }
            },
            { new Product
                {
                    Category_id = 1,
                    Title = "valid_title",
                    Content = "content",
                    Price = 10,
                    Old_price = 100,
                    Status = 1,
                    Keywords = "keywords",
                    Description = "desc",
                    Hit = 0
                },
                new Product
                {
                    Category_id = 0,
                    Title = "new_title",
                    Content = "new_content",
                    Price = 50,
                    Old_price = 100,
                    Status = 0,
                    Keywords = "new_keywords",
                    Description = "new_desc",
                    Hit = 1
                }
            },
            {
                new Product
                {
                    Category_id = 1,
                    Title = "valid_title",
                    Content = "content",
                    Price = 10,
                    Old_price = 100,
                    Status = 1,
                    Keywords = "keywords",
                    Description = "desc",
                    Hit = 0
                },
                new Product
                {
                    Category_id = 2,
                    Title = "new_title",
                    Content = "new_content",
                    Price = 50,
                    Old_price = 100,
                    Status = -1,
                    Keywords = "new_keywords",
                    Description = "new_desc",
                    Hit = 1
                }
            },
            {
                new Product
                {
                    Category_id = 1,
                    Title = "valid_title",
                    Content = "content",
                    Price = 10,
                    Old_price = 100,
                    Status = 1,
                    Keywords = "keywords",
                    Description = "desc",
                    Hit = 0
                },
                new Product
                {
                    Category_id = 2,
                    Title = "new_title",
                    Content = "new_content",
                    Price = 50,
                    Old_price = 100,
                    Status = 2,
                    Keywords = "new_keywords",
                    Description = "new_desc",
                    Hit = 1
                }
            },
            {
                new Product
                {
                    Category_id = 1,
                    Title = "valid_title",
                    Content = "content",
                    Price = 10,
                    Old_price = 100,
                    Status = 1,
                    Keywords = "keywords",
                    Description = "desc",
                    Hit = 0
                },
                new Product
                {
                    Category_id = 2,
                    Title = "new_title",
                    Content = "new_content",
                    Price = 50,
                    Old_price = 100,
                    Status = 0,
                    Keywords = "new_keywords",
                    Description = "new_desc",
                    Hit = -1
                }
            },
            {
                new Product
                {
                    Category_id = 1,
                    Title = "valid_title",
                    Content = "content",
                    Price = 10,
                    Old_price = 100,
                    Status = 1,
                    Keywords = "keywords",
                    Description = "desc",
                    Hit = 0
                },
                new Product
                {
                    Category_id = 2,
                    Title = "new_title",
                    Content = "new_content",
                    Price = 50,
                    Old_price = 100,
                    Status = 0,
                    Keywords = "new_keywords",
                    Description = "new_desc",
                    Hit = 2
                }
            }
        };
    }

    [Theory]
    [MemberData(nameof(GetDeleteData))]
    public async Task Delete_Existing_Product_Should_Delete(Product product)
    {
        var response = await _apiClient.AddProductAsync(product);
        var createdResponse = await response.Content.ReadFromJsonAsync<AddProductResponse>();
        var products = await _apiClient.GetAllProductsAsync();

        Assert.True(response.IsSuccessStatusCode);
        Assert.True(IsProductListContains(products, createdResponse.Id));

        var deleteResponse = await _apiClient.DeleteProductAsync(createdResponse.Id);
        Assert.True(deleteResponse.IsSuccessStatusCode);

        var updatedProducts = await _apiClient.GetAllProductsAsync();
        Assert.False(IsProductListContains(updatedProducts, createdResponse.Id));
    }

    public static TheoryData<Product> GetDeleteData()
    {
        return
            [
                new()
                {
                    Category_id = 1,
                    Title = "new_title",
                    Content = "new_content",
                    Price = 50,
                    Old_price = 100,
                    Status = 1,
                    Keywords = "new_keywords",
                    Description = "new_desc",
                    Hit = 1
                }
            ];
    }

    [Fact]
    public async Task Delete_Not_Existing_Product_Should_Not_Delete()
    {
        int notExistingId = 999999;

        var deleteResponse = await _apiClient.DeleteProductAsync(notExistingId);
        var delete = await deleteResponse.Content.ReadFromJsonAsync<EditDeleteProductResponse>();
        Assert.Equal(0, delete.Status);
    }
}