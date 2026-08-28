using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Context
{
    public interface IApartmentContextManager
    {
        Task<(List<ApartmentEntity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, int? buildingId = null, string? search = null);

        Task<List<ApartmentEntity>> GetAllAsync(bool? isActive = null);

        Task<ApartmentEntity?> GetByIdAsync(int id);

        Task<ApartmentEntity> CreateAsync(ApartmentEntity apartment);

        Task<ApartmentEntity?> UpdateAsync(int id, ApartmentEntity apartment);

        Task<bool> DeleteAsync(int id);

        Task<ApartmentEntity?> SetActiveAsync(int id, bool isActive);
    }
}
