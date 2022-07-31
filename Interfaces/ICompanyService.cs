using System.Collections.Generic;
using Backend.Entities.Models;

namespace Backend.Services;

public interface ICompanyService
{
    void addCompany(string companyName, string address,string ownerEmail);
    List<Vehicle> getBranchListings(long branchID);
    void addBranch(string name, string address, string phonenumber, long userID);
    void deleteBranch(long branchID);
    void changeOwner(long companyID, string ownerEmail);
    void deleteCompany(long companyID);
    void addUserToBranch(long branchID, string email,long userID);
}