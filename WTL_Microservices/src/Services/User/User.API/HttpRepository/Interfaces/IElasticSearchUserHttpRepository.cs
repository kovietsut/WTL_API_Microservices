using Microsoft.AspNetCore.Mvc;

namespace User.API.HttpRepository.Interfaces
{
    public interface IElasticSearchUserHttpRepository
    {
        Task<IActionResult> GetElasticSearchUser(int? pageNumber, int? pageSize);
    }
}
