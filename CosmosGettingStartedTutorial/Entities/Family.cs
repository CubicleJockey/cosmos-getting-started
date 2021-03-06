using Newtonsoft.Json;

namespace CosmosGettingStartedTutorial.Entities
{
    public class Family
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string LastName { get; set; }
        public Parent[] Parents { get; set; }
        public Child[] Children { get; set; }
        public Address Address { get; set; }
        public bool IsRegistered { get; set; }

        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}
