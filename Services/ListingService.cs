using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Entities;
using Backend.Entities.Models;
using Backend.Payloads;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using MimeKit;


namespace Backend.Services;

public class ListingService : IListingService
{
    
    private readonly BackendContext _db;
    private readonly IMailService _mailService;
    private readonly IPhotoService _photoService;
    private readonly IRepositoryManager _repositoryManager;
    
    
    public ListingService(BackendContext db, IMailService mailService, IPhotoService photoService, IRepositoryManager repositoryManager)
    {
        _db = db;
        _mailService = mailService;
        _photoService = photoService;
        _repositoryManager = repositoryManager;
    }

    public void addMake(string name)
    {
        var existent = _repositoryManager.getMakeByName(name);
        if (existent != null) throw new Exception("Manufacturer already exists.");
        Make m = new Make();
        m.name = name;
        _repositoryManager.addMake(m);
    }

    public void deleteMake(long makeID)
    {
        
        var make = _repositoryManager.getMakeById(makeID);
        if (make == null) throw new Exception("Manufacturer doesn't exist.");
        var models = _repositoryManager.getModels(makeID);
        var hasListings = _repositoryManager.makeHasListings(makeID);
        if (hasListings) throw new Exception("Manufacturer has existing listings.");
        _repositoryManager.removeModels(models);
        _repositoryManager.removeMake(make);
    }
    
    public void deleteModel(long modelID)
    {
        var model = _repositoryManager.getModel(modelID);
        if (model == null) throw new Exception("Model doesn't exist.");
        var hasListings = _repositoryManager.modelHasListing(modelID);
        if (hasListings) throw new Exception("Model has existing listings.");
        _repositoryManager.removeModel(model);
    }
    
    public void addModel(long makeID, string name)
    {
        var existent = _repositoryManager.getModel(makeID, name);
        if (existent != null) throw new Exception("Model already exists for this manufacturer.");
        Model m = new Model();
        m.makeID = makeID;
        m.name = name;
        _repositoryManager.addModel(m);
    }
    
    public List<Vehicle> getFavorites(long userID)
    {
        var favoritesQuery = _repositoryManager.getFavoritesByUser(userID);
        List<Vehicle> favorites = new List<Vehicle>();
        foreach(Favorites f in favoritesQuery)
        {
            Vehicle fav = _repositoryManager.getVehicleById(f.VehicleId);
            favorites.Add(fav);
        }
        return favorites.ToList();
    }
    
    public void refreshPromoted()
    {
        var queryPromoted = _repositoryManager.getNotPromoted();
        foreach(Promoted p in queryPromoted)
        {
            p.isPromoted = false;
        }
        _db.SaveChanges();
    }

    public List<Vehicle> getPromoted()
    {
        refreshPromoted();
        List<Vehicle> promotedVehicles = new List<Vehicle>();
        var query = _repositoryManager.getPromoted();
        foreach(Promoted prom in query)
        {
            Vehicle promoted = _repositoryManager.getVehicleById(prom.vehicleId);
            if (promoted.active)
            {
                promotedVehicles.Add(promoted);
            }
        }

        return promotedVehicles;
    }

    public long addListing(ListingPayload listingPayload, long userID)
    {
            var foundUser = _repositoryManager.getUserById(userID);
            if(foundUser.Role == "User")
            {
                if (foundUser.availableListings < 1) throw new Exception("You have 0 listings available.");
                foundUser.availableListings--;
            }
            var vehicle = new Vehicle();
            vehicle.UserID = userID;
            vehicle.user = foundUser.Lastname + " " + foundUser.Firstname;
            vehicle.description = listingPayload.description;
            vehicle.price = listingPayload.price;
            vehicle.km = listingPayload.km;
            vehicle.manufactureYear = listingPayload.manufactureYear;
            if (listingPayload.VIN != null) vehicle.VIN = listingPayload.VIN;
            vehicle.condition = listingPayload.condition;
            vehicle.postDate = DateTime.Now.ToString();
            vehicle.country = _repositoryManager.getCountry(listingPayload.countryID).name;
            vehicle.countryID = listingPayload.countryID;
            vehicle.vehicleTypeId = listingPayload.vehicleType;
            vehicle.type = _repositoryManager.getVehicleType(listingPayload.vehicleType).type;
            vehicle.fuelTypeId = listingPayload.fuel;
            vehicle.fuel = _repositoryManager.getFuelType(listingPayload.fuel).fuel;
            vehicle.vehicleColorId = listingPayload.color;
            vehicle.color = _repositoryManager.getVehicleColor(listingPayload.color).color;
            vehicle.active = true;
            vehicle.modelID = listingPayload.modelID;
            vehicle.model = _repositoryManager.getModel(listingPayload.modelID).name;
            vehicle.makeID = listingPayload.makeID;
            vehicle.make = _repositoryManager.getMakeById(listingPayload.makeID).name;
            vehicle.cc = listingPayload.cc;
            vehicle.power = listingPayload.power;
            var vehicleid = _repositoryManager.addVehicle(vehicle);
            return vehicleid;

    }

