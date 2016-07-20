using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Paynter.Harvest.Models;
using Paynter.Harvest.Services;

namespace DotNetCoreChatBots
{
    public class HarvestDataHelper
    {
        private HarvestService _harvestService;
        public IList<HarvestProject> Projects { get; private set; }

        public HarvestDataHelper(HarvestService harvestService)
        {
            _harvestService = harvestService;
        }

        public async Task RefreshProjectsList()
        {
            var results = await _harvestService.Projects();
            Projects = results.ToList();
        }

        public IEnumerable<HarvestProject> QueryProjectsByName(string query)
        {
            return Projects.Where(u => u.Name.ToLower().Contains(query.ToLower()));
        }
    }
}