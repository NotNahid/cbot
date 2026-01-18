using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FinalYearChatbot
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ChatForm());
        }
    }

    public class ChatbotLogic
    {
        private List<string> knowledgeBase;

        public ChatbotLogic()
        {
            knowledgeBase = new List<string>
            {
                "A chatbot is artificial intelligence software that can simulate a conversation.",
                "Chatbots are important because they automate interaction between humans and machines.",
                "Natural Language Processing (NLP) helps computers understand human language.",
                "C# is a powerful programming language created by Microsoft.",
                "Visual Studio is the best IDE for building Windows applications.",
                "The Final Year Project is a crucial part of the engineering curriculum."
            };
        }

        public string GetResponse(string userInput)
        {
            userInput = userInput.ToLower();
            string[] greetings = { "hello", "hi", "hey", "sup", "greetings" };
            if (greetings.Any(g => userInput.Contains(g)))
            {
                string[] replies = { "Hi there!", "Hello!", "Greetings!", "Ready to help." };
                return replies[new Random().Next(replies.Length)];
            }

            var userVector = Tokenize(userInput);
            double bestScore = 0;
            string bestResponse = "I am sorry, I don't understand that yet.";

            foreach (var sentence in knowledgeBase)
            {
                var sentenceVector = Tokenize(sentence.ToLower());
                double score = CalculateCosineSimilarity(userVector, sentenceVector);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestResponse = sentence;
                }
            }

            if (bestScore < 0.1) return "I am sorry, I don't understand that.";
            return bestResponse;
        }

        private Dictionary<string, int> Tokenize(string text)
        {
            var vector = new Dictionary<string, int>();
            var words = Regex.Replace(text, "[^a-z0-9 ]", "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                if (vector.ContainsKey(word)) vector[word]++;
                else vector[word] = 1;
            }
            return vector;
        }

        private double CalculateCosineSimilarity(Dictionary<string, int> vec1, Dictionary<string, int> vec2)
        {
            var intersection = vec1.Keys.Intersect(vec2.Keys);
            double dotProduct = intersection.Sum(k => vec1[k] * vec2[k]);
            double mag1 = Math.Sqrt(vec1.Values.Sum(v => v * v));
            double mag2 = Math.Sqrt(vec2.Values.Sum(v => v * v));
            if (mag1 == 0 || mag2 == 0) return 0;
            return dotProduct / (mag1 * mag2);
        }
    }

    public class ChatForm : Form
    {
        private TextBox inputJson;
        private Button sendButton;
        private RichTextBox chatHistory;
        private ChatbotLogic bot;

        public ChatForm()
        {
            bot = new ChatbotLogic();
            this.Text = "Final Year Project Chatbot";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            chatHistory = new RichTextBox();
            chatHistory.Location = new Point(12, 12);
            chatHistory.Size = new Size(460, 480);
            chatHistory.ReadOnly = true;
            chatHistory.Font = new Font("Segoe UI", 10);
            chatHistory.BackColor = Color.White;
            this.Controls.Add(chatHistory);

            inputJson = new TextBox();
            inputJson.Location = new Point(12, 510);
            inputJson.Size = new Size(360, 30);
            inputJson.Font = new Font("Segoe UI", 12);
            inputJson.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) SendMessage(); };
            this.Controls.Add(inputJson);

            sendButton = new Button();
            sendButton.Text = "Send";
            sendButton.Location = new Point(380, 508);
            sendButton.Size = new Size(90, 32);
            sendButton.Click += (s, e) => SendMessage();
            this.Controls.Add(sendButton);
        }

        private void SendMessage()
        {
            string msg = inputJson.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;
            AppendText("You: " + msg + "\n", Color.Blue);
            inputJson.Clear();
            string response = bot.GetResponse(msg);
            AppendText("Bot: " + response + "\n\n", Color.Black);
            chatHistory.SelectionStart = chatHistory.Text.Length;
            chatHistory.ScrollToCaret();
        }

        private void AppendText(string text, Color color)
        {
            chatHistory.SelectionStart = chatHistory.TextLength;
            chatHistory.SelectionLength = 0;
            chatHistory.SelectionColor = color;
            chatHistory.AppendText(text);
            chatHistory.SelectionColor = chatHistory.ForeColor;
        }
    }
}
