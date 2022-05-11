using Newtonsoft.Json;

namespace CosmosGettingStartedTutorial.Entities;

public class Pet
{
    public string GivenName { get; set; }

    public override string ToString() => JsonConvert.SerializeObject(this);
}