using Newtonsoft.Json;

namespace CosmosGettingStartedTutorial.Entities;

public class Parent
{
    public string FamilyName { get; set; }
    public string FirstName { get; set; }

    public override string ToString() => JsonConvert.SerializeObject(this);
}