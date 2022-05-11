using Newtonsoft.Json;

namespace CosmosGettingStartedTutorial.Entities;

public class Child
{
    public string FamilyName { get; set; }
    public string FirstName { get; set; }
    public string Gender { get; set; }
    public int Grade { get; set; }
    public Pet[] Pets { get; set; }

    public override string ToString() => JsonConvert.SerializeObject(this);
}
