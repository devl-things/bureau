using Sven.Configurations;
using System.Text.Json.Serialization;

namespace Sven
{
    public class Jwks
    {
        [JsonPropertyName("keys")]
        public Jwk[] Keys { get; set; }

        public Jwks(Jwk jwk)
        {
            Keys = new Jwk[] { jwk };
        }
    }

    public class Jwk
    {
        [JsonPropertyName("kty")]
        public string Kty { get; set; } = AuthConstants.OAuth.SigningAlgorithms.RSA;
        [JsonPropertyName("use")]
        public string Use { get; set; } = "sig";
        [JsonPropertyName("kid")]
        public string Kid { get; set; }
        [JsonPropertyName("alg")]
        public string Alg { get; set; } = AuthConstants.OAuth.SigningAlgorithms.Rsa256;
        [JsonPropertyName("n")]
        public string N { get; set; }
        [JsonPropertyName("e")]
        public string E { get; set; }

        public Jwk(string kid, string n, string e)
        {
            Kid = kid;
            N = n;
            E = e;
        }
    }
}
