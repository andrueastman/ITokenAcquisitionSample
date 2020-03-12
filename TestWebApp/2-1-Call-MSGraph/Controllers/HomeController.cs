using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
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
            TokenAcquisitionAuthProvider tokenAcquisitionAuthProvider = new TokenAcquisitionAuthProvider(_tokenAcquisition, new[] { Constants.ScopeUserRead });
            
            HttpClient httpClient = GraphClientFactory.Create(tokenAcquisitionAuthProvider);
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me/");
            HttpResponseMessage meResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            string response = await meResponseMessage.Content.ReadAsStringAsync();
            var me = JsonSerializer.Deserialize(response, typeof(Microsoft.Graph.User));

            ViewData["Me"] = me;

            try
            {
                httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me/");
                meResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                // Get user photo
                using (var photoStream = await meResponseMessage.Content.ReadAsStreamAsync())
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