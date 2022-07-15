using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Entities;
using Backend.Entities.Models;
using Microsoft.Extensions.Configuration;
using Backend.Payloads;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {

        private IConfiguration _config { get; }
        private readonly BackendContext _db;

        public CompanyController(BackendContext db, IConfiguration configuration)
        {
            _config = configuration;
            _db = db;
        }
        protected int getUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        }

        [HttpGet("getCompany")]
        public Company getCompany(long companyID)
        {
            var query = _db.Companies.AsNoTracking();
            var company = query.Where(c => c.Id == companyID).SingleOrDefault();
            return company;
        }

        [HttpGet("getBranchListings")]
        public IActionResult getCompanyListings(long branchID)
        {
            var branches = _db.Branches.Where(b => b.Id == branchID).AsNoTracking();
            List<User> sellers = new List<User>();
            foreach(Branch b in branches)
            {
                sellers.AddRange(_db.Users.Where(u => u.branchID == b.Id).AsNoTracking());
            }
            List<Vehicle> vehicles = new List<Vehicle>();
            foreach(User u in sellers)
            {
                vehicles.AddRange(_db.Vehicles.Where(v => v.UserID == u.Id).AsNoTracking());
            }
            return new JsonResult(new { array = vehicles });
        }
        [Authorize]
        [HttpGet("getMyCompany")]
        public IActionResult getMyCompany()
        {
            var foundUser = _db.Users.Where(u => u.Id == getUserId()).SingleOrDefault();
            var company = _db.Companies.Where(c => c.Id == foundUser.companyID).SingleOrDefault();
            var branches = _db.Branches.Where(b => b.CompanyID == company.Id).AsNoTracking();
            List<User> sellers = new List<User>();
            foreach (Branch b in branches)
            {
                sellers.AddRange(_db.Users.Where(u => u.branchID == b.Id).AsNoTracking().ToList());
            }
            foreach (User u in sellers)
            {
                u.Vehicles = new List<Vehicle>();
                u.branch = _db.Branches.Where(b => b.Id == u.branchID).SingleOrDefault();
                u.Vehicles.AddRange(_db.Vehicles.Where(v => v.UserID == u.Id).AsNoTracking().ToList());
            }
            return new JsonResult(new { sellers = sellers, company = company});
        }
        [Authorize(Roles = Role.Admin)]
        [HttpGet("getCompanies")]
        public IActionResult getCompanies()
        {
            var query = _db.Companies.AsNoTracking();
            return new JsonResult(new { array = query });
        }

        [Authorize]
        [HttpGet("getSellers")]
        public IActionResult getSellers(long branchID)
        {
            var sellers = _db.Users.Where(u => u.branchID == branchID).AsNoTracking().ToList();
            foreach(User u in sellers)
            {
                u.branch = _db.Branches.Where(b => b.Id == branchID).SingleOrDefault();
                u.Vehicles = _db.Vehicles.Where(v => v.UserID == u.Id).AsNoTracking().ToList();
            }
            return new JsonResult(new { array = sellers });
        }

        [HttpGet("getBranch")]
        public IActionResult getBranch(long branchID)
        {
            var query = _db.Branches.AsNoTracking();
            var branch = _db.Branches.Where(b => b.Id == branchID).SingleOrDefault();
            var company = _db.Companies.Where(c => c.Id == branch.CompanyID);
            return new JsonResult(new { branch = branch, company = company });
        }
        [Authorize(Roles = Role.Owner + "," + Role.Admin)]
        [HttpGet("getMyBranches")]
        public IActionResult getMyBranches()
        {
            var foundUser = _db.Users.Where(u => u.Id == getUserId()).SingleOrDefault();
            var result = _db.Branches.Where(b => b.CompanyID == foundUser.companyID).AsNoTracking();
            return new JsonResult(new { array = result });
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPost("addCompany")]
        public IActionResult addCompany(string companyName, string address,string email)
        {
            var foundUser = _db.Users.Where(u => u.Email == email).SingleOrDefault();
            if (foundUser == null) return BadRequest(new { message = "No user found with the email: " + email, status = false });
            if (foundUser.companyID != null) return BadRequest(new { message = "A user can own only one company.", status = false });
            Company c = new Company();
            c.companyName = companyName;
            c.address = address;
            c.owner = foundUser;

            _db.Companies.Add(c);
            _db.SaveChanges();

            foundUser.company = c;
            foundUser.companyID = c.Id;
            if (foundUser.Role == Role.User)
            {
                foundUser.Role = Role.Owner;
            }
            _db.SaveChanges();

        return new JsonResult(new { status = "Company added", ok = "true" });

        }
        [Authorize(Roles = Role.Owner + "," + Role.Admin)]
        [HttpPost("addBranch")]
        public IActionResult addBranch(string name, string address, string phonenumber)
        {
                var foundUser = _db.Users.Where(u => u.Id == getUserId()).SingleOrDefault();
                var foundCompany = _db.Companies.Where(c => c.Id == foundUser.companyID).SingleOrDefault();
                if (foundUser.companyID != foundCompany.Id)
                {
                    return BadRequest(new { message = "Can't add a branch to another company than yours", status = false });
                }
                Branch b = new Branch();
                b.Name = name;
                b.address = address;
                b.company = foundCompany;
                b.CompanyID = foundCompany.Id;
                b.phoneNumber = phonenumber;

                _db.Branches.Add(b);
                _db.SaveChanges();

                return new JsonResult(new { status = "Branch added to the company: " + foundCompany.companyName, ok = "true" });

        }

        [Authorize(Roles = Role.Owner + "," + Role.Admin)]
        [HttpDelete("deleteBranch")]
        public IActionResult deleteBranch(long branchID)
        {
            var foundBranch = _db.Branches.Where(b => b.Id == branchID).SingleOrDefault();
            if (foundBranch == null) return BadRequest(new { message = "Branch not found.", status = false });
            var foundUsers = _db.Users.Where(u => u.branchID == branchID).AsNoTracking().ToList();
            foreach(User u in foundUsers)
            {
                u.branchID = null;
            }
            _db.Branches.Remove(foundBranch);
            _db.SaveChanges();
            return new JsonResult(new { status = "Branch removed" });
        }

        [Authorize]
        [HttpPatch("changeOwner")]
        public IActionResult changeOwner(long companyID, string email)
        {
            var foundCompany = _db.Companies.Where(c => c.Id == companyID).SingleOrDefault();
            if (foundCompany == null) return BadRequest(new { message = "Company not found.", status = false });
            var foundOldOwner = _db.Users.Where(o => o.companyID == companyID).SingleOrDefault();
            var newOwner = _db.Users.Where(u => u.Email == email).SingleOrDefault();
            if (newOwner == null) return BadRequest(new { message = "No user with the email address: " + email, status = false });
            foundOldOwner.company = null;
            foundOldOwner.companyID = null;
            foundCompany.owner = newOwner;
            newOwner.company = foundCompany;
            newOwner.companyID = foundCompany.Id;
            newOwner.Role = Role.Owner;
            return new JsonResult(new { status = "Owner changed" });

        }

        [Authorize(Roles = Role.Admin)]
        [HttpDelete("deleteCompany")]
        public IActionResult deleteCompany(long companyID)
        {
            var foundCompany = _db.Companies.Where(c => c.Id == companyID).SingleOrDefault();
            if (foundCompany == null) return BadRequest(new { message = "Company not found.", status = false });
            var branches = _db.Branches.Where(b => b.CompanyID == companyID).AsNoTracking();
            foreach(Branch b in branches)
            {
                this.deleteBranch(b.Id);
            }
            var user = _db.Users.Where(u => u.companyID == companyID).SingleOrDefault();
            user.companyID = null;
            _db.Companies.Remove(foundCompany);
            _db.SaveChanges();
            return new JsonResult(new { status = "Company removed" });
        }

        [Authorize(Roles = Role.Owner + "," + Role.Admin)]
        [HttpPost("addUserToBranch")]
        public IActionResult addUserToBranch(long BranchID, string email)
        {
            var foundUser = _db.Users.Where(u => u.Email == email).SingleOrDefault();
            if (foundUser == null)
            {
                return BadRequest(new { message = "No user found with the email: " + email, status = false });
            }
            if (foundUser.Role == Role.Admin || foundUser.Role == Role.Owner)
            {
                return BadRequest(new { message = "You can't assign an admin/owner of a company to a branch", status = false });
            }
            var foundBranch = _db.Branches.Where(b => b.Id == BranchID).SingleOrDefault();
                if (foundBranch.CompanyID != _db.Users.Where(u => u.Id == getUserId()).SingleOrDefault().companyID)
                {
                    return new JsonResult(new { status = "This is not your company.", ok = "false" });
                }

                foundUser.branchID = BranchID;
                foundUser.branch = foundBranch;
                foundUser.Role = Role.Seller;

                _db.SaveChanges();

                return new JsonResult(new { status = "User added to the branch: " + foundBranch.Name, ok = "true" });

        }
    }
}

