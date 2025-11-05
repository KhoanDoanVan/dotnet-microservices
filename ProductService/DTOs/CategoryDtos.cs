namespace ProductService.DTOs;



public class CategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}


public class CreateCategoryRequest
{
    public string CategoryName { get; set; } = string.Empty;
}


public class UpdateCategoryRequest
{
    public string CategoryName { get; set; } = string.Empty;
}