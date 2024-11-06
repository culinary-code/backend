using System.Threading.Tasks;
using BL.DTOs.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication3.Controllers;

public interface IIdentityProviderController
{
    Task<IActionResult> RegisterUser(UserRegistrationRequestDto request);
}