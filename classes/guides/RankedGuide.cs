public class RankedGuide
{
    public Guide Guide { get; set; }
    public int Points { get; set; }


    public RankedGuide(Guide guide, int points)
    {
        Guide = guide;
        Points = points;
    }
}