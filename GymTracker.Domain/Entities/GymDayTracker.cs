using Newtonsoft.Json;

namespace GymTracker.Domain.Entities
{
    public class GymDayTracker
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }
        public int CurrentGymOccupancy { get; set; }
        public int HighestGymOccupancy { get; set; }
        public DateTimeOffset CurrentDate { get; set; }
        public string DayOfWeek => CurrentDate.DayOfWeek.ToString();
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
