
namespace PetShop.Domain.Exceptions;
public struct ValidationCode
{
    public static ValidationCode None {get; set;} =  new ValidationCode("NONE");
    public static ValidationCode MissingRequirement {get; set;} =  new ValidationCode("MISSING_REQUIREMENT");
    public static ValidationCode MissingRequirementEntity {get; set;} =  new ValidationCode("MISSING_REQUIREMENT_ENTITY");
    public static ValidationCode MissingRequirementField {get; set;} =  new ValidationCode("MISSING_REQUIREMENT_FIELD");
    public static ValidationCode DuplicateEntity {get; set;} =  new ValidationCode("DUPLICATE_ENTITY");
    public static ValidationCode InvalideValue {get; set;} =  new ValidationCode("INVALIDE_VALUE");


    //implementation of the ValidationCode
    public string Value {get; set;} = "NONE";

    private ValidationCode(string value) => Value = value;
    public override readonly string ToString() => Value;

    public static implicit operator string(ValidationCode x) => x.Value;

    public static ValidationCode FromString(string? value) => new ValidationCode(value ?? None);

}