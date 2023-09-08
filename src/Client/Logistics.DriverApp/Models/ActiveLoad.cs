using Logistics.Models;

namespace Logistics.DriverApp.Models;

public class ActiveLoad
{
    public ActiveLoad(LoadDto loadDto, string embedMapHtml)
    {
        LoadData = loadDto;
        EmbedMapHtml = embedMapHtml;
    }
    
    public LoadDto LoadData { get; set; }
    public string EmbedMapHtml { get; set; }
}
