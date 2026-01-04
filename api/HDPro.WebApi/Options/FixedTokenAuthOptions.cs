using System.Collections.Generic;

namespace HDPro.WebApi.Options
{
    public class FixedTokenAuthOptions
    {
        public bool Enabled { get; set; }

        public string HeaderName { get; set; } = "X-Fixed-Token";

        public string Token { get; set; }

        public IEnumerable<string> AllowedCidrs { get; set; }
    }
}