    public void editListing(ListingPayload listingPayload, long vehicleID)
    {
        var vehicle = _repositoryManager.getVehicleById(vehicleID);
        if (vehicle == null) throw new Exception("Listing not found.");
        vehicle.description = listingPayload.description;
        vehicle.price = listingPayload.price;
        vehicle.km = listingPayload.km;
        vehicle.manufactureYear = listingPayload.manufactureYear;
        if (listingPayload.VIN != null) vehicle.VIN = listingPayload.VIN;
        vehicle.condition = listingPayload.condition;
        vehicle.postDate = DateTime.Now.ToString();
        vehicle.country = _repositoryManager.getCountry(listingPayload.countryID).name;
        vehicle.countryID = listingPayload.countryID;
        vehicle.vehicleTypeId = listingPayload.vehicleType;
        vehicle.type = _repositoryManager.getVehicleType(listingPayload.vehicleType).type;
        vehicle.fuelTypeId = listingPayload.fuel;
        vehicle.fuel = _repositoryManager.getFuelType(listingPayload.fuel).fuel;
        vehicle.vehicleColorId = listingPayload.color;
        vehicle.color = _repositoryManager.getVehicleColor(listingPayload.color).color;
        vehicle.modelID = listingPayload.modelID;
        vehicle.model = _repositoryManager.getModel(listingPayload.modelID).name;
        vehicle.makeID = listingPayload.makeID;
        vehicle.make = _repositoryManager.getMakeById(listingPayload.makeID).name;
        vehicle.cc = listingPayload.cc;
        vehicle.power = listingPayload.power;
        _db.SaveChanges();
    }

    public void buyPromoted(long vehicleID, int days, long userID)
    {
        var foundVehicle = _repositoryManager.getVehicleById(vehicleID);
        if (foundVehicle.UserID != userID) throw new Exception("You can't promote a vehicle that isn't yours.");
        var existing = _repositoryManager.getPromotedByVehicleId(vehicleID);
        if(existing != null)
        {
            existing.promotedUntil = existing.promotedUntil.AddDays(days);
        }
        else
        {
            var promoted = new Promoted();
            promoted.vehicle = foundVehicle;
            promoted.vehicleId = vehicleID;
            promoted.isPromoted = true;
            promoted.promotedUntil = DateTime.Now.AddDays(days);
            _repositoryManager.addPromoted(promoted);
        }
        _db.SaveChanges();
    }

    public void addFavorite(long vehicleID, long userID)
    {
        var foundUser = _repositoryManager.getUserById(userID);
        var foundvehicle = _repositoryManager.getVehicleById(vehicleID);
        if (foundvehicle.UserID == foundUser.Id) throw new Exception("You can't add to favorites your own vehicle.");
        Favorites fav = new Favorites();
        fav.UserId = userID;
        fav.user = foundUser;
        fav.VehicleId = vehicleID;
        fav.vehicle = foundvehicle;
        _repositoryManager.addFavorites(fav);
    }

    public void removeFavorite(long vehicleID, long userID)
    {
        var foundfav = _repositoryManager.getFavorite(vehicleID, userID);
        _repositoryManager.removeFavorite(foundfav);
    }

    public List<Vehicle> searchListings(SearchPayload searchPayload)
    {
        var vehicleQuery = _repositoryManager.getAllListings();
        if (searchPayload.priceMin != null) vehicleQuery = vehicleQuery.Where(v => v.price >= searchPayload.priceMin).ToList();
        if (searchPayload.priceMax != null) vehicleQuery = vehicleQuery.Where(v => v.price <= searchPayload.priceMax).ToList();
        if (searchPayload.kmMin != null) vehicleQuery = vehicleQuery.Where(v => v.km >= searchPayload.kmMin).ToList();
        if (searchPayload.kmMax != null) vehicleQuery = vehicleQuery.Where(v => v.km <= searchPayload.kmMax).ToList();
        if (searchPayload.manufactureYearMin != null) vehicleQuery = vehicleQuery.Where(v => v.manufactureYear >= searchPayload.manufactureYearMin).ToList();
        if (searchPayload.manufactureYearMax != null) vehicleQuery = vehicleQuery.Where(v => v.manufactureYear <= searchPayload.manufactureYearMax).ToList();
        if (searchPayload.condition != null) vehicleQuery = vehicleQuery.Where(v => v.condition.Equals(searchPayload.condition)).ToList();
        if (searchPayload.vehicleType != null) vehicleQuery = vehicleQuery.Where(v => v.vehicleTypeId == searchPayload.vehicleType).ToList();
        if (searchPayload.makeID != null) vehicleQuery = vehicleQuery.Where(v => v.makeID == searchPayload.makeID).ToList();
        if (searchPayload.modelID != null) vehicleQuery = vehicleQuery.Where(v => v.modelID == searchPayload.modelID).ToList();
        if (searchPayload.fuel != null) vehicleQuery = vehicleQuery.Where(v => v.fuelTypeId == searchPayload.fuel).ToList();
        if (searchPayload.color != null) vehicleQuery = vehicleQuery.Where(v => v.vehicleColorId == searchPayload.color).ToList();
        if (searchPayload.countryID != null) vehicleQuery = vehicleQuery.Where(v => v.countryID == searchPayload.countryID).ToList();
        return vehicleQuery;
    }

    public async void buyVehicle(string proof, long vehicleID, long userID)
    {
            var vehicle = _repositoryManager.getVehicleById(vehicleID);
            if (vehicle.active == false) throw new Exception("The listing is not active.");
            Sale s = new Sale();
            s.buyerID = userID;
            var founduser = _repositoryManager.getUserById(userID);
            var seller = _repositoryManager.getUserById(vehicle.UserID);
            s.buyer = founduser;
            s.proofOfPay = proof;
            s.vehicleID = vehicleID;
            s.vehicle = vehicle;
            s.finalprice = s.vehicle.price;
            
            vehicle.active = false;

            _repositoryManager.addSale(s);
            
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = "Congratulations, your vehicle " + "<a href='http://localhost:4200/details/" + vehicle.Id + "'>"  + vehicle.manufactureYear + " " + vehicle.make + " " + vehicle.model + "</a> has been sold" +
                           " for the price: " + vehicle.price + " euro. <br>" +
                           " Please contact the buyer " + founduser.Firstname + " " + founduser.Lastname + " on email at: " + founduser.Email + " or " +
                           " give him a phone call at " + founduser.Phonenumber + " in order to deliver the vehicle and for the necessary paperwork in maximum 48 hours." +
                           "<br> Thank you for using our web application."
            };

            await _mailService.sendEmail(seller.Firstname + " " + seller.Lastname, seller.Email, "Vehicle Sold",
                bodyBuilder);
            
            var bodyBuilder2 = new BodyBuilder
            {
                HtmlBody = "Congratulations, for your new purchase: " + "<a href='http://localhost:4200/details/" + vehicle.Id + "'>" + vehicle.manufactureYear + " " + vehicle.make + " " + vehicle.model + "</a> has been sold" +
                           " for the price: " + vehicle.price + " euro. <br>" +
                           " You will be contacted by the seller:  " + seller.Firstname + " " + seller.Lastname + "<br> Email of the seller: " + seller.Email + " <br> Phone number:  " +
                           founduser.Phonenumber + "<br> Thank you for using our web application."
            };

            await _mailService.sendEmail(founduser.Firstname + " " + founduser.Lastname, founduser.Email, "Vehicle Bought!",
                bodyBuilder2);
    }

    public void deleteListing(long vehicleID, long userID)
    {
        var vehicle = _repositoryManager.getVehicleById(vehicleID);
        var sold = _repositoryManager.getSaleByVehicle(vehicleID);
        if (sold != null) throw new Exception("Cannot delete a sold vehicle."); // cannot delete sold vehicle
        if (vehicle == null) throw new Exception("Vehicle not found"); // vehicle not found
        if (vehicle.UserID != userID) throw new Exception("You can only delete your own vehicle.") ; // can't delete another user's vehicle 
        var favorites = _repositoryManager.getFavoritesByVehicle(vehicleID);
        if(favorites != null) _repositoryManager.removeFavorites(favorites);
        var promoted = _repositoryManager.getPromotedByVehicleId(vehicleID);
        if(promoted != null) _repositoryManager.removePromoted(promoted);
        var pictures = _repositoryManager.getListingPhotos(vehicleID);
        if (pictures != null)
        {
            foreach (VehicleImage image in pictures)
            {
                _photoService.deletePhoto(image.filename);
                _repositoryManager.removeVehicleImage(image);
            }
        }

        
        _repositoryManager.removeVehicle(vehicle);
    }

    public string addListingPhoto(long listingID, int order, IFormFile file)
    {
        var uploadedImage =_photoService.addPhoto(file).Result;
        var image = new VehicleImage();
        image.vehicleID = listingID;
        image.vehicle = _repositoryManager.getVehicleById(listingID);
        image.order = order;
        image.imageURL = uploadedImage.blobUrl;
        image.filename = uploadedImage.systemFileName;
        _repositoryManager.addVehicleImage(image);
        if(order == 1)
        {
            image.vehicle.firstImage = uploadedImage.blobUrl;
        }
        _db.SaveChanges();

        return uploadedImage.blobUrl;
    }

    public string editPhoto(long listingID, int order, IFormFile file)
    {
        var uploadedImage = _photoService.addPhoto(file).Result;
        VehicleImage image = _repositoryManager.getVehicleImage(listingID, order);
        _photoService.deletePhoto(image.filename);
        image.vehicleID = listingID;
        image.vehicle = _repositoryManager.getVehicleById(listingID);
        image.order = order;
        image.filename = uploadedImage.systemFileName;
        image.imageURL = uploadedImage.blobUrl;
        if (order == 1)
        {
            image.vehicle.firstImage = uploadedImage.blobUrl;
        }
        _db.SaveChanges();

        return uploadedImage.blobUrl;
    }
    
}

