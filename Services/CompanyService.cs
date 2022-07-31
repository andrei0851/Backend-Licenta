using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Entities;
using Backend.Entities.Models;

namespace Backend.Services;

public class CompanyService : ICompanyService
{
    
    private readonly BackendContext _db;
    private readonly IRepositoryManager _repositoryManager;
    
    public CompanyService(BackendContext db, IRepositoryManager repositoryManager)
    {
        _db = db;
        _repositoryManager = repositoryManager;
    }

    public void addCompany(string companyName, string address,string ownerEmail)
    {
        var foundUser = _repositoryManager.getUserByEmail(ownerEmail);
        if (foundUser == null) throw new Exception("No user found.");
        if (foundUser.companyID != null) throw new Exception("User already has a company.");
        Company c = new Company();
        c.companyName = companyName;
        c.address = address;
        c.owner = foundUser;

        _repositoryManager.addCompany(c);

        foundUser.company = c;
        foundUser.companyID = c.Id;
        if (foundUser.Role == Role.User)
        {
            foundUser.Role = Role.Owner;
        }
        _db.SaveChanges();
    }
    
    public List<Vehicle> getBranchListings(long branchID)
    {
        var sellers = _repositoryManager.getSellersByBranch(branchID);
        List<Vehicle> vehicles = new List<Vehicle>();
        foreach(User u in sellers)
        {
            vehicles.AddRange(_repositoryManager.getUserListings(u.Id));
        }
        return vehicles;
    }

    public void addBranch(string name, string address, string phonenumber, long userID)
    {
        var foundUser = _repositoryManager.getUserById(userID);
        var foundCompany = _repositoryManager.getCompany(foundUser.companyID.GetValueOrDefault());
        if (foundUser.companyID != foundCompany.Id) throw new Exception("Can't add a branch to another company than yours");
        Branch b = new Branch();
        b.Name = name;
        b.address = address;
        b.company = foundCompany;
        b.CompanyID = foundCompany.Id;
        b.phoneNumber = phonenumber;
        _repositoryManager.addBranch(b);
    }
    
    public void deleteBranch(long branchID)
    {
        var foundBranch = _repositoryManager.getBranch(branchID);
        if (foundBranch == null) throw new Exception("Branch not found");
        _repositoryManager.removeBranch(foundBranch);
    }

    public void changeOwner(long companyID, string ownerEmail)
    {
        var foundCompany = _repositoryManager.getCompany(companyID);
        if (foundCompany == null) throw new Exception("Company not found.");
        var foundOldOwner = _repositoryManager.getUserByCompany(companyID);
        var newOwner = _repositoryManager.getUserByEmail(ownerEmail);
        if (newOwner == null) throw new Exception("No user found with the email address " + newOwner.Email);
        foundOldOwner.company = null;
        foundOldOwner.companyID = null;
        foundCompany.owner = newOwner;
        newOwner.company = foundCompany;
        newOwner.companyID = foundCompany.Id;
        newOwner.Role = Role.Owner;
        _db.SaveChanges();
    }

    public void deleteCompany(long companyID)
    {
        var foundCompany = _repositoryManager.getCompany(companyID);
        if (foundCompany == null) throw new Exception("Company not found.");
        _repositoryManager.removeCompany(foundCompany);
    }

    public void addUserToBranch(long branchID, string email,long userID)
    {
        var foundUser = _repositoryManager.getUserByEmail(email);
        if (foundUser == null) throw new Exception("No user found with the email " + email);
        if (foundUser.Role == Role.Admin || foundUser.Role == Role.Owner) throw new Exception("You can't assign an admin/owner of a company to a branch");
        var foundBranch = _repositoryManager.getBranch(branchID);
        if (foundBranch.CompanyID != _repositoryManager.getUserById(userID).companyID)
            throw new Exception("This is not your company.");
        foundUser.branchID = branchID;
        foundUser.branch = foundBranch;
        foundUser.Role = Role.Seller;

        _db.SaveChanges();
    }
}