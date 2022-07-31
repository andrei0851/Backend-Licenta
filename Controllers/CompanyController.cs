using System;
using Microsoft.AspNetCore.Mvc;
using Backend.Entities.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Backend.Services;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly IRepositoryManager _repositoryManager;

        public CompanyController(ICompanyService companyService, IRepositoryManager repositoryManager)
        {
            _companyService = companyService;
            _repositoryManager = repositoryManager;
        }
        protected int getUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        }

        [HttpGet("getCompany")]
        public Company getCompany(long companyID)
        {
            var company = _repositoryManager.getCompany(companyID);
            return company;
        }

        [HttpGet("getBranchListings")]
        public IActionResult getBranchListings(long branchID)
        {
            var vehicles = _companyService.getBranchListings(branchID);
            return new JsonResult(new { array = vehicles });
        }
        [Authorize]
        [HttpGet("getMyCompany")]
        public IActionResult getMyCompany()
        {
            var companyID = _repositoryManager.getUserById(getUserId()).companyID.GetValueOrDefault();
            var company = _repositoryManager.getCompany(companyID);
            var sellers = _repositoryManager.getSellersByCompany(company.Id);
            return new JsonResult(new { sellers = sellers, company = company});
        }
        [Authorize(Roles = Role.Admin)]
        [HttpGet("getCompanies")]
        public IActionResult getCompanies()
        {
            var companies = _repositoryManager.getCompanies();
            return new JsonResult(new { array = companies });
        }

        [Authorize]
        [HttpGet("getSellers")]
        public IActionResult getSellers(long branchID)
        {
            var sellers = _repositoryManager.getSellersByBranch(branchID);
            return new JsonResult(new { array = sellers });
        }

        [HttpGet("getBranch")]
        public IActionResult getBranch(long branchID)
        {
            var branch = _repositoryManager.getBranch(branchID);
            var company = _repositoryManager.getCompany(branch.CompanyID);
            return new JsonResult(new { branch = branch, company = company });
        }
        [Authorize(Roles = Role.Owner + "," + Role.Admin)]
        [HttpGet("getMyBranches")]
        public IActionResult getMyBranches()
        {
            var foundUser = _repositoryManager.getUserById(getUserId());
            var result = _repositoryManager.getBranchesByCompany(foundUser.companyID.GetValueOrDefault());
            return new JsonResult(new { array = result });
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPost("addCompany")]
        public IActionResult addCompany(string companyName, string address,string email)
        {
            try
            {
                _companyService.addCompany(companyName, address, email);
                return new JsonResult(new { status = "Company added", ok = "true" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }


        }
        [Authorize(Roles = Role.Owner + "," + Role.Admin)]
        [HttpPost("addBranch")]
        public IActionResult addBranch(string name, string address, string phonenumber)
        {
            try
            {
                _companyService.addBranch(name, address, phonenumber, getUserId());
                return new JsonResult(new { status = "Branch added to your company", ok = "true" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        [Authorize(Roles = Role.Owner + "," + Role.Admin)]
        [HttpDelete("deleteBranch")]
        public IActionResult deleteBranch(long branchID)
        {
            try
            {
                _companyService.deleteBranch(branchID);
                return new JsonResult(new { status = "Branch removed" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPatch("changeOwner")]
        public IActionResult changeOwner(long companyID, string email)
        {
            try
            {
                _companyService.changeOwner(companyID, email);
                return new JsonResult(new { status = "Owner changed" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        [Authorize(Roles = Role.Admin)]
        [HttpDelete("deleteCompany")]
        public IActionResult deleteCompany(long companyID)
        {
            try
            {
                _companyService.deleteCompany(companyID);
                return new JsonResult(new { status = "Company removed" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = Role.Owner + "," + Role.Admin)]
        [HttpPost("addUserToBranch")]
        public IActionResult addUserToBranch(long branchID, string email)
        {
            try
            {
                _companyService.addUserToBranch(branchID, email, getUserId());
                return new JsonResult(new { status = "User added to the branch.", ok = "true" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

