using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Entities.Models;
using Backend.Payloads;
using Microsoft.AspNetCore.Http;

namespace Backend.Services;

public interface IListingService
{
    void addMake(string name);
    void deleteMake(long makeID);
    void deleteModel(long modelID);
    void addModel(long makeID, string name);
    List<Vehicle> getFavorites(long userID);
    void refreshPromoted();
    List<Vehicle> getPromoted();
    long addListing(ListingPayload listingPayload, long userID);
    void editListing(ListingPayload listingPayload, long vehicleID);
    void buyPromoted(long vehicleID, int days, long userID);
    void addFavorite(long vehicleID, long userID);
    void removeFavorite(long vehicleID, long userID);
    List<Vehicle> searchListings(SearchPayload searchPayload);
    void buyVehicle(string proof, long vehicleID, long userID);
    void deleteListing(long vehicleID, long userID);
    string addListingPhoto(long listingID, int order, IFormFile file);
    string editPhoto(long listingID, int order, IFormFile file);
}