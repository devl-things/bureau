using System.Text.Json.Serialization;

namespace Sven.Models
{
    public class IntrospectionResponse
    {
        private bool _active;
        private string? _sub;
        private string? _scope;
        private string? _clientId;
        private long _exp;
        private long _iat;
        private string? _jti;
        private string? _iss;

        [JsonPropertyName("active")]
        public bool Active
        {
            get { return _active; }
            set { _active = value; }
        }

        [JsonPropertyName("sub")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Sub
        {
            get { return _sub; }
            set { _sub = value; }
        }

        [JsonPropertyName("scope")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Scope
        {
            get { return _scope; }
            set { _scope = value; }
        }

        [JsonPropertyName("client_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ClientId
        {
            get { return _clientId; }
            set { _clientId = value; }
        }

        [JsonPropertyName("exp")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long Exp
        {
            get { return _exp; }
            set { _exp = value; }
        }

        [JsonPropertyName("iat")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long Iat
        {
            get { return _iat; }
            set { _iat = value; }
        }

        [JsonPropertyName("jti")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Jti
        {
            get { return _jti; }
            set { _jti = value; }
        }

        [JsonPropertyName("iss")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Iss
        {
            get { return _iss; }
            set { _iss = value; }
        }
    }
}
