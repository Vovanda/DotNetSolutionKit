namespace NamespaceRoot.ProductName.Common.Contracts.Domain.Context;

/// <summary>
/// Контекст пользователя, единый для всех сервисов приложения.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Имя пользователя (для хранения в Data или логах)
    /// </summary>
    public static readonly string DisplayNamePropName = "DisplayName";

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    string Login { get; }

    /// <summary>
    /// Роли пользователя
    /// </summary>
    List<Role> Roles { get; }

    /// <summary>
    /// Дополнительные данные пользователя
    /// </summary>
    Dictionary<string, object>? Data { get; }

    /// <summary>
    /// Роли, доступные в приложении
    /// </summary>
    public enum Role
    {
        User = 1,
        System
    }

    /// <summary>
    /// Проверка принадлежности к определённой роли.
    /// </summary>
    bool InRole(Role role) => Roles.Contains(role);
}