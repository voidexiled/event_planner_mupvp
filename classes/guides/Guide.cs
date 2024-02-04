public class Guide
{
    public int Id { get; set; }
    public string Image { get; set; }
    public string Video { get; set; }
    public string Title { get; set; }
    public List<string> Body { get; set; }
    public GuidesCategories Category { get; set; }
    public List<string> Tags { get; set; }

    public string Author { get; set; }
    public string AuthorId { get; set; }

    public bool IsFeatured { get; set; }
    public List<int> RelatedGuides { get; set; }
    public List<Commentary> Commentaries { get; set; }

    public string Status { get; set; }
    public int Likes { get; set; }
    public int Dislikes { get; set; }

    public string Date { get; set; }

    public Guide()
    {
        Commentaries = new();
        Tags = new();
        RelatedGuides = new();
    }
}