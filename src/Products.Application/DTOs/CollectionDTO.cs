namespace Products.Application.DTOs;

public class CollectionDTO<T>
{
    public required IEnumerable<T> Items { get; set; }
}
