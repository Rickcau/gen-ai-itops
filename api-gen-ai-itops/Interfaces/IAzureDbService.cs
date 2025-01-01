using api_gen_ai_itops.Models;

namespace api_gen_ai_itops.Interfaces
{
    public interface IAzureDbService
    {
        Task<string> GetDbResults(string query);
        Task<PersonDetail?> GetMissingPerson(string name, int age, DateTime dateReported);
        Task<int> UpdateMissingPerson(int id, DateTime dateFound);
    }
}
