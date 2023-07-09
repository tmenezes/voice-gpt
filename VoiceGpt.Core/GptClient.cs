using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json.Linq;

namespace VoiceGpt.Core
{
    public class GptClient
    {
        private readonly HttpClient    _client = new();
        private readonly StringBuilder _conversation;

        public string Conversation => _conversation.ToString().Substring(Constants.BasePrompt.Length);

        public GptClient(string apiKey)
        {
            var openAiUrl = $"{Constants.OpenAiBaseUrl}/v1/engines/{Constants.Gpt3ModelName}/completions";
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _client.BaseAddress = new Uri(openAiUrl);


            _conversation = new StringBuilder(Constants.BasePrompt);
        }

        public async Task<string> CallGpt(string userInput)
        {
            _conversation.Append(userInput).Append("\n");

            var request = new HttpRequestMessage(HttpMethod.Post, "");
            request.Content = JsonContent.Create(new
            {
                prompt = _conversation.ToString(),
                temperature = 0.9,
                max_tokens = 150,
                top_p = 1,
                frequency_penalty = 0,
                presence_penalty = 0.6
            });
            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            // Get the response from the API
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JObject.Parse(responseContent);
            var gptResponse = responseJson["choices"][0]["text"].ToString();
            
            // update conversation state
            _conversation.Append(gptResponse).Append("\n\n");

            return gptResponse;
        }
    }
}
