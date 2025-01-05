using ManagedCommon;
using System.Text;
using Wox.Plugin;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Wox.Infrastructure;
using Wox.Plugin.Common;
using Microsoft.PowerToys.Settings.UI.Library;

namespace Community.PowerToys.Run.Plugin.PasswordGenerator
{
    public class Main : IPlugin
{
        public static string PluginID => "3f9c27f4-4b8f-40d4-9f64-47f5f5b74d6b";

        private string IconPath { get; set; }

        private PluginInitContext Context { get; set; }
        public string Name => "Password";

        public string Description => "Password Generator";

        public List<Result> Query(Query query)
        {
            string[] args = query.Search.Trim().Split(' ');

            int length = 16; // Default password length

            // If the user provides an argument, check if it's a number
            if (args.Length > 0)
            {
                if (int.TryParse(args[0], out int userLength))
                {
                    length = userLength;
                }
            }
            var password = GeneratePassword(length);
            var applePassword = GenerateAppleStylePassword();

            return new List<Result>
            {
                new Result
                {
                    Title = password,
                     SubTitle = $"Standard password ({length} characters). Press Enter to copy.",
                    IcoPath = IconPath,
                    Action = e =>
                    {
                        Clipboard.SetText(password);
                        return true;
                    },
                },
                new Result
                {
                    Title = applePassword,
                     SubTitle = "Apple-style password (xxxxx-yyyyy-zzzzz). Press Enter to copy.",
                    IcoPath = IconPath,
                    Action = e =>
                    {
                        Clipboard.SetText(applePassword);
                        return true;
                    },
                },
            };
        }

        private static string GeneratePassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());
        }

        private static string GenerateAppleStylePassword()
        {
            const string lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
            const string uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numbers = "0123456789";
            Random random = new Random();

            // Generate three 6-character segments
            string part1 = new string(Enumerable.Repeat(lowercaseChars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            string part2 = new string(Enumerable.Repeat(lowercaseChars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            string part3 = new string(Enumerable.Repeat(lowercaseChars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            // Pick a random part to insert a digit (Apple passwords always contain exactly one number)
            int partToModify = random.Next(3); // 0, 1, or 2
            int insertPosition = random.Next(6); // Random position within the part
            char randomDigit = numbers[random.Next(numbers.Length)];

            if (partToModify == 0)
                part1 = part1.Substring(0, insertPosition) + randomDigit + part1.Substring(insertPosition + 1);
            else if (partToModify == 1)
                part2 = part2.Substring(0, insertPosition) + randomDigit + part2.Substring(insertPosition + 1);
            else
                part3 = part3.Substring(0, insertPosition) + randomDigit + part3.Substring(insertPosition + 1);

            // Introduce rare uppercase letters (~15-20% chance per character)
            part1 = RarelyCapitalize(part1, random);
            part2 = RarelyCapitalize(part2, random);
            part3 = RarelyCapitalize(part3, random);

            return $"{part1}-{part2}-{part3}";
        }

        // Function to introduce rare capitalization (~15-20% chance per character)
        private static string RarelyCapitalize(string input, Random random)
        {
            char[] chars = input.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (random.NextDouble() < 0.15) // 15% chance of making a letter uppercase
                {
                    chars[i] = char.ToUpper(chars[i]);
                }
            }
            return new string(chars);
        }

        public void Init(PluginInitContext context)
        {
            Context = context;
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());
        }

        private void UpdateIconPath(Theme theme)
        {
            if (theme == Theme.Light || theme == Theme.HighContrastWhite)
            {
                IconPath = "icon.png";
            }
            else
            {
                IconPath = "icon.png";
            }
        }

        private void OnThemeChanged(Theme currentTheme, Theme newTheme)
        {
            UpdateIconPath(newTheme);
        }
    }
}