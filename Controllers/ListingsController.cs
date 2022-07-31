using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend.Entities;
using Backend.Entities.Models;
using Microsoft.Extensions.Configuration;
using Backend.Payloads;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Backend.Services;
using Microsoft.AspNetCore.Http;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListingsController : Controller
    {
        private IConfiguration _config { get; }
        private readonly BackendContext _db;
        private readonly IListingService _listingService;
        private readonly IRepositoryManager _repositoryManager;

        public ListingsController(BackendContext db, IConfiguration configuration, IListingService listingService, IRepositoryManager repositoryManager)
        {
            _config = configuration;
            _db = db;
            _listingService = listingService;
            _repositoryManager = repositoryManager;
        }

        protected int getUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        }

        [Authorize]
        [HttpPost("addListing")]
        public IActionResult addListing([FromBody] ListingPayload listingPayload)
        {
            try
            {
                long listingAdded = _listingService.addListing(listingPayload, getUserId());
                return new JsonResult(new { status = "Listing Added.", vehicleID = listingAdded, ok = "true" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPatch("editListing")]
        public IActionResult editListing([FromBody] ListingPayload listingPayload, long vehicleID)
        {
            try
            {
                _listingService.editListing(listingPayload, vehicleID);
                return new JsonResult(new { status = "Listing Edited.", ok = "true" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("buyPromoted")]
        public IActionResult buyPromoted(long vehicleID, int days)
        {
            try
            {
                _listingService.buyPromoted(vehicleID, days, getUserId());
                return new JsonResult(new { status = "Vehicle Promoted", ok = "true" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }
        
        [Authorize]
        [HttpGet("getRemainingListings")]
        public int getRemainingListings()
        {
            var availableListings = _repositoryManager.getRemainingListings(getUserId());
            return availableListings;
        }

        [HttpGet("isAvailable")]
        public IActionResult isAvailable(long vehicleID)
        {
            return new JsonResult(new { active = _repositoryManager.isAvailable(vehicleID) });
        }

        [Authorize]
        [HttpGet("isFavorite")]
        public IActionResult isFavorite(long vehicleID)
        {
            return new JsonResult(new { fav = _repositoryManager.isFavorite(vehicleID,getUserId()) });
        }
        [Authorize]
        [HttpPost("buyVehicle")]
        public IActionResult buyVehicle(string proof,long vehicleID)
        {
            try
            {
                _listingService.buyVehicle(proof, vehicleID, getUserId());
                return new JsonResult(new { status = "Vehicle bought!", ok = "true" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message});
            }
            
        }

        [HttpGet("getAllListings")]
        public IActionResult getAllListings()
        {
            var vehicles = _repositoryManager.getAllListings();
            return new JsonResult(new { array = vehicles  });
        }

        [HttpGet("getListing/{vehicleID}")]
        public Vehicle getListing([FromRoute] long vehicleID)
        {

            var veh = _repositoryManager.getListing(vehicleID);
            return veh;

        }

        [HttpGet("getUserListings/{userID}")]
        public IActionResult getUserListings([FromRoute] long userID)
        {
            var vehicles = _repositoryManager.getUserListings(userID);
            return new JsonResult(new { array = vehicles });

        }

        [HttpGet("getMyListings")]
        public IActionResult getMyListings()
        { 
            var vehicles = _repositoryManager.getUserListings(this.getUserId());
            return new JsonResult(new { array = vehicles });

        }

        [Authorize]
        [HttpPost("addFavorite")]
        public IActionResult addFavorite(long vehicleID)
        {
            try
            {
                _listingService.addFavorite(vehicleID, getUserId());
                return new JsonResult(new { status = "Added to favorites." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("removeFavorite")]
        public IActionResult removeFavorite(long vehicleID)
        {
            _listingService.removeFavorite(vehicleID,getUserId());
            return new JsonResult(new { status = "Favorite vehicle removed." });
        }


        [Authorize]
        [HttpGet("getFavorites")]
        public IActionResult getFavorites()
        {
            var favorites = _listingService.getFavorites(getUserId());
            return new JsonResult(new { array = favorites });
        }

        [HttpGet("getTotalListings")]
        public int getTotalListings()
        {
            return _repositoryManager.getTotalListings();
        }


        [HttpPost("searchListings")]
        public IActionResult searchListings([FromBody] SearchPayload searchPayload)
        {
            var results = _listingService.searchListings(searchPayload);
            return new JsonResult(new { array = results });
        }

        [Authorize]
        [HttpPost("addPhoto")]
        public async Task<IActionResult> addPhoto(long listingID,int order)
        {
            IFormFile file = Request.Form.Files[0];
            var url = _listingService.addListingPhoto(listingID, order, file);
            return new JsonResult(new { status = "ok", uri = url });
        }

        [Authorize]
        [HttpDelete("deleteListing")]
        public IActionResult deleteListing(long vehicleID)
        {
            try
            {
                _listingService.deleteListing(vehicleID, getUserId());
                return new JsonResult(new { status = "Vehicle Removed" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }
        [Authorize]
        [HttpPatch("editPhoto")]
        public async Task<IActionResult> editPhoto(long listingID, int order)
        {
            IFormFile file = Request.Form.Files[0];
            var url = _listingService.editPhoto(listingID, order, file);
            return new JsonResult(new { status = "ok", uri = url });
        }
        [Authorize(Roles = Role.Admin)]
        [HttpPost("addMake")]
        public IActionResult addMake(string name)
        {
            try
            {
                _listingService.addMake(name);
                return new JsonResult(new { status = "Make added" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        [Authorize(Roles = Role.Admin)]
        [HttpDelete("deleteMake")]
        public IActionResult deleteMake(long makeID)
        {
            try
            {
                _listingService.deleteMake(makeID);
                return new JsonResult(new { status = "Manufacturer deleted." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = Role.Admin)]
        [HttpDelete("deleteModel")]
        public IActionResult deleteModel(long modelID)
        {
            try
            {
                _listingService.deleteModel(modelID);
                return new JsonResult(new { status = "Model deleted." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPost("addModel")]
        public IActionResult addModel(long makeID,string name)
        {
            try
            {
                _listingService.addModel(makeID, name);
                return new JsonResult(new { status = "Model added" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("getListingPhotos")]
        public IActionResult getListingPhotos(long listingID)
        {
            var photos = _repositoryManager.getListingPhotos(listingID);
            return new JsonResult(new { array = photos });
        }

        [HttpGet("getColors")]
        public IActionResult getColors()
        {

            var colors = _repositoryManager.getColors();
            return new JsonResult(new { array = colors });

        }

        [HttpGet("getFuelTypes")]
        public IActionResult getFuelTypes()
        {

            var fueltypes = _repositoryManager.GetFuelTypes();
            return new JsonResult(new { array = fueltypes });

        }

        [HttpGet("getCountries")]
        public IActionResult getCountries()
        {
            var countries = _repositoryManager.getCountries();
            return new JsonResult(new { array = countries });

        }

        [HttpGet("getTypes")]
        public IActionResult getTypes()
        {
            var types = _repositoryManager.getTypes();
            return new JsonResult(new { array = types });

        }

        [HttpGet("getMakes")]
        public IActionResult getMakes()
        {
            var makes = _repositoryManager.getMakes();
            return new JsonResult(new { array = makes });

        }
        [HttpGet("getModels")]
        public IActionResult getModels(long makeID)
        {
            var models = _repositoryManager.getModels(makeID);
            return new JsonResult(new { array = models });

        }
        [HttpGet("getPromoted")]
        public IActionResult getPromoted()
        {
            var promotedVehicles = _listingService.getPromoted();
            return new JsonResult(new { array = promotedVehicles });
        }

    }
}
