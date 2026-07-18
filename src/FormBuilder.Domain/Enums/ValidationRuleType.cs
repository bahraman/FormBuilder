namespace FormBuilder.Domain.Enums;

public enum ValidationRuleType
{
    Required = 0,
    MinLength = 1,
    MaxLength = 2,
    MinValue = 3,
    MaxValue = 4,
    Regex = 5,
    Email = 6,
    Phone = 7,
    Url = 8,
    AllowedFileTypes = 9,
    MaxFileSize = 10
}
