using System;
using System.Collections.Generic;
using Backend.Entities;
using Backend.Entities.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Services;

public class RepositoryManager : IRepositoryManager
{
    
    private readonly BackendContext _db;
    
    public RepositoryManager(BackendContext db)
    {
        _db = db;
    }

    public User getUserByEmail(string email)
    {
        return _db.Users.Where(u => u.Email == email).SingleOrDefault();
    }
    
    public User getUserById(long userID)
    {
        return _db.Users.Where(u => u.Id == userID).SingleOrDefault();
    }
    
    public User getUserByCompany(long companyID)
    {
        return _db.Users.Where(u => u.companyID == companyID).SingleOrDefault();
    }


    public void addUser(User user)
    {
        _db.Users.Add(user);
        _db.SaveChanges();
    }

    public void addProfilePicture(ProfilePicture profilePicture)
    {
        _db.ProfilePictures.Add(profilePicture);
        _db.SaveChanges();
    }

    public void removeProfilePicture(ProfilePicture profilePicture)
    {
        _db.ProfilePictures.Remove(profilePicture);
        _db.SaveChanges();
    }

    public ProfilePicture getProfilePictureById(long userID)
    {
        return _db.ProfilePictures.Where(photo => photo.UserID == userID).SingleOrDefault();
    }
    
    public string getProfilePicture(long userID)
    {
        var found = _db.ProfilePictures.Where(photo => photo.UserID == userID).SingleOrDefault();
        if (found == null)
            return "https://carsalewebappstorage.blob.core.windows.net/firstcontainer/no-photo-available-icon-20.jpeg";
        return found.imgLink;
    }
    
    
        public Company getCompany(long companyID)
    {
        return _db.Companies.Where(c => c.Id == companyID).SingleOrDefault();
    }

    public Branch getBranch(long branchID)
    {
        return _db.Branches.Where(b => b.Id == branchID).SingleOrDefault();
    }



    public List<Company> getCompanies()
    {
        var companies = _db.Companies.ToList();
        return companies;
    }

    public List<User> getSellersByBranch(long branchID)
    {
        var sellers = _db.Users.Where(u => u.branchID == branchID).ToList();
        foreach(User u in sellers)
        {
            u.branch = _db.Branches.Where(b => b.Id == branchID).SingleOrDefault();
            u.Vehicles = _db.Vehicles.Where(v => v.UserID == u.Id).ToList();
        }
        return sellers;
    }

    public List<User> getSellersByCompany(long companyID)
    {
        var branches = getBranchesByCompany(companyID);
        List<User> sellers = new List<User>();
        foreach (Branch b in branches)
        {
            sellers.AddRange(_db.Users.Where(u => u.branchID == b.Id).ToList());
        }
        foreach (User u in sellers)
        {
            u.Vehicles = new List<Vehicle>();
            u.branch = _db.Branches.Where(b => b.Id == u.branchID).SingleOrDefault();
            u.Vehicles = _db.Vehicles.Where(v => v.UserID == u.Id).ToList();
        }
        return sellers;
    }

    public List<Branch> getBranchesByCompany(long companyID)
    {
        var branches = _db.Branches.Where(b => b.CompanyID == companyID).ToList();
        return branches;
    }

    public void addCompany(Company company)
    {
        _db.Companies.Add(company);
        _db.SaveChanges();
    }
    
    public void addBranch(Branch branch)
    {
        _db.Branches.Add(branch);
        _db.SaveChanges();
    }

    public void removeBranch(Branch branch)
    {
        var foundUsers = _db.Users.Where(u => u.branchID == branch.Id).ToList();
        foreach(User u in foundUsers)
        {
            u.branchID = null;
        }
        _db.Branches.Remove(branch);
        _db.SaveChanges();
    }

    public void removeCompany(Company company)
    {
        var branches = getBranchesByCompany(company.Id);
        foreach(Branch b in branches)
        {
            removeBranch(b);
        }
        var user = getUserByCompany(company.Id);
        user.companyID = null;
        _db.Companies.Remove(company);
        _db.SaveChanges();
    }

    public Vehicle getVehicleById(long vehicleID)
    {
        return _db.Vehicles.Where(v => v.Id == vehicleID).SingleOrDefault();
    }

    public long addVehicle(Vehicle vehicle)
    {
        _db.Vehicles.Add(vehicle);
        _db.SaveChanges();
        return vehicle.Id;
    }
    
        public int getRemainingListings(long userID)
    {

        return _db.Users.Where(u => u.Id == userID).SingleOrDefault().availableListings;
    }

    public Boolean isAvailable(long vehicleID)
    {
        var foundvehicle = _db.Vehicles.Where(v => v.Id == vehicleID).SingleOrDefault();
        if (foundvehicle.active) return true;
        return false;
    }

    public Boolean isFavorite(long vehicleID, long userID)
    {
        var query = _db.Favorites.Where(v => v.VehicleId == vehicleID && v.UserId == userID).SingleOrDefault();
        if (query == null) return false;
        return true;
    }

    public Promoted getPromotedByVehicleId(long vehicleID)
    {
        return _db.Promoted.Where(p => p.vehicleId == vehicleID).SingleOrDefault();
    }

    public void addPromoted(Promoted promoted)
    {
        _db.Promoted.Add(promoted);
        _db.SaveChanges();
    }

    public void addFavorites(Favorites favorite)
    {
        _db.Favorites.Add(favorite);
        _db.SaveChanges();
    }

    public Favorites getFavorite(long vehicleID, long userID)
    {
        return _db.Favorites.Where(v => v.VehicleId == vehicleID && v.UserId == userID).SingleOrDefault();
    }

