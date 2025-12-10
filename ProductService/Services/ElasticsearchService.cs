using Elastic.Clients.Elasticsearch;
using ProductService.Models;
using ProductService.DTOs;

namespace ProductService.Services;


public interface IElasticsearchService
{
    Task IndexProductAsync(Product product);
    Task<List<ProductDto>> SearchProductsAsync(string query, int from = 0, int size = 20);
    Task<List<ProductDto>> SuggestProductsAsync(string query, int size = 10);
    Task DeleteProductAsync(int productId);
    Task BulkIndexProductsAsync(List<Product> products);
}



public class ElasticsearchService: IElasticsearchService
{

    private readonly ElasticsearchClient _client;
    private const string IndexName = "products";


    public ElasticsearchService(string elasticsearchUrl)
    {
        var settings = new ElasticsearchClientSettings(new Uri(elasticsearchUrl)).DefaultIndex(IndexName);

        _client = new ElasticsearchClient(settings);


        // Create index if not exists
        InitializeIndexAsync().Wait();
    }



    private async Task InitializeIndexAsync()
    {
        var existsResponse = await _client.Indices.ExistsAsync(IndexName);

        if (!existsResponse.Exists)
        {
            await _client.Indices.CreateAsync(IndexName, c => c.Mappings(m => m.Properties<ProductSearchDocument>(
                p => p
                    .IntegerNumber(n => n.ProductId)
                    .Text(t => t.ProductName, t => t.Analyzer("standard"))
                    .Keyword(k => k.Barcode)
                    .IntegerNumber(n => n.CategoryId)
                    .IntegerNumber(n => n.SupplierId)
                    .FloatNumber(f => f.Price)
                    .Keyword(k => k.Unit)
            )));
        }
    }



    public async Task IndexProductAsync(Product product)
    {
        var document = new ProductSearchDocument
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            Barcode = product.Barcode,
            CategoryId = product.CategoryId,
            SupplierId = product.SupplierId,
            Price = product.Price,
            Unit = product.Unit,
            CreatedAt = product.CreatedAt
        };

        await _client.IndexAsync(document, IndexName);
    }



    public async Task<List<ProductDto>> SearchProductsAsync(string query, int from = 0, int size = 20)
    {
        var searchResponse = await _client.SearchAsync<ProductSearchDocument>(s => s
            .Indices(IndexName)
            .From(from)
            .Size(size)
            .Query(q => q
                .Bool(b => b
                    .Should(
                        sh => sh.Match(m => m
                            .Field(f => f.ProductName)
                            .Query(query)
                            .Boost(2)
                        ),
                        sh => sh.Term(t => t
                            .Field(f => f.Barcode)
                            .Value(query)
                            .Boost(3)
                        ),
                        sh => sh.Wildcard(w => w
                            .Field(f => f.ProductName)
                            .Value($"*{query}*")
                        )
                    )
                )
            )
        );



        return searchResponse.Documents.Select(d => new ProductDto
        {
            ProductId = d.ProductId,
            ProductName = d.ProductName,
            Barcode = d.Barcode,
            CategoryId = d.CategoryId,
            SupplierId = d.SupplierId,
            Price = d.Price,
            Unit = d.Unit,
            CreatedAt = d.CreatedAt
        }).ToList();
    }


    public async Task<List<ProductDto>> SuggestProductsAsync(string query, int size = 10)
    {
        var searchResponse = await _client.SearchAsync<ProductSearchDocument>(s => s
            .Indices(IndexName)
            .Size(size)
            .Query(q => q
                .Prefix(p => p
                    .Field(f => f.ProductName)
                    .Value(query)
                )
            )
        );

        return searchResponse.Documents.Select(d => new ProductDto
        {
            ProductId = d.ProductId,
            ProductName = d.ProductName,
            Barcode = d.Barcode,
            CategoryId = d.CategoryId,
            SupplierId = d.SupplierId,
            Price = d.Price,
            Unit = d.Unit,
            CreatedAt = d.CreatedAt
        }        
        ).ToList();
    }



    public async Task DeleteProductAsync(int productId)
    {
        await _client.DeleteAsync<ProductSearchDocument>(productId, d => d.Index(IndexName));
    }


    public async Task BulkIndexProductsAsync(List<Product> products)
    {
        var documents = products.Select(p => new ProductSearchDocument
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            Barcode = p.Barcode,
            CategoryId = p.CategoryId,
            SupplierId = p.SupplierId,
            Price = p.Price,
            Unit = p.Unit,
            CreatedAt = p.CreatedAt
        }).ToList();


        await _client.BulkAsync(b => b
            .Index(IndexName)
            .IndexMany(documents)
        );
    }

}