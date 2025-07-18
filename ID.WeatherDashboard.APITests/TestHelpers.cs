namespace ID.WeatherDashboard.APITests
{
    using System;
    using System.Reflection;
    using System.Text;

    public static class TestHelpers
    {
        private static readonly Random _random = new Random();

        public static readonly char[] UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        public static readonly char[] LowercaseLetters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        public static readonly char[] Digits = "0123456789".ToCharArray();
        public static readonly char[] Symbols = "!@#$%^&*()-_=+[]{}|;:',.<>?/`~".ToCharArray();

        public static string RandomString(int length, params char[][] possibleCharacters)
        {
            if (length < 1)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than 0.");

            if (possibleCharacters?.Any() != true)
                throw new ArgumentException("At least one possible character must be provided.", nameof(possibleCharacters));

            var chars = possibleCharacters.SelectMany(c => c).Distinct().ToArray();

            var result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                var ch = chars[_random.Next(chars.Length)];
                result.Append(ch);
            }
            return result.ToString();
        }

        public static T RandomEnumValue<T>() where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(_random.Next(values.Length))!;
        }

        public static double RandomDoubleBetween(double minValue, double maxValue)
        {
            return minValue + (Random.Shared.NextDouble() * (maxValue - minValue));
        }

        public static int RandomIntBetween(int minValue, int maxValue)
        {
            return (int)(minValue + (Random.Shared.NextDouble() * (maxValue - minValue)));
        }

        public static T InvokePrivateGenericMethod<T>(this object obj, string methodName, Type[] parameterTypes, params object[] parameters)
        {
            var method = obj.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(m =>
                    m.Name == methodName &&
                    m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes));

            if (method == null)
                throw new InvalidOperationException($"Method {methodName} with specified parameters not found.");

            return (T)method.Invoke(obj, parameters);
        }

    }
}
