using ManagedCommon;
using System.Text;
using Wox.Plugin;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Wox.Infrastructure;
using Wox.Plugin.Common;
using Microsoft.PowerToys.Settings.UI.Library;
using System.Security.Cryptography;
using System.IO;

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

            int length = 12; // Default password length

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
        private static string GenerateAppleStylePassword()
        {
            // Vowel + consonant pools to favor readability
            const string vowels = "aeiouy";
            const string consonants = "bcdfghjkmnpqrstvwxz";

            // Uppercase and digits
            const string uppercaseChars = "ABCDEFGHJKLMNOPQRSTUVWXZ";
            const string numbers = "0123456789";

            // We'll create 18 total letters (3 blocks * 6 chars) 
            // then insert 2 hyphens to get 20 chars.
            char[] passwordLetters = new char[18];

            // 1) Generate 3 "pronounceable" blocks of 6 characters each
            for (int block = 0; block < 3; block++)
            {
                // In each 6-char block, we can ensure at least 2 vowels 
                // and 4 consonants
                List<char> blockChars = new List<char>(6);

                // Add 2 vowels
                for (int i = 0; i < 2; i++)
                {
                    blockChars.Add(GetRandomChar(vowels));
                }
                // Add 4 consonants
                for (int i = 0; i < 4; i++)
                {
                    blockChars.Add(GetRandomChar(consonants));
                }

                // Shuffle them so they're not always in vowel->consonant order until there are no 3 consecutive consonants
                do
                {
                    Shuffle(blockChars);
                }
                while (HasConsecutiveConsonants(string.Concat(blockChars), consonants));

                // Copy this block into the main array
                int startIndex = block * 6;
                for (int i = 0; i < 6; i++)
                {
                    passwordLetters[startIndex + i] = blockChars[i];
                }
            }

            // 2) Insert exactly 1 uppercase letter in a random group
            int uppercaseGroup = GetRandomInt(0, 3); // 0..2
            int uppercaseIndex = uppercaseGroup * 6 + GetRandomInt(0, 6);
            passwordLetters[uppercaseIndex] = GetRandomChar(uppercaseChars);

            // 3) Insert exactly 1 digit in a different random group
            int digitGroup;
            do
            {
                digitGroup = GetRandomInt(0, 3); // 0..2
            }
            while (digitGroup == uppercaseGroup);

            int digitIndex = digitGroup * 6 + GetRandomInt(0, 6);
            passwordLetters[digitIndex] = GetRandomChar(numbers);

            // 4) Insert two hyphens in the final string (positions 6 and 13)
            // So we construct the final 20-char array:
            char[] finalPassword = new char[20];
            // Copy first 6 chars
            Array.Copy(passwordLetters, 0, finalPassword, 0, 6);
            finalPassword[6] = '-';
            // Copy next 6 chars
            Array.Copy(passwordLetters, 6, finalPassword, 7, 6);
            finalPassword[13] = '-';
            // Copy last 6 chars
            Array.Copy(passwordLetters, 12, finalPassword, 14, 6);

            return new string(finalPassword);
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

        // Check if there are 3 consecutive consonants in the given string
        private static bool HasConsecutiveConsonants(string str, string consonants)
        {
            bool result = Regex.IsMatch(str, $"[{consonants}]{{3}}");
            return result;
        }

        // Fisher-Yates shuffle for a List<T>
        private static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = GetRandomInt(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
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