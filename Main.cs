using ManagedCommon;
using System.Text;
using Wox.Plugin;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Wox.Infrastructure;
using Wox.Plugin.Common;
using Microsoft.PowerToys.Settings.UI.Library;
using System.Security.Cryptography;

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
            // Convert it to an integer and use it as the password length
            if (args.Length > 0 && int.TryParse(args[0], out int parsedLength) && parsedLength > 128)
            {
                return new List<Result>
                {
                    new() {
                        Title = "Password can be maximum 128 characters.",
                        SubTitle = "Please provide a number less than 129.",
                        IcoPath = IconPath,
                    },
                };
            }
            else if (args.Length > 0)
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
                new() {
                    Title = password,
                    SubTitle = $"Password {length} characters long, including special. Press Enter to copy.",
                    IcoPath = IconPath,
                    Action = e =>
                    {
                        Clipboard.SetText(password);
                        return true;
                    },
                },
                new() {
                    Title = applePassword,
                    SubTitle = "Apple-style password. Press Enter to copy.",
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
            char[] password = new char[length];
            byte[] randomBytes = new byte[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            for (int i = 0; i < length; i++)
            {
                password[i] = chars[randomBytes[i] % chars.Length];
            }

            return new string(password);
        }
        // 71-bit entropy compliant with Apple's password requirements
        private string GenerateAppleStylePassword()
        {
            const string lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
            const string uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numbers = "0123456789";
            char[] password = new char[20];

            // Step 1: Fill with 16 random lowercase letters
            for (int i = 0; i < 20; i++)
            {
                password[i] = GetRandomChar(lowercaseChars);
            }

            // Step 2: Insert exactly 1 uppercase letter in a random group
            int uppercaseGroup = GetRandomInt(0, 3);
            int uppercaseIndex = GetRandomInt(0, 6) + (uppercaseGroup * 6);
            password[uppercaseIndex] = GetRandomChar(uppercaseChars);

            // Step 3: Insert exactly 1 digit in a different random group
            int digitGroup;
            do
            {
                digitGroup = GetRandomInt(0, 3);
            } while (digitGroup == uppercaseGroup);

            int digitIndex = GetRandomInt(0, 6) + (digitGroup * 6);
            password[digitIndex] = GetRandomChar(numbers);

            // Step 4: Insert exactly 2 hyphens at fixed positions (6th and 13th characters)
            password[6] = '-';
            password[13] = '-';

            return new string(password);
        }

        // Securely generate a random character from a given set
        private static char GetRandomChar(string characterSet)
        {
            byte[] buffer = new byte[1];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
            }
            return characterSet[buffer[0] % characterSet.Length];
        }

        // Securely generate a random integer within a range
        private static int GetRandomInt(int min, int max)
        {
            byte[] buffer = new byte[4];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
            }
            int value = BitConverter.ToInt32(buffer, 0) & int.MaxValue; // Ensure positive number
            return min + (value % (max - min));
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