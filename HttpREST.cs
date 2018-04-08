using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static OntologyXmlParser.GlobalVars;

namespace OntologyXmlParser
{
    public class RestAPI
    {
        private static readonly HttpClient client = new HttpClient();

        public static void ProcessPostingObjects(ObservationObject observation, AffectObject affect, BehaviourObject behaviour, EmotionObject emotion)
        {
            ResponseObject observationResponse = PostObservation(observation).Result;
            if(observationResponse != null){
                Console.WriteLine("POSTed observation {0}", observationResponse.Observation_ID);
                if(affect != null)
                {
                    affect.ObservationID = observationResponse.Observation_ID;
                    HttpResponseMessage postAffectResponse =  PostAffect(affect).Result;
                    if(!postAffectResponse.IsSuccessStatusCode)
                        Console.WriteLine("Failed to POST affect, with error: {0}", postAffectResponse.Content.ReadAsStringAsync().Result.ToString());
                    else Console.WriteLine("POSTed affect");
                }
                if(behaviour != null)
                {
                    behaviour.ObservationID = observationResponse.Observation_ID;
                    HttpResponseMessage postBehaviourResponse =  PostBehaviour(behaviour).Result;
                    if(!postBehaviourResponse.IsSuccessStatusCode)
                        Console.WriteLine("Failed to POST behaviour, with error: {0}", postBehaviourResponse.Content.ReadAsStringAsync().Result.ToString());
                    else Console.WriteLine("POSTed behaviour");
                }
                if(emotion != null)
                {
                    emotion.ObservationID = observationResponse.Observation_ID;
                    HttpResponseMessage postEmotionResponse =  PostEmotion(emotion).Result;
                    if(!postEmotionResponse.IsSuccessStatusCode)
                        Console.WriteLine("Failed to POST emotion, with error: {0}", postEmotionResponse.Content.ReadAsStringAsync().Result.ToString());
                    else Console.WriteLine("POSTed emotion");
                }
            }
        }
        public static async Task<ResponseObject> PostObservation(ObservationObject observation)
        {
            if (observation == null)
            {
                throw new ArgumentNullException(nameof(observation));
            }

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "OntologyXmlParser");
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            HttpContent content = new StringContent(JsonConvert.SerializeObject(observation), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync($"{url}/insert/observation/Digital", content);

            if (response.IsSuccessStatusCode) {
                return JsonConvert.DeserializeObject<ResponseObject>(response.Content.ReadAsStringAsync().Result);
            }else
            {
                Console.WriteLine("Failed to POST observation, with error: {0}", response.Content.ReadAsStringAsync().Result.ToString());
                return null;
            }
        }
        public static async Task<HttpResponseMessage> PostAffect(AffectObject affect)
        {
            if (affect == null)
            {
                throw new ArgumentNullException(nameof(affect));
            }

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "OntologyXmlParser");
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            string data = JsonConvert.SerializeObject(affect,
                                    Newtonsoft.Json.Formatting.None,
                                    new JsonSerializerSettings
                                    {
                                        NullValueHandling = NullValueHandling.Ignore,
                                        MissingMemberHandling = MissingMemberHandling.Ignore
                                    });
            HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync($"{url}/insert/affect", content);

            return response;
        }
        public static async Task<HttpResponseMessage> PostBehaviour(BehaviourObject behaviour)
        {
            if (behaviour == null)
            {
                throw new ArgumentNullException(nameof(behaviour));
            }

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "OntologyXmlParser");
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            string data = JsonConvert.SerializeObject(behaviour,
                                    Newtonsoft.Json.Formatting.None,
                                    new JsonSerializerSettings
                                    {
                                        NullValueHandling = NullValueHandling.Ignore,
                                        MissingMemberHandling = MissingMemberHandling.Ignore
                                    });
            HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync($"{url}/insert/behaviour", content);

            return response;
        }
        public static async Task<HttpResponseMessage> PostEmotion(EmotionObject emotion)
        {
            if (emotion == null)
            {
                throw new ArgumentNullException(nameof(emotion));
            }

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "OntologyXmlParser");
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            string data = JsonConvert.SerializeObject(emotion,
                                    Newtonsoft.Json.Formatting.None,
                                    new JsonSerializerSettings
                                    {
                                        NullValueHandling = NullValueHandling.Ignore,
                                        MissingMemberHandling = MissingMemberHandling.Ignore
                                    });
            HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync($"{url}/insert/emotion", content);

            return response;
        }
    }
}