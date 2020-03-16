using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using WebApp_OpenIDConnect_DotNet.Models;
using Constants = WebApp_OpenIDConnect_DotNet.Infrastructure.Constants;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        readonly ITokenAcquisition _tokenAcquisition;

        public HomeController(ITokenAcquisition tokenAcquisition)
        {
            this._tokenAcquisition = tokenAcquisition;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AuthorizeForScopes(Scopes = new[] { Constants.ScopeUserRead })]
        public async Task<IActionResult> Profile()
        {
            // Initialize the GraphServiceClient. 
            TokenAcquisitionAuthProvider tokenAcquisitionAuthProvider = new TokenAcquisitionAuthProvider(_tokenAcquisition);
            BaseClient baseClient = new BaseClient("https://graph.microsoft.com/v1.0", tokenAcquisitionAuthProvider);


            BaseRequest baseRequest = new BaseRequest("https://graph.microsoft.com/v1.0/me/", baseClient).WithScopes(new[] { Constants.ScopeUserRead });

            ViewData["Me"] = await baseRequest.SendAsync<User>(null,CancellationToken.None);

            try
            {
                baseRequest = new BaseRequest("https://graph.microsoft.com/v1.0/me/photo/$value", baseClient).WithScopes(new[] { Constants.ScopeUserRead });
                // Get user photo
                using (var photoStream = await baseRequest.SendStreamRequestAsync(null,CancellationToken.None))
                {
                    byte[] photoByte = ((MemoryStream)photoStream).ToArray();
                    ViewData["Photo"] = Convert.ToBase64String(photoByte);
                }
            }
            catch (System.Exception)
            {
                ViewData["Photo"] = null;
            }

            return View();
        }
        
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}