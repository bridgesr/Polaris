using Newtonsoft.Json;

namespace PolarisAuthHandover.Factories
{
    public class CmsAuthValuesFactory : ICmsAuthValuesFactory
    {
        public string SerializeCmsAuthValues(string cookies)
        {
            return JsonConvert.SerializeObject(new TransportObject
            {
                Cookies = cookies
            });
        }

        public string SerializeCmsAuthValues(string cookies, string token)
        {
            return JsonConvert.SerializeObject(new TransportObject
            {
                Cookies = cookies,
                Token = token
            });
        }

        private class TransportObject
        {
            public string Cookies { get; set; }

            public string Token { get; set; }
        }
    }
}