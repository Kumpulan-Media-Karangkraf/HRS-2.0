namespace HRS_2._0.Repositories.Abstract
{
    public interface IActiveDirectoryService
    {
        bool IsAuthenticated(string domain, string username, string password, out string errorMessage);

    }
}
