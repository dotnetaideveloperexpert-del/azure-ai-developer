using System.Text.Json;
using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Chat;


var endpoint = "https://meridianopenai.openai.azure.com/";
var deploymentName = "gpt-4.1-mini";
var historyFile = "chat-history.json";

var client = new AzureOpenAIClient(
    new Uri(endpoint),
    new DefaultAzureCredential());

var chatClient = client.GetChatClient(deploymentName);

// ─────────────────────────────────────────────
// HISTORY LOAD KARO — agar file hai to
// ─────────────────────────────────────────────
List<ChatMessage> messages;

if (File.Exists(historyFile))
{
    var json = File.ReadAllText(historyFile);
    var saved = JsonSerializer.Deserialize<List<SavedMessage>>(json) ?? new();

    messages = saved.Select(m => m.Role switch
    {
        "system" => (ChatMessage)new SystemChatMessage(m.Content),
        "user" => new UserChatMessage(m.Content),
        "assistant" => new AssistantChatMessage(m.Content),
        _ => new UserChatMessage(m.Content)
    }).ToList();

    Console.WriteLine($"[Loaded {messages.Count} messages from previous session]");
}
else
{
    messages = new List<ChatMessage>
    {
        new SystemChatMessage("You are a helpful .NET mentor. Answer briefly and clearly.")
    };
}

Console.WriteLine("Meridian AI Assistant");
Console.WriteLine("Type 'exit' to quit, 'clear' to start fresh.");
Console.WriteLine();

while (true)
{
    Console.Write("You: ");
    var userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput)) continue;

    if (userInput.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        SaveHistory(messages, historyFile);
        Console.WriteLine("Goodbye! Chat saved.");
        break;
    }

    if (userInput.Trim().Equals("clear", StringComparison.OrdinalIgnoreCase))
    {
        messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful .NET mentor. Answer briefly and clearly.")
        };
        SaveHistory(messages, historyFile);
        Console.WriteLine("[History cleared]");
        continue;
    }

    messages.Add(new UserChatMessage(userInput));

    Console.Write("AI: ");
    var fullAnswer = "";

    await foreach (var update in chatClient.CompleteChatStreamingAsync(messages))
    {
        foreach (var part in update.ContentUpdate)
        {
            Console.Write(part.Text);
            Console.Out.Flush();
            fullAnswer += part.Text;
        }
    }

    Console.WriteLine();
    Console.WriteLine();

    messages.Add(new AssistantChatMessage(fullAnswer));

    // Har jawab ke baad save karo
    SaveHistory(messages, historyFile);
}

// ─────────────────────────────────────────────
// HELPER FUNCTIONS
// ─────────────────────────────────────────────
static void SaveHistory(List<ChatMessage> messages, string file)
{
    var toSave = new List<SavedMessage>();

    foreach (var m in messages)
    {
        string role;
        string content;

        switch (m)
        {
            case SystemChatMessage s:
                role = "system";
                content = s.Content.FirstOrDefault()?.Text ?? "";
                break;

            case UserChatMessage u:
                role = "user";
                content = u.Content.FirstOrDefault()?.Text ?? "";
                break;

            case AssistantChatMessage a:
                role = "assistant";
                content = a.Content.FirstOrDefault()?.Text ?? "";
                break;

            default:
                role = "user";
                content = "";
                break;
        }

        toSave.Add(new SavedMessage(role, content));
    }

    var json = JsonSerializer.Serialize(toSave, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(file, json);
}
// Message ko save karne ke liye simple type
public record SavedMessage(string Role, string Content);