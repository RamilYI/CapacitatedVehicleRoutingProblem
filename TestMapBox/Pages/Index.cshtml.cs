using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace TestMapBox.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        public string MapboxAccessToken { get; }

        public IndexModel(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            MapboxAccessToken = configuration["Mapbox:AccessToken"];
        }
    }
}
