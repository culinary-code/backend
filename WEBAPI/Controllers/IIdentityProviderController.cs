using BL.DTOs.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace WEBAPI.Controllers;

public interface IIdentityProviderController
{
    Task<IActionResult> RegisterUser(UserRegistrationRequestDto request);
}