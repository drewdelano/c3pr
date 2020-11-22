using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SlackNet;
using System;
using C3PR.Core.Services;
using C3PR.Api.Models;

namespace C3PR.Api.Controllers
{
    [Route("Shipping")]
    public class ShippingCheckController : ControllerBase
    {
        readonly ISlackApiDaemonService _slackApiDaemonService;

        public ShippingCheckController(ISlackApiDaemonService slackApiDaemonService)
        {
            _slackApiDaemonService = slackApiDaemonService;
        }

        [HttpGet]
        [Route("SafeToDeployProd")]
        public async Task<IActionResult> SafeToDeployProd(string channelName)
        {
            if (string.IsNullOrWhiteSpace(channelName))
            {
                return BadRequest();
            }
            if (!await _slackApiDaemonService.IsChannelNameValid(channelName))
            {
                return Conflict();
            }


            if (await _slackApiDaemonService.IsSafeToShip(channelName))
            {
                return Ok();
            }
            else
            {
                return new IAmATeaPot();
            }
        }
    }

}
