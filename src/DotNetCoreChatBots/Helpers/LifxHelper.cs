using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DotNetCoreChatBots.Helpers
{
    public class LifxHelper
    {
        private LifxOptions _options;
        public LifxHelper(IOptions<LifxOptions> options)
        {
            _options = options.Value;
        }

        private HttpClient _httpClient;

        public HttpClient HttpClient 
        { 
            get
            {
                if(_httpClient == null)
                {
                    _httpClient = new HttpClient();
                    _httpClient.BaseAddress = new Uri($"https://api.lifx.com/v1/");
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.Token}");                
                }

                return _httpClient;
            }
        }

        public async Task<bool> LightIsOn(string light)
        {
            var lights = await GetLights($"label:{light}");
            var first = lights.FirstOrDefault();
            return first != null && first.Power == "on";
        }

        public async Task<bool> LightIsOff(string light)
        {
            var lights = await GetLights($"label:{light}");
            var first = lights.FirstOrDefault();
            return first != null && first.Power == "off";
        }

        public void TurnLightOn(string light)
        {
            ChangeLightState(new StateParams()
            {
                Selector = $"label:{light}",
                Power = "on"
            });
        }

        public void TurnLightOnWithColor(string light, string color)
        {
            ChangeLightState(new StateParams()
            {
                Selector = $"label:{light}",
                Power = "on",
                Color = color
            });
        }

        public void TurnLightOff(string light)
        {
            ChangeLightState(new StateParams()
            {
                Selector = $"label:{light}",
                Power = "off"
            });
        }

        public void ChangeLightColor(string light, string color)
        {
            ChangeLightState(new StateParams()
            {
                Selector = $"label:{light}",
                Color = color
            });
        }

        public async Task ChangeLightState(StateParams stateParams)
        {
            var statesParams = new StatesParams()
            {
                States = new System.Collections.Generic.List<StateParams>() { stateParams }
            };

            var httpContent = new StringContent(JsonSerializerHelper.Serialize(statesParams), Encoding.UTF8, "application/json");
            var response = await HttpClient.PutAsync(LifxApiServiceUrls.States, httpContent);

            var content = await response.Content.ReadAsStringAsync();
        }

        public async Task<IEnumerable<LifxLightsResponse>> GetLights(string label)
        {
            var response = await HttpClient.GetAsync(LifxApiServiceUrls.Lights(label));
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<LifxLightsResponse>>(content);
        }
    }

    public class LifxOptions
    {
        public string Token { get; set; }
    }
    public class LifxLightsResponse
    {
        public string Power { get; set; }
    }

    public class LifxApiServiceUrls
    {
        public static string Base = "https://api.lifx.com/v1/{0}";

        public static string States
        {
            get
            {
                return string.Format(Base, "lights/states");
            }
        }

        public static string Lights(string selector)
        {
            return string.Format(Base, $"lights/{selector}");
        }

        public static string State(string selector)
        {
            return string.Format(Base, $"lights/{selector}/state");
        }
    }
    public class StateParams
    {
        public string Selector { get; set; }
        public string Power { get; set; }
        public string Color { get; set; }
        public string Brightness { get; set; }
        public string Duration { get; set; }
    }

    public class StatesParams
    {
        public List<StateParams> States { get; set; }
        public StateParams Defaults { get; set; }
    }
    
    public static class JsonSerializerHelper
    {
        /// <summary>
        /// Serialze to lower case json
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="referenceLoopHandling">http://stackoverflow.com/questions/7397207/json-net-error-self-referencing-loop-detected-for-type</param>
        /// <returns></returns>
        public static string Serialize(object obj, ReferenceLoopHandling referenceLoopHandling = ReferenceLoopHandling.Error)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    ReferenceLoopHandling = referenceLoopHandling
                });
        }
    }
}