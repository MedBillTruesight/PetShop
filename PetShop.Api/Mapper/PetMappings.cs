using PetShop.Api.Dtos;

namespace PetShop.Api.Mapper
{
    public static class PetMappings
    {
        public static PetDto ToDto(this Models.Pet pet)
        {
            if (pet == null) return null;
            return new PetDto
            {
                Id = pet.Id,
                Name = pet.Name,
                Kind = pet.Kind,
                Price = pet.Price,
                Color = pet.Color,
                Breed = pet.Breed
            };
        }
        public static Models.Pet ToModel(this Dtos.CreatePetDto dto)
        {
            if (dto == null) return null;
            return new Models.Pet
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Kind = dto.Kind,
                Breed = dto.Breed,
                Price = dto.Price,
            };
        }
        public static Models.Pet ToModel(this Dtos.UpdatePetDto dto, Models.Pet existingPet)
        {
            if (dto == null) return null;

            if (existingPet == null)
            {
                return new Models.Pet
                {
                    Name = dto.Name,
                    Kind = dto.Kind,
                    Breed = dto.Breed,
                    Price = dto.Price,
                    Color = dto.Color
                };
            }

            existingPet.Name = dto.Name;
            existingPet.Kind = dto.Kind;
            existingPet.Breed = dto.Breed;
            existingPet.Price = dto.Price;
            existingPet.Color = dto.Color;

            return existingPet;
        }

    }
}
