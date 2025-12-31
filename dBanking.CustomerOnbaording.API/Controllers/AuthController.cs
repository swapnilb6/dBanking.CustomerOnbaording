using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Text;

namespace dBanking.CustomerOnbaording.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("token")]
        public async Task<IActionResult> Token()
        {
            var authHeader = Request.Headers.Authorization;
            var authHeadInfo = authHeader.ToString().Split(' ');
            var bas64TokenInfo = Convert.FromBase64String(authHeadInfo[1]);
            var base64DecodeIdandSecrete = Encoding.UTF8.GetString(bas64TokenInfo);
            var app = ConfidentialClientApplicationBuilder
                .Create(base64DecodeIdandSecrete.Split(':')[0])
                .WithClientSecret(base64DecodeIdandSecrete.Split(':')[1])
                .WithAuthority($"{_configuration.GetValue<string>("AzureAd:Instance")}{_configuration.GetValue<string>("AzureAd:TenantId")}")
                .Build();

            var tokenResult = await app.AcquireTokenForClient(new List<string>
            { "api://38be1d86-8bdc-4bad-ad6c-c20ca69474f0/.default" }).ExecuteAsync();

            return Ok(new { access_token = tokenResult.AccessToken });
        }
    }
}
