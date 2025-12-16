namespace Sven.Tests.TestUtils
{
    internal class SvenUrl
    {
        private readonly string _url;

        public SvenUrl(string? url)
        {
            Assert.NotNull(url);
            Assert.NotEmpty(url);
            this._url = url;
            Parameters = MiscHelper.GetQueryParameters(url);
        }

        public string Url { get { return _url; } }
        public Dictionary<string, string?> Parameters { get; set; }

        public void CheckForParameter(string parameterName, string? expectedValue = null)
        {
            Assert.True(Parameters.ContainsKey(parameterName), $"Parameter '{parameterName}' not found in URL '{_url}'");

            if (expectedValue != null)
            {
                Assert.Equal(expectedValue, Parameters[parameterName]);
            }
        }

        public string? GetParameterValue(string parameterName)
        {
            Assert.True(Parameters.ContainsKey(parameterName), $"Parameter '{parameterName}' not found in URL '{_url}'");
            return Parameters[parameterName];
        }
    }
}
