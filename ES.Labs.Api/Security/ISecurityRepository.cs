namespace ES.Labs.Api.Security;

public interface ISecurityRepository
{
    Task<IEnumerable<string>> GetAvailableRoles();
}