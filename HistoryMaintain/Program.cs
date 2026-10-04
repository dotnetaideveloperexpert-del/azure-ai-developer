using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Chat;

var endpoint = "https://meridianopenai.openai.azure.com/";
var deploymentName = "gpt-4.1-mini";

var client = new AzureOpenAIClient(
    new Uri(endpoint),
    new DefaultAzureCredential());

var chatClient = client.GetChatClient(deploymentName);

// ─────────────────────────────────────────────
// CHAT HISTORY — poori conversation yahan store hoti hai
// ─────────────────────────────────────────────
var messages = new List<ChatMessage>
{
    new SystemChatMessage("You are a helpful assistant. Answer briefly."),
    new UserChatMessage("What is the capital of France?"),
};

// ─────────────────────────────────────────────
// PEHLA SAWAL
// ─────────────────────────────────────────────
var response1 = await chatClient.CompleteChatAsync(messages);
var answer1 = response1.Value.Content[0].Text;

Console.WriteLine("Q1: What is the capital of France?");
Console.WriteLine("A1: " + answer1);
Console.WriteLine();

// History mein assistant ka jawab bhi daalo
messages.Add(new AssistantChatMessage(answer1));

// ─────────────────────────────────────────────
// DOOSRA SAWAL — "its" ka matlab AI ko pata hai
// ─────────────────────────────────────────────
messages.Add(new UserChatMessage("What is its population?"));

var response2 = await chatClient.CompleteChatAsync(messages);
var answer2 = response2.Value.Content[0].Text;

Console.WriteLine("Q2: What is its population?");
Console.WriteLine("A2: " + answer2);