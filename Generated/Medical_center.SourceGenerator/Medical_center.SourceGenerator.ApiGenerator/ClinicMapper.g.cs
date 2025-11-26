#nullable enable
using System;
using Medical_center.Models;
using Medical_center.Models.Generated;

namespace Medical_center.Models.Generated
{
    public static class ClinicMapper
    {
        public static ClinicDto ToDto(this Clinic entity)
        {
            if (entity == null) return null;

            return new ClinicDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Address = entity.Address,
                PhoneNumber = entity.PhoneNumber
            };
        }
    }
}
