using Newtonsoft.Json;

namespace CosmosGettingStartedTutorial.Entities;

public class Address
{
    public string State { get; set; }
    public string County { get; set; }
    public string City { get; set; }

    public override string ToString() => JsonConvert.SerializeObject(this);
}