using System.ComponentModel.DataAnnotations;

namespace BlazorSessionCore.Components
{
    internal sealed class SessionData
    {
        public SessionData()
        {
            SessionMessage = string.Empty;
        }

        public string SessionMessage { get; set; }
    }
}
