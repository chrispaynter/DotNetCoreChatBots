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

        public IList<HarvestUser> People { get; private set; }
        public IList<HarvestTask> Tasks { get; set; }

        public HarvestDataHelper(HarvestService harvestService)
        {
            _harvestService = harvestService;
            RefreshProjectsList();
            RefreshPeopleList();
            RefreshTasks();
        }

        public async Task RefreshProjectsList()
        {
            var results = await _harvestService.Projects();
            Projects = results.ToList();
        }

        public async Task RefreshPeopleList()
        {
            var results = await _harvestService.People();
            People = results.ToList();
        }

        public async Task RefreshTasks()
        {
            var results = await _harvestService.Tasks();
            Tasks = results.ToList();
        }

        public IEnumerable<HarvestProject> QueryProjectsByName(string query)
        {
            return Projects.Where(u => u.Active && u.Name.ToLower().Contains(query.ToLower()));
        }

        public HarvestProject GetProjectById(string id)
        {
            return Projects.FirstOrDefault(u => u.Active && u.Id == id);
        }

        public HarvestUser GetUserByEmail(string email)
        {
            return People.FirstOrDefault(u => u.Email == email);
        }

        /// To save loading all tasks for all projects up front, we lazy load them
        /// This won't return anything but will add the tasks to the project object
        public async Task LazyLoadTasks(HarvestProject project)
        {
            if(project != null && project.Tasks == null || !project.Tasks.Any())
            {
                // Tasks haven't yet been loaded so grab the assignments
                project.Tasks = await _harvestService.TaskAssignments(project.Id);

                foreach(var taskAssignment in project.Tasks)
                {
                    // Then create a link to the actual task on the task assignment.
                    taskAssignment.Task = Tasks.FirstOrDefault(u => u.Id == taskAssignment.TaskId);
                }
            }
        }


        // public async Task CheckProjectDataLoaded()
        // {
        //     if(Projects == null || !Projects.Any())
        //     {
        //         await RefreshProjectsList();
        //     }
        // }

        // public async Task CheckPeopleDataLoaded()
        // {
        //     if(People == null || !People.Any())
        //     {
        //         await RefreshPeopleList();
        //     }
        // }

    }
}