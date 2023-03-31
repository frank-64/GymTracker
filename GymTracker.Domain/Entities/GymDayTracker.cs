using Newtonsoft.Json;

namespace GymTracker.Domain.Entities
{
    public class GymDayTracker
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "month")]
        public string Month { get; set; }
        public bool IsOpen { get; set; } = false;
        public bool AdminClosedGym { get; set; } = false;
        public int CurrentGymOccupancy { get; set; }
        public int HighestGymOccupancy { get; set; }
        public int MaximumOccupancy { get; set; }
        public DateTimeOffset CurrentDate { get; set; }
        public string DayOfWeek => CurrentDate.DayOfWeek.ToString();
        public Day? CustomOpeningHours { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public void UpdateOpeningHours(Day? day)
        {
            AdminClosedGym = !day.IsOpen;
            CustomOpeningHours = day.IsOpen ? day : null;
        }
    }
}
