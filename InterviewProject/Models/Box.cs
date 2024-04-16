namespace InterviewProject.Models;

public class Box
{
    private readonly IList<Content> _content;

    public Box()
    {
        _content = new List<Content>();
    }

    public void AddContent(Content content)
    {
        _content.Add(content);
    }

    public string SupplierIdentifier { get; set; }

    public string Identifier { get; set; }

    public IReadOnlyCollection<Content> Contents => _content.AsReadOnly();
}