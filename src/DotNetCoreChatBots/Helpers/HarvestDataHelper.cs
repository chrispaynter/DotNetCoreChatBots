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

        public async Task<IEnumerable<HarvestProject>> QueryProjectsByName(string query)
        {
            await CheckDataLoaded();

            return Projects.Where(u => u.Name.ToLower().Contains(query.ToLower()));
        }

        public async Task<HarvestProject> GetProjectById(string id)
        {
            await CheckDataLoaded();

            return Projects.FirstOrDefault(u => u.Id == id);
        }

        public async Task CheckDataLoaded()
        {
            if(Projects == null || !Projects.Any())
            {
                await RefreshProjectsList();
            }
        }
    }
}