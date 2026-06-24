using Microsoft.AspNetCore.Mvc;
using Minimart_Api.DTOS.Address;
using Minimart_Api.DTOS.General;
using Minimart_Api.Services.Address;
using Minimart_Api.Services.CurrentUserServices;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.Controllers
{
    /// <summary>
    /// Enhanced Address controller following modern API design patterns
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // Require authentication for all endpoints
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AddressController> _logger;

        public AddressController(
            IAddressService addressService,
            ICurrentUserService currentUserService,
            ILogger<AddressController> logger)
        {
            _addressService = addressService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        #region Address CRUD Operations

        /// <summary>
        /// Get address by ID
        /// </summary>
        /// <param name="id">Address ID</param>
        /// <returns>Address details</returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAddressById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Invalid address ID"));
                }

                var result = await _addressService.GetAddressByIdAsync(id);
                
                if (!result.Success)
                {
                    return NotFound(ApiResponse<object>.CreateError(result.Message));
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address with ID {AddressId}", id);
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Get all addresses for the current user
        /// </summary>
        /// <returns>List of user addresses</returns>
        [HttpGet("my-addresses")]
        public async Task<IActionResult> GetMyAddresses()
        {
            try
            {
                var userId = _currentUserService.UserId;
                
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized(ApiResponse<object>.CreateError("User not authenticated"));
                }

                var result = await _addressService.GetAddressesByUserIdAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting addresses for current user");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Get addresses by user ID (Admin/Support use)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user addresses</returns>
        [HttpGet("user/{userId}")]
        //[Authorize(Roles = "Admin,Support")] // Restrict to admin/support
        public async Task<IActionResult> GetAddressesByUserId(
            [Required] string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(ApiResponse<object>.CreateError("User ID is required"));
                }

                var result = await _addressService.GetAddressesByUserIdAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting addresses for user {UserId}", userId);
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Add new address
        /// </summary>
        /// <param name="request">Address details</param>
        /// <returns>Created address details</returns>
        [HttpPost]
        public async Task<IActionResult> AddAddress([FromBody] CreateAddressRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Invalid request data",
                        ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
                }

                // Use current user if not specified
                var userId = request.ApplicationUserId ?? _currentUserService.UserId;
                
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(ApiResponse<object>.CreateError("User ID is required"));
                }

                request.ApplicationUserId = userId;
                var result = await _addressService.AddAddressAsync(request);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(nameof(GetAddressById), 
                    new { id = result.Data }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding address");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Update existing address
        /// </summary>
        /// <param name="id">Address ID</param>
        /// <param name="request">Updated address details</param>
        /// <returns>Update result</returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAddress(
            int id, 
            [FromBody] UpdateAddressRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Invalid request data",
                        ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
                }

                if (id <= 0)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Invalid address ID"));
                }

                request.AddressID = id;
                
                // Use current user if not specified
                var userId = request.ApplicationUserId ?? _currentUserService.UserId;
                request.ApplicationUserId = userId;

                var result = await _addressService.UpdateAddressAsync(request);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId}", id);
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Delete address
        /// </summary>
        /// <param name="id">Address ID</param>
        /// <returns>Delete result</returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Invalid address ID"));
                }

                var result = await _addressService.DeleteAddressAsync(id);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId}", id);
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        #endregion

        #region Address Management Operations

        /// <summary>
        /// Set address as default
        /// </summary>
        /// <param name="id">Address ID</param>
        /// <returns>Update result</returns>
        [HttpPatch("{id:int}/set-default")]
        public async Task<IActionResult> SetDefaultAddress(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Invalid address ID"));
                }

                var userId = _currentUserService.UserId;
                
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized(ApiResponse<object>.CreateError("User not authenticated"));
                }

                var result = await _addressService.SetDefaultAddressAsync(id, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default address {AddressId}", id);
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Get default address for current user
        /// </summary>
        /// <returns>Default address details</returns>
        [HttpGet("default")]
        public async Task<IActionResult> GetDefaultAddress()
        {
            try
            {
                var userId = _currentUserService.UserId;
                
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized(ApiResponse<object>.CreateError("User not authenticated"));
                }

                var result = await _addressService.GetDefaultAddressAsync(userId);
                
                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default address for current user");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Validate address format and completeness
        /// </summary>
        /// <param name="request">Address to validate</param>
        /// <returns>Validation result</returns>
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateAddress([FromBody] CreateAddressRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Invalid request data",
                        ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
                }

                var result = await _addressService.ValidateAddressAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating address");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        #endregion

        #region Legacy Support

        /// <summary>
        /// Legacy endpoint - Get addresses by user ID (maintains backward compatibility)
        /// </summary>
        [HttpGet("GetAddressesByUserId/{userId}")]
        public async Task<IActionResult> GetAddressesByUserIdLegacy(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(new
                    {
                        responseCode = 400,
                        responseMessage = "User ID is required"
                    });
                }

                var result = await _addressService.GetAddressesByUserIdAsync(userId);
                
                if (result.Success)
                {
                    return Ok(result.Data);
                }

                return BadRequest(new
                {
                    responseCode = 400,
                    responseMessage = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in legacy get addresses endpoint");
                return BadRequest(new
                {
                    responseCode = 500,
                    responseMessage = "An error occurred while fetching addresses.",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Legacy endpoint - Add address (maintains backward compatibility)
        /// </summary>
        [HttpPost("AddAddress")]
        public async Task<IActionResult> AddAddressLegacy([FromBody] AddressDTO address)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        responseCode = 400,
                        responseMessage = "Invalid address data",
                        errors = ModelState.Values.SelectMany(v => v.Errors)
                    });
                }

                var request = new CreateAddressRequest
                {
                    ApplicationUserId = address.ApplicationUserId,
                    Name = address.Name,
                    PhoneNumber = address.Phonenumber,
                    PostalAddress = address.PostalAddress,
                    County = address.County,
                    Town = address.Town,
                    PostalCode = address.PostalCode,
                    ExtraInformation = address.ExtraInformation,
                    IsDefault = address.isDefault
                };

                var result = await _addressService.AddAddressAsync(request);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        responseCode = 400,
                        responseMessage = result.Message
                    });
                }

                var updatedAddresses = await _addressService.GetAddressesByUserIdAsync(address.ApplicationUserId!);

                return Ok(new
                {
                    responseCode = 200,
                    responseMessage = "Address added successfully",
                    addresses = updatedAddresses.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in legacy add address endpoint");
                return BadRequest(new
                {
                    responseCode = 500,
                    responseMessage = "An error occurred while adding address.",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Legacy endpoint - Edit address (maintains backward compatibility)
        /// </summary>
        [HttpPost("EditAddress")]
        public async Task<IActionResult> EditAddressLegacy([FromBody] EditAddressDTO address)
        {
            try
            {
                var request = new UpdateAddressRequest
                {
                    AddressID = address.AddressID,
                    ApplicationUserId = address.ApplicationUserId,
                    Name = address.Name,
                    PhoneNumber = address.Phonenumber,
                    PostalAddress = address.PostalAddress,
                    County = address.County,
                    Town = address.Town,
                    PostalCode = address.PostalCode,
                    ExtraInformation = address.ExtraInformation,
                    IsDefault = address.isDefault
                };

                var result = await _addressService.UpdateAddressAsync(request);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        responseCode = 400,
                        responseMessage = result.Message
                    });
                }

                var updatedAddresses = await _addressService.GetAddressesByUserIdAsync(address.ApplicationUserId!);

                return Ok(new
                {
                    responseCode = 200,
                    responseMessage = "Address updated successfully.",
                    addresses = updatedAddresses.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in legacy edit address endpoint");
                return BadRequest(new
                {
                    responseCode = 500,
                    responseMessage = "An error occurred while updating the address.",
                    error = ex.Message
                });
            }
        }

        #endregion
    }

    #region Request Models

    /// <summary>
    /// Request model for creating a new address
    /// </summary>
    public class CreateAddressRequest
    {
        public string? ApplicationUserId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [MaxLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal address is required")]
        [MaxLength(200, ErrorMessage = "Postal address cannot exceed 200 characters")]
        public string PostalAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "County is required")]
        [MaxLength(50, ErrorMessage = "County cannot exceed 50 characters")]
        public string County { get; set; } = string.Empty;

        [Required(ErrorMessage = "Town is required")]
        [MaxLength(50, ErrorMessage = "Town cannot exceed 50 characters")]
        public string Town { get; set; } = string.Empty;

        [MaxLength(10, ErrorMessage = "Postal code cannot exceed 10 characters")]
        public string PostalCode { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Extra information cannot exceed 500 characters")]
        public string ExtraInformation { get; set; } = string.Empty;

        public bool IsDefault { get; set; }
    }

    /// <summary>
    /// Request model for updating an address
    /// </summary>
    public class UpdateAddressRequest
    {
        public int AddressID { get; set; }
        public string? ApplicationUserId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [MaxLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal address is required")]
        [MaxLength(200, ErrorMessage = "Postal address cannot exceed 200 characters")]
        public string PostalAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "County is required")]
        [MaxLength(50, ErrorMessage = "County cannot exceed 50 characters")]
        public string County { get; set; } = string.Empty;

        [Required(ErrorMessage = "Town is required")]
        [MaxLength(50, ErrorMessage = "Town cannot exceed 50 characters")]
        public string Town { get; set; } = string.Empty;

        [MaxLength(10, ErrorMessage = "Postal code cannot exceed 10 characters")]
        public string PostalCode { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Extra information cannot exceed 500 characters")]
        public string ExtraInformation { get; set; } = string.Empty;

        public bool IsDefault { get; set; }
    }

    #endregion
}
