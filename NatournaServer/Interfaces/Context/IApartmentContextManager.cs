using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Context
{
    public interface IApartmentContextManager
    {
        Task<List<ApartmentEntity>> GetAllAsync(int? apartmentId = null, int? buildingId = null, bool? isActive = null);

        Task<ApartmentEntity?> GetByIdAsync(int id);

        Task<List<ApartmentEntity>> GetByBuildingIdAsync(int buildingId);

        Task<ApartmentEntity> CreateAsync(ApartmentEntity apartment);

        Task<ApartmentEntity?> UpdateAsync(int id, ApartmentEntity apartment);

        Task<bool> DeleteAsync(int id);

        Task<ApartmentEntity?> SetActiveAsync(int id, bool isActive);
    }
}