    public void removeFavorite(Favorites favorite)
    {
        _db.Favorites.Remove(favorite);
        _db.SaveChanges();
    }

    public Country getCountry(long countryID)
    {
        return _db.Countries.Where(c => c.countryID == countryID).SingleOrDefault();
    }

    public void removeFavorites(List<Favorites> favorites)
    {
        _db.Favorites.RemoveRange(favorites);
        _db.SaveChanges();
    }
    
    public List<Vehicle> getAllListings()
    {
        var vehicleQuery = _db.Vehicles.Where(v => v.active == true);
        return vehicleQuery.ToList();
    }

    public Vehicle getListing(long vehicleID)
    {
        return _db.Vehicles.Where(v => v.Id == vehicleID).SingleOrDefault();
    }

    public List<Vehicle> getUserListings(long userID)
    {
        var vehicleQuery = _db.Vehicles.Where(v => v.UserID == userID).ToList();
        return vehicleQuery;
    }

    public void addMake(Make make)
    {
        _db.Makes.Add(make);
        _db.SaveChanges();
    }
    

    public int getTotalListings()
    {
        return _db.Vehicles.Where(v => v.active == true).ToList().Count();
    }

    public List<VehicleImage> getListingPhotos(long listingID)
    {
        return _db.VehicleImages.Where(i => i.vehicleID == listingID).ToList();
    }

    public List<VehicleColor> getColors()
    {
        return _db.VehicleColor.ToList();
    }
    
    public List<FuelType> GetFuelTypes()
    {
        return _db.FuelType.ToList();
    }

    public List<Country> getCountries()
    {
        return _db.Countries.ToList();
    }

    public List<VehicleType> getTypes()
    {
        return _db.VehicleType.ToList();
    }

    public List<Make> getMakes()
    {
        var makes = _db.Makes.ToList();
        return makes.OrderBy(o => o.name).ToList();
    }

    public void addSale(Sale sale)
    {
        _db.Sales.Add(sale);
        _db.SaveChanges();
    }

    public Sale getSaleByVehicle(long vehicleID)
    {
        return _db.Sales.Where(v => v.vehicleID == vehicleID).SingleOrDefault();
    }

    public VehicleImage getVehicleImage(long listingID, int order)
    {
        return _db.VehicleImages.Where(i => i.vehicleID == listingID && i.order == order).SingleOrDefault();
    }

    public void addVehicleImage(VehicleImage image)
    {
        _db.VehicleImages.Add(image);
        _db.SaveChanges();
    }

    public List<Favorites> getFavoritesByVehicle(long vehicleID)
    {
        return _db.Favorites.Where(v => v.VehicleId == vehicleID).ToList();
    }

    public List<Model> getModels(long makeID)
    {
        var models = _db.Models.Where(m => m.makeID == makeID).ToList();
        return models.OrderBy(m => m.name).ToList();
    }

    public void removeModels(List<Model> models)
    {
        _db.Models.RemoveRange(models);;
        _db.SaveChanges();
    }

    public void removeModel(Model model)
    {
        _db.Models.Remove(model);
        _db.SaveChanges();
    }

    public void removePromoted(Promoted promoted)
    {
        _db.Promoted.Remove(promoted);
        _db.SaveChanges();
    }

    public void removeVehicle(Vehicle vehicle)
    {
        _db.Vehicles.Remove(vehicle);
        _db.SaveChanges();
    }

    public void removeVehicleImage(VehicleImage image)
    {
        _db.VehicleImages.Remove(image);
        _db.SaveChanges();
    }
    
    public List<Promoted> getNotPromoted()
    {
        return _db.Promoted.Where(p => p.promotedUntil < DateTime.Now).ToList();
    }

    public List<Promoted> getPromoted()
    {
        return _db.Promoted.Where(p => p.promotedUntil > DateTime.Now).ToList();
    }
    
    public void addModel(Model model)
    {
        _db.Models.Add(model);
        _db.SaveChanges();
    }

    public VehicleColor getVehicleColor(long vehicleColorID)
    {
        return _db.VehicleColor.Where(vc => vc.Id == vehicleColorID).SingleOrDefault();
    }

    public FuelType getFuelType(long fuelTypeID)
    {
        return _db.FuelType.Where(ft => ft.Id == fuelTypeID).SingleOrDefault();
    }

    public VehicleType getVehicleType(long vehicleTypeID)
    {
        return _db.VehicleType.Where(vt => vt.Id == vehicleTypeID).SingleOrDefault();
    }

    public void removeMake(Make make)
    {
        _db.Makes.Remove(make);
        _db.SaveChanges();
    }

    public Model getModel(long makeID, string name)
    {
        return _db.Models.Where(m => m.name.ToLower() == name.ToLower() && m.makeID == makeID).SingleOrDefault();
    }
    
    public Model getModel(long modelID)
    {
        return _db.Models.Where(m => m.Id == modelID).SingleOrDefault();
    }

    public Make getMakeByName(string name)
    {
        return _db.Makes.Where(m => m.name.ToLower() == name.ToLower()).SingleOrDefault();
    }
    
    public Make getMakeById(long makeID)
    {
        return _db.Makes.Where(m => m.Id == makeID).SingleOrDefault();
    }

    public List<Favorites> getFavoritesByUser(long userID)
    {
        return _db.Favorites.Where(f => f.UserId == userID).ToList();
    }

    public Boolean makeHasListings(long makeID)
    {
        if (_db.Vehicles.Where(v => v.makeID == makeID).SingleOrDefault() != null) return true;
        else return false;
    }
    
    public Boolean modelHasListing(long modelID)
    {
        if (_db.Vehicles.Where(v => v.modelID == modelID).SingleOrDefault() != null) return true;
        else return false;
    }

}