using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Context
{
    public interface IApartmentContextManager
    {
        Task<List<ApartmentEntity>> GetAllAsync(int? apartmentId = null, int? buildingId = null, bool? isActive = null);

        Task<(List<ApartmentEntity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, int? buildingId = null, bool? isActive = null, string? search = null);

        Task<ApartmentEntity?> GetByIdAsync(int id);

        Task<List<ApartmentEntity>> GetByBuildingIdAsync(int buildingId);

        Task<ApartmentEntity> CreateAsync(ApartmentEntity apartment);

        Task<ApartmentEntity?> UpdateAsync(int id, ApartmentEntity apartment);

        Task<bool> DeleteAsync(int id);

        Task<ApartmentEntity?> SetActiveAsync(int id, bool isActive);
    }
}
