using Newtonsoft.Json;

namespace PersonalProjectNotes.Services.Exceptions.Response
{
    public class ExceptionRespons
    {
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
