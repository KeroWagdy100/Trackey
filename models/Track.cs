namespace Trackey;

class Track
{
    public Guid   Id            {get; set;} = Guid.NewGuid();
    public string Title         {get; set;} = "N/A";
    public string Artist        {get; set;} = "N/A";
    public string FileLocation  {get; set;} = "N/A";
    public string SourceUrl     {get; set;} = "N/A";
}
