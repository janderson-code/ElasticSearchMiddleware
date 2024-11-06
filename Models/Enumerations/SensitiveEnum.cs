using System;
using System.Text.RegularExpressions;
using elasticsearch.Utils;

namespace elasticsearch.Models.Enumerations
{
    public class SensitiveEnum : Enumeration<SensitiveEnum>
    {
        public static readonly SensitiveEnum Document = new SensitiveEnum(1, "Document",
            value => DocumentFormat.FormatDoc(value));

        public static readonly SensitiveEnum CreditCard = new SensitiveEnum(2, "CreditCard", value =>
            Regex.Replace(value, @"^(\d{6}).(\d{4})$", "$1***$2"));

        public static readonly SensitiveEnum DefaultInitial = new SensitiveEnum(3, "DefaultInitial", value =>
            Regex.Replace(value, @"(\d{3}).", "*****$1"));
        
        public static readonly SensitiveEnum DefaultLast = new SensitiveEnum(4, "DefaultLast", value =>
            Regex.Replace(value, @".(\d{3})$", "*****$1"));

        private Func<string, string> MaskFunction { get; }

        private SensitiveEnum(int value, string name, Func<string, string> maskFunction)
            : base(value, name)
        {
            MaskFunction = maskFunction;
        }

        public string Mask(string data) => MaskFunction(data);
    }
}