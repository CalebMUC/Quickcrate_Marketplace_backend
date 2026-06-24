using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.DTOS.Address
{
    #region Modern DTOs

    /// <summary>
    /// Enhanced address detail DTO with comprehensive information
    /// </summary>
    public class AddressDetailDto
    {
        public int AddressID { get; set; }
        public string? ApplicationUserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PostalAddress { get; set; } = string.Empty;
        public string County { get; set; } = string.Empty;
        public string Town { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string ExtraInformation { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }

        // Enhanced properties
        public int? CountyId { get; set; }
        public int? TownId { get; set; }
        public string FullAddress => $"{PostalAddress}, {Town}, {County} {PostalCode}".Trim();
        public bool IsValid => !string.IsNullOrWhiteSpace(Name) &&
                              !string.IsNullOrWhiteSpace(PostalAddress) &&
                              !string.IsNullOrWhiteSpace(County) &&
                              !string.IsNullOrWhiteSpace(Town);
    }

    /// <summary>
    /// Address validation result
    /// </summary>
    public class AddressValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();

        public void AddError(string error)
        {
            ValidationErrors.Add(error);
            IsValid = false;
        }

        public void AddSuggestion(string suggestion)
        {
            Suggestions.Add(suggestion);
        }
    }

    /// <summary>
    /// Address summary DTO for list views
    /// </summary>
    public class AddressSummaryDto
    {
        public int AddressID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortAddress { get; set; } = string.Empty;
        public string Town { get; set; } = string.Empty;
        public string County { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }

    #endregion

    #region Legacy DTOs (maintained for backward compatibility)

    /// <summary>
    /// Legacy address DTO - maintained for backward compatibility
    /// </summary>
    public class AddressDTO
    {
        public string? ApplicationUserId { get; set; } // Identity User ID
        public string Name { get; set; } = string.Empty;
        public string Phonenumber { get; set; } = string.Empty;
        public string PostalAddress { get; set; } = string.Empty;
        public string County { get; set; } = string.Empty;
        public string Town { get; set; } = string.Empty;
        public string ExtraInformation { get; set; } = string.Empty;
        public bool isDefault { get; set; }
        public string PostalCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Legacy edit address DTO - maintained for backward compatibility
    /// </summary>
    public class EditAddressDTO
    {
        public int AddressID { get; set; }
        public string? ApplicationUserId { get; set; } // Identity User ID
        public string Name { get; set; } = string.Empty;
        public string Phonenumber { get; set; } = string.Empty;
        public string PostalAddress { get; set; } = string.Empty;
        public string County { get; set; } = string.Empty;
        public string Town { get; set; } = string.Empty;
        public string ExtraInformation { get; set; } = string.Empty;
        public bool isDefault { get; set; }
        public string PostalCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Legacy get address DTO - maintained for backward compatibility
    /// </summary>
    public class GetAddressDTO
    {
        public int AddressID { get; set; }
        public string? ApplicationUserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PostalAddress { get; set; } = string.Empty;
        public int CountyId { get; set; }
        public int TownId { get; set; }
        public string ExtraInformation { get; set; } = string.Empty;
        public bool isDefault { get; set; }
        public string PostalCode { get; set; } = string.Empty;
        public string County { get; set; } = string.Empty;
        public string Town { get; set; } = string.Empty;
    }

    #endregion
}
