using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NeoModules.Rest.Interfaces;

namespace NeoModulesCore.Controllers
{
    public class NeoNetworkController : Controller
    {

        private readonly IHappyNodesService _happyNodesService;


        public NeoNetworkController(IHappyNodesService happyNodesService)
        {
            _happyNodesService = happyNodesService;
        }

        public async Task<IActionResult> Index()
        {

            var bestBlock = await _happyNodesService.GetBestBlock();
            var blockTime = await _happyNodesService.GetBlockTime();
            var unconfirmedTxs = await _happyNodesService.GetUnconfirmed();
            var nodesFlat = await _happyNodesService.GetNodesFlat();
            ViewData["LastBlock"] = bestBlock;
            ViewData["BlockTime"] = blockTime;
            ViewData["UnconfirmedTxs"] = unconfirmedTxs.UnconfirmedTransactions;
            ViewData["Nodes"] = nodesFlat;


            // Other ways to look at nodes
            //var nodes = await _happyNodesService.GetNodes();
            //var nodeById = await _happyNodesService.GetNodeById(482);
            //var edges = await _happyNodesService.GetEdges();
            //var nodesList = await _happyNodesService.GetNodesList();
            //var endpoints = await _happyNodesService.GetEndPoints();

            // todo charts
            //var dailyHistory = await _happyNodesService.GetDailyNodeHistory();
            //var weeklyHistory = await _happyNodesService.GetWeeklyNodeHistory();
            //var dailyStability = await _happyNodesService.GetDailyNodeStability(480);
            //var weeklyStability = await _happyNodesService.GetWeeklyNodeStability(0);
            //var dailyLatency = await _happyNodesService.GetDailyNodeLatency(480);
            //var weeklyLatency = await _happyNodesService.GetWeeklyNodeLatency(480);
            //var blockHeightLag = await _happyNodesService.GetNodeBlockheightLag(0);

            return View();
        }
    }
}