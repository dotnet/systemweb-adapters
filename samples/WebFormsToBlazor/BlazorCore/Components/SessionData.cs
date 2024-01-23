using System.ComponentModel.DataAnnotations;

namespace BlazorCore.Components
{
    internal sealed class SessionData
    {
        public SessionData()
        {
            SessionMessage = string.Empty;
        }

        [Required]
        public string SessionMessage { get; set; }
    }
}
