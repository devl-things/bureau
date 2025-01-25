using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.UI.API.Services
{
    public class BureauLinkGenerator
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;

        public BureauLinkGenerator(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
        {
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
        }

        public string GetLink(string routeName, RouteValueDictionary values)
        {
            if(_httpContextAccessor != null && _httpContextAccessor.HttpContext != null)
            {
                return _linkGenerator.GetUriByAddress(
                    _httpContextAccessor.HttpContext,
                    routeName,
                    values
                ) ?? string.Empty;
            }
            return string.Empty;
        }
    }
}
