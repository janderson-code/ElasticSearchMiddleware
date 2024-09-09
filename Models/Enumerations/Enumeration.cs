using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace elasticsearch.Models.Enumerations
{
    /// <summary>
    /// Representa uma enumeração como um objeto com um nome e um identificador númerico único.
    /// </summary>
    /// <typeparam name="TEnum">Tipo da classe de enumeração a ser definida.</typeparam>
    public abstract class Enumeration<TEnum> : IEquatable<Enumeration<TEnum>>, IComparable<Enumeration<TEnum>>
        where TEnum : Enumeration<TEnum>
    {
        private static readonly Lazy<Dictionary<int, TEnum>> EnumerationsDictionary =
            new(() => GetAllEnumerationOptions(typeof(TEnum))
                .ToDictionary(enumeration => enumeration.Value));

        protected Enumeration(int value, string name) =>
            (Value, Name) = (value, name);

        public static IReadOnlyCollection<TEnum> List => EnumerationsDictionary.Value.Values.ToList();

        public int Value { get; }

        public string Name { get; }

        public static bool operator ==(Enumeration<TEnum>? a, Enumeration<TEnum>? b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;

            return a.Equals(b);
        }

        public static bool operator !=(Enumeration<TEnum> a, Enumeration<TEnum> b) => !(a == b);

        public static bool TryParse(int value, out TEnum? @enum)
        {
            @enum = FromValue(value);
            return @enum is not null;
        }

        public static bool TryParse(string name, out TEnum? @enum)
        {
            @enum = FromName(name);
            return @enum is not null;
        }

        /// <summary>
        /// Cria uma enumeração do tipo especificado com base no valor (identificador) especificado.
        /// </summary>
        /// <param name="value">O identificador da enumeração.</param>
        /// <returns>Retorna a instância da enumeração, caso ela exista.</returns>
        public static TEnum? FromValue(int value) =>
            EnumerationsDictionary
                .Value
                .TryGetValue(value, out var enumeration)
                ? enumeration
                : null;

        /// <summary>
        /// Cria uma enumeração do tipo especificado com base no nome especificado.
        /// </summary>
        /// <param name="name">Nome da enumeração.</param>
        /// <param name="ignoreCase">Caso seja necessário ignorar case na comparação</param>
        /// <returns>Instância da enumeração que possue o nome especificado.</returns>
        public static TEnum? FromName(string name, bool ignoreCase = true) =>
            EnumerationsDictionary.Value.Values.SingleOrDefault(x =>
                x.Name.Equals(name, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));

        /// <summary>
        /// Verifica se uma enumeração com o valor especificado existe.
        /// </summary>
        /// <param name="value">Identificador da enumeração.</param>
        /// <returns>Verdadeiro se a enumeração com o identificador especificado existe, caso contrário falso.</returns>
        public static bool ContainsValue(int value) => EnumerationsDictionary.Value.ContainsKey(value);

        /// <inheritdoc />
        public bool Equals(Enumeration<TEnum>? other)
        {
            if (other is null)
            {
                return false;
            }

            return GetType() == other.GetType() && other.Value.Equals(Value);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            var isSameInstanceType = obj?.GetType() == GetType();
            if (obj is null || isSameInstanceType) return false;

            return obj is Enumeration<TEnum> otherValue &&
                   otherValue.Value
                       .Equals(Value);
        }

        /// <inheritdoc />
        public int CompareTo(Enumeration<TEnum>? other) => other is null ? 1 : Value.CompareTo(other.Value);

        /// <inheritdoc />
        public override int GetHashCode() => Value.GetHashCode();

        private static IEnumerable<TEnum> GetAllEnumerationOptions(Type enumType) =>
            Assembly
                .GetAssembly(enumType)?
                .GetTypes()
                .Where(enumType.IsAssignableFrom)
                .SelectMany(GetFieldsOfType<TEnum>)!;

        private static List<TFieldType> GetFieldsOfType<TFieldType>(Type type) =>
            type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fieldInfo => type.IsAssignableFrom(fieldInfo.FieldType))
                .Select(fieldInfo => (TFieldType)fieldInfo.GetValue(null)!)
                .ToList()!;
    }
}