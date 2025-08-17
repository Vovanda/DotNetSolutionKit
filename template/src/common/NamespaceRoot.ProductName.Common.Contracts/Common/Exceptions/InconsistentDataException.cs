namespace NamespaceRoot.ProductName.Common.Contracts.Common.Exceptions;

/// <summary>
/// Возникает, если данные не полны или противоречивы
/// </summary>
/// <param name="message"></param>
public class InconsistentDataException(string? message, string? parameterName = null) : BusinessLogicException(message)
{
    public string? ParameterName => parameterName;

    /// <summary>
    /// Выбросит исключение если значение пустое
    /// </summary>
    /// <exception cref="InconsistentDataException"></exception>
    public static void ThrowIfEmpty(string name, string? data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            throw new InconsistentDataException($"{name} не может содержать пустые значения", name);
        }
    }

    /// <summary>
    /// Выбросит исключение если значение пустое
    /// </summary>
    /// <exception cref="InconsistentDataException"></exception>
    public static void ThrowIfEmpty<T>(string name, T? data)
    {
        object? defaultVal = default(T);

        var underlyingType = Nullable.GetUnderlyingType(typeof(T));
        if (underlyingType != null)
        {
            defaultVal = Activator.CreateInstance(underlyingType);
        }

        if (data == null || data.Equals(defaultVal))
        {
            throw new InconsistentDataException($"{name} не может содержать пустые значения", name);
        }
    }

    /// <summary>
    /// Выбросит исключение если значение не определено
    /// </summary>
    /// <exception cref="InconsistentDataException"></exception>
    public static void ThrowIfNull<T>(string name, T? data)
    {
        if (data == null)
        {
            throw new InconsistentDataException($"{name} не может содержать пустые значения", name);
        }
    }

    /// <summary>
    /// Проверим, что строка является кодом валюты, ну или хотя бы похожа на неё
    /// </summary>
    public static void ThrowIfNotCurrencyCode(string name, string? data)
    {
        ThrowIfEmpty(name, data);

        if (data!.Length != 3)
        {
            throw new InconsistentDataException($"{name} должна содержать код валюты");
        }
    }
    
    /// <summary>
    /// Проверим, что значение в диапазоне от 0 до 100
    /// </summary>
    public static void ThrowIfNotPercentWhenNotNull(string name, decimal? data)
    {
        if (data is < 0 || data is > 100)
        {
            throw new InconsistentDataException($"{name} должн быть в диапазоне от 0 до 100");
        }
    }

    /// <summary>
    /// Проверим, что строка не превышает длину
    /// </summary>
    public static void ThrowIfStringToLong(string name, string? data, int expectedLength)
    {
        if (data?.Length > expectedLength)
        {
            throw new InconsistentDataException($"{name} превышает максимальную длину. Длина {data.Length}, максимально {expectedLength}");
        }
    }
}