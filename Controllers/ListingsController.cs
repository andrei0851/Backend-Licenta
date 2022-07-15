using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Entities;
using Backend.Entities.Models;
using Microsoft.Extensions.Configuration;
using Backend.Payloads;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MimeKit;
using MailKit.Security;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListingsController : Controller
    {
        private IConfiguration _config { get; }
        private readonly BackendContext _db;

        public ListingsController(BackendContext db, IConfiguration configuration)
        {
            _config = configuration;
            _db = db;
        }

        protected int getUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        }

        [Authorize]
        [HttpPost("addListing")]
        public IActionResult addListing([FromBody] ListingPayload listingPayload)
        {
            var foundUser = _db.Users.Where(u => u.Id == getUserId()).SingleOrDefault();
            if(foundUser.Role == "User")
            {
                if(foundUser.availableListings < 1) return BadRequest(new { message = "You have 0 listings available.", status = false });
                foundUser.availableListings--;
            }
            var vehicle = new Vehicle();
            vehicle.UserID = getUserId();
            vehicle.user = foundUser.Lastname + " " + foundUser.Firstname;
            vehicle.description = listingPayload.description;
            vehicle.price = listingPayload.price;
            vehicle.km = listingPayload.km;
            vehicle.manufactureYear = listingPayload.manufactureYear;
            if (listingPayload.VIN != null) vehicle.VIN = listingPayload.VIN;
            vehicle.condition = listingPayload.condition;
            vehicle.postDate = DateTime.Now.ToString();
            vehicle.country = _db.Countries.Where(c => c.countryID == listingPayload.countryID).SingleOrDefault().name;
            vehicle.countryID = listingPayload.countryID;
            vehicle.vehicleTypeId = listingPayload.vehicleType;
            vehicle.type = _db.VehicleType.Where(vt => vt.Id == listingPayload.vehicleType).SingleOrDefault().type;
            vehicle.fuelTypeId = listingPayload.fuel;
            vehicle.fuel = _db.FuelType.Where(ft => ft.Id == listingPayload.fuel).SingleOrDefault().fuel;
            vehicle.vehicleColorId = listingPayload.color;
            vehicle.color = _db.VehicleColor.Where(vc => vc.Id == listingPayload.color).SingleOrDefault().color;
            vehicle.active = true;
            vehicle.modelID = listingPayload.modelID;
            vehicle.model = _db.Models.Where(m => m.Id == listingPayload.modelID).SingleOrDefault().name;
            vehicle.makeID = listingPayload.makeID;
            vehicle.make = _db.Makes.Where(m => m.Id == listingPayload.makeID).SingleOrDefault().name;
            vehicle.cc = listingPayload.cc;
            vehicle.power = listingPayload.power;


            _db.Vehicles.Add(vehicle);
            _db.SaveChanges();



            return new JsonResult(new { status = "Listing Added.", vehicleID = vehicle.Id, ok = "true" });

        }

        [Authorize]
        [HttpGet("getRemainingListings")]
        public int getRemainingListings()
        {
            return _db.Users.Where(u => u.Id == getUserId()).SingleOrDefault().availableListings;
        }

        [Authorize]
        [HttpPatch("editListing")]
        public IActionResult editListing([FromBody] ListingPayload listingPayload, long vehicleID)
        {
            var foundUser = _db.Users.Where(u => u.Id == getUserId()).SingleOrDefault();
            var vehicle = _db.Vehicles.Where(v => v.Id == vehicleID).SingleOrDefault();
            if(vehicle == null) return BadRequest(new { message = "Vehicle not found", status = false });
            vehicle.UserID = getUserId();
            vehicle.user = foundUser.Lastname + " " + foundUser.Firstname;
            vehicle.description = listingPayload.description;
            vehicle.price = listingPayload.price;
            vehicle.km = listingPayload.km;
            vehicle.manufactureYear = listingPayload.manufactureYear;
            if (listingPayload.VIN != null) vehicle.VIN = listingPayload.VIN;
            vehicle.condition = listingPayload.condition;
            vehicle.postDate = DateTime.Now.ToString();
            vehicle.country = _db.Countries.Where(c => c.countryID == listingPayload.countryID).SingleOrDefault().name;
            vehicle.countryID = listingPayload.countryID;
            vehicle.vehicleTypeId = listingPayload.vehicleType;
            vehicle.type = _db.VehicleType.Where(vt => vt.Id == listingPayload.vehicleType).SingleOrDefault().type;
            vehicle.fuelTypeId = listingPayload.fuel;
            vehicle.fuel = _db.FuelType.Where(ft => ft.Id == listingPayload.fuel).SingleOrDefault().fuel;
            vehicle.vehicleColorId = listingPayload.color;
            vehicle.color = _db.VehicleColor.Where(vc => vc.Id == listingPayload.color).SingleOrDefault().color;
            var sold = _db.Sales.Where(s => s.vehicleID == vehicleID).SingleOrDefault();
            vehicle.modelID = listingPayload.modelID;
            vehicle.model = _db.Models.Where(m => m.Id == listingPayload.modelID).SingleOrDefault().name;
            vehicle.makeID = listingPayload.makeID;
            vehicle.make = _db.Makes.Where(m => m.Id == listingPayload.makeID).SingleOrDefault().name;
            vehicle.cc = listingPayload.cc;
            vehicle.power = listingPayload.power;

            _db.SaveChanges();



            return new JsonResult(new { status = "Listing Edited.", ok = "true" });

        }

        [Authorize]
        [HttpPost("buyPromoted")]
        public IActionResult buyPromoted(long vehicleID, int days)
        {
            var foundVehicle = _db.Vehicles.Where(v => v.Id == vehicleID).SingleOrDefault();
            if (foundVehicle.UserID != getUserId())
            {
                return new JsonResult(new { status = "You can't promote a vehcile that isn't yours." });
            }
            var existing = _db.Promoted.Where(p => p.vehicleId == vehicleID).SingleOrDefault();
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
                _db.Promoted.Add(promoted);
            }
            _db.SaveChanges();
            return new JsonResult(new { status = "Vehicle Promoted", ok = "true" });

        }

        [HttpGet("isAvailable")]
        public IActionResult isAvailable(long vehicleID)
        {
            var foundvehicle = _db.Vehicles.Where(v => v.Id == vehicleID).SingleOrDefault();
            if (foundvehicle.active == true) return new JsonResult(new { active = true });
            else return new JsonResult(new { active = false });
        }

        [Authorize]
        [HttpGet("isFavorite")]
        public IActionResult isFavorite(long vehicleID)
        {
            var query = _db.Favorites.Where(v => v.VehicleId == vehicleID).Where(v => v.UserId == this.getUserId()).SingleOrDefault();
            if (query == null) return new JsonResult(new { fav = false });
            else return new JsonResult(new { fav = true });
        }
        [Authorize]
        [HttpPost("buyVehicle")]
        public async Task<IActionResult> buyVehicle(string proof,long vehicleID)
        {
            if (_db.Vehicles.Where(v => v.Id == vehicleID).SingleOrDefault().active == false)
            {
                return new JsonResult(new { status = "The listing is not active.", ok = "false" });
            }
            Sale s = new Sale();
            s.buyerID = getUserId();
            var vehicle = _db.Vehicles.Where(v => v.Id == vehicleID).SingleOrDefault();
            var founduser = _db.Users.Where(u => u.Id == getUserId()).SingleOrDefault();
            var seller = _db.Users.Where(user => user.Id == vehicle.UserID).SingleOrDefault();
            s.buyer = founduser;
            s.proofOfPay = proof;
            s.vehicleID = vehicleID;
            s.vehicle = vehicle;
            s.finalprice = s.vehicle.price;

            var foundvehicle = _db.Vehicles.Where(v => v.Id == vehicleID).SingleOrDefault();
            foundvehicle.active = false;

            _db.Sales.Add(s);

            _db.SaveChanges();

            using var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                "WorldWide Cars Trade",
                "ww.cars.trade@gmail.com"
            ));
            message.To.Add(new MailboxAddress(
                seller.Firstname + " " + seller.Lastname,
                seller.Email
            ));
            message.Subject = "Vehicle Sold!";
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = "Congratulations, your vehicle " + "<a href='http://localhost:4200/details/" + vehicle.Id + "'>"  + vehicle.manufactureYear + " " + vehicle.make + " " + vehicle.model + "</a> has been sold" +
                " for the price: " + vehicle.price + " euro. <br>" +
                " Please contact the buyer " + founduser.Firstname + " " + founduser.Lastname + " on email at: " + founduser.Email + " or " +
                " give him a phone call at " + founduser.Phonenumber + " in order to deliver the vehicle and for the necessary paperwork in maximum 48 hours." +
                "<br> Thank you for using our web application."
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();
            // SecureSocketOptions.StartTls force a secure connection over TLS
            await client.ConnectAsync("smtp.sendgrid.net", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(
                userName: "apikey", // the userName is the exact string "apikey" and not the API key itself.
                password: "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" // password is the API key
            );
            await client.SendAsync(message);

            using var message2 = new MimeMessage();
            message2.From.Add(new MailboxAddress(
                "WorldWide Cars Trade",
                "ww.cars.trade@gmail.com"
            ));
            message2.To.Add(new MailboxAddress(
                founduser.Firstname + " " + founduser.Lastname,
                founduser.Email
            ));
            message2.Subject = "Vehicle Bought!";
            var bodyBuilder2 = new BodyBuilder
            {
                HtmlBody = "Congratulations, for your new purchase: " + "<a href='http://localhost:4200/details/" + vehicle.Id + "'>" + vehicle.manufactureYear + " " + vehicle.make + " " + vehicle.model + "</a> has been sold" +
                " for the price: " + vehicle.price + " euro. <br>" +
                " You will be contacted by the seller:  " + seller.Firstname + " " + seller.Lastname + "<br> Email of the seller: " + seller.Email + " <br> Phone number:  " +
                founduser.Phonenumber + "<br> Thank you for using our web application."
            };
            message2.Body = bodyBuilder.ToMessageBody();

            await client.SendAsync(message2);

            await client.DisconnectAsync(true);

            return new JsonResult(new { status = "Vehicle bought!", ok = "true" });

        }

        [HttpGet("getAllListings")]
        public IActionResult getAllListings()
        {
            var vehicleQuery = _db.Vehicles.AsNoTracking().Where(v => v.active == true);
            var result = vehicleQuery.ToList();
            return new JsonResult(new { array = result });
        }

        [HttpGet("getListing/{vehicleID}")]
        public Vehicle getListing([FromRoute] long vehicleID)
        {

            var vehicleQuery = _db.Vehicles.AsNoTracking();
            var veh = vehicleQuery.Where(v => v.Id == vehicleID).SingleOrDefault();
            return veh;

        }

        [HttpGet("getUserListings/{userID}")]
        public IActionResult getUserListings([FromRoute] long userID)
        {
            var vehicleQuery = _db.Vehicles.Where(v => v.UserID == userID).AsNoTracking();
            var result = vehicleQuery.ToList();
            return new JsonResult(new { array = result });

        }

        [HttpGet("getMyListings")]
        public IActionResult getMyListings()
        {
            var vehicleQuery = _db.Vehicles.Where(v => v.UserID == getUserId()).AsNoTracking();
            var result = vehicleQuery.ToList();
            return new JsonResult(new { array = result });

        }

        [Authorize]
        [HttpPost("addFavorite")]
        public IActionResult addFavorite(long vehicleID)
        {
            var foundUser = _db.Users.Where(u => u.Id == getUserId()).SingleOrDefault();
            var foundvehicle = _db.Vehicles.Where(v => v.Id  == vehicleID).SingleOrDefault();
            if (foundvehicle.UserID == foundUser.Id) return BadRequest(new { message = "You can't add to favorites your own vehicle." });
            Favorites fav = new Favorites();
            fav.UserId = getUserId();
            fav.user = foundUser;
            fav.VehicleId = vehicleID;
            fav.vehicle = foundvehicle;
            _db.Favorites.Add(fav);
            _db.SaveChanges();
            return new JsonResult(new { status = "Added to favorites." });
        }

        [Authorize]
        [HttpDelete("removeFavorite")]
        public IActionResult removeFavorite(long vehicleID)
        {
            var foundfav = _db.Favorites.Where(v => v.VehicleId == vehicleID).Where(u => u.UserId == this.getUserId()).SingleOrDefault();
            _db.Favorites.Remove(foundfav);
            _db.SaveChanges();
            return new JsonResult(new { status = "Favorite vehicle removed." });
        }


        [Authorize]
        [HttpGet("getFavorites")]
        public IActionResult getFavorites()
        {
            List<Vehicle> favorites = new List<Vehicle>();
            var favoritesQuery = _db.Favorites.Where(u => u.UserId == getUserId()).AsNoTracking();
            foreach(Favorites f in favoritesQuery)
            {
                Vehicle fav = _db.Vehicles.Where(v => v.Id == f.VehicleId).SingleOrDefault();
                favorites.Add(fav);
            }
            return new JsonResult(new { array = favorites.ToList() });
        }

        [HttpGet("getTotalListings")]
        public int getTotalListings()
        {
            return _db.Vehicles.Where(v => v.active == true).AsNoTracking().Count();
        }


        [HttpPost("searchListings")]
        public IActionResult searchListings([FromBody] SearchPayload searchPayload)
        {
            var vehicleQuery = _db.Vehicles.AsNoTracking().Where(v => v.active == true);
            if (searchPayload.priceMin != null) vehicleQuery = vehicleQuery.Where(v => v.price >= searchPayload.priceMin).AsNoTracking();
            if (searchPayload.priceMax != null) vehicleQuery = vehicleQuery.Where(v => v.price <= searchPayload.priceMax).AsNoTracking();
            if (searchPayload.kmMin != null) vehicleQuery = vehicleQuery.Where(v => v.km >= searchPayload.kmMin).AsNoTracking();
            if (searchPayload.kmMax != null) vehicleQuery = vehicleQuery.Where(v => v.km <= searchPayload.kmMax).AsNoTracking();
            if (searchPayload.manufactureYearMin != null) vehicleQuery = vehicleQuery.Where(v => v.manufactureYear >= searchPayload.manufactureYearMin).AsNoTracking();
            if (searchPayload.manufactureYearMax != null) vehicleQuery = vehicleQuery.Where(v => v.manufactureYear <= searchPayload.manufactureYearMax).AsNoTracking();
            if (searchPayload.condition != null) vehicleQuery = vehicleQuery.Where(v => v.condition.Equals(searchPayload.condition)).AsNoTracking();
            if (searchPayload.vehicleType != null) vehicleQuery = vehicleQuery.Where(v => v.vehicleTypeId == searchPayload.vehicleType);
            if (searchPayload.makeID != null) vehicleQuery = vehicleQuery.Where(v => v.makeID == searchPayload.makeID);
            if (searchPayload.modelID != null) vehicleQuery = vehicleQuery.Where(v => v.modelID == searchPayload.modelID);
            if (searchPayload.fuel != null) vehicleQuery = vehicleQuery.Where(v => v.fuelTypeId == searchPayload.fuel);
            if (searchPayload.color != null) vehicleQuery = vehicleQuery.Where(v => v.vehicleColorId == searchPayload.color);
            if (searchPayload.countryID != null) vehicleQuery = vehicleQuery.Where(v => v.countryID == searchPayload.countryID);

            return new JsonResult(new { array = vehicleQuery.ToList() });
        }

        [Authorize]
        [HttpPost("addPhoto")]
        public async Task<IActionResult> addPhoto(long listingID,int order)
        {
            IFormFile file = Request.Form.Files[0];
            string systemFileName = Guid.NewGuid().ToString();
            string blobstorageconnection = _config["ConnectionStrings:AzureConnectionString"];
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(blobstorageconnection);
            CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("firstcontainer");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(systemFileName);
            await using (var data = file.OpenReadStream())
            {
                await blockBlob.UploadFromStreamAsync(data);
            }
            var blobUrl = blockBlob.Uri.AbsoluteUri;
            VehicleImage image = new VehicleImage();
            image.vehicleID = listingID;
            image.vehicle = _db.Vehicles.Where(v => v.Id == listingID).SingleOrDefault();
            image.order = order;
            image.imageURL = blobUrl;
            image.filename = systemFileName;
            _db.VehicleImages.Add(image);
            if(order == 1)
            {
                image.vehicle.firstImage = blobUrl;
            }
            _db.SaveChanges();

            return new JsonResult(new { status = "ok", uri = blobUrl });



        }

        [Authorize]
        [HttpDelete("deleteListing")]
        public IActionResult deleteListing(long vehicleID)
        {
            var vehicle = _db.Vehicles.Where(v => v.Id == vehicleID).SingleOrDefault();
            var sold = _db.Sales.Where(v => v.vehicleID == vehicleID).SingleOrDefault();
            if(sold != null) return BadRequest(new { message = "You cannot delete a sold vehicle." });
            if (vehicle == null) return BadRequest(new { message = "Vehicle not found" });
            if(vehicle.UserID != getUserId()) return BadRequest(new { message = "You can only delete your own vehicle." });
            var favorites = _db.Favorites.Where(v => v.Id == vehicleID).AsNoTracking();
            if(favorites != null) _db.Favorites.RemoveRange(favorites);
            var promoted = _db.Promoted.Where(v => v.vehicleId == vehicleID).SingleOrDefault();
           if(promoted != null) _db.Promoted.Remove(promoted);
            var pictures = _db.VehicleImages.Where(i => i.vehicleID == vehicleID).AsNoTracking();
            if (pictures != null)
            {
                foreach (VehicleImage image in pictures)
                {
                    deletePhotoFromCloud(image.filename);
                    _db.VehicleImages.Remove(image);
                }
            }
            _db.Vehicles.Remove(vehicle);
            _db.SaveChanges();

            return new JsonResult(new { status = "Vehicle Removed" });
        }

        private void deletePhotoFromCloud(string filename)
        {
            string blobstorageconnection = _config["ConnectionStrings:AzureConnectionString"];
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(blobstorageconnection);
            CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("firstcontainer");
            var blob = container.GetBlockBlobReference(filename);
            blob.DeleteIfExistsAsync();
        }

        [Authorize]
        [HttpPatch("editPhoto")]
        public async Task<IActionResult> editPhoto(long listingID, int order)
        {
            IFormFile file = Request.Form.Files[0];
            string systemFileName = Guid.NewGuid().ToString();
            string blobstorageconnection = _config["ConnectionStrings:AzureConnectionString"];
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(blobstorageconnection);
            CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("firstcontainer");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(systemFileName);
            await using (var data = file.OpenReadStream())
            {
                await blockBlob.UploadFromStreamAsync(data);
            }
            var blobUrl = blockBlob.Uri.AbsoluteUri;
            VehicleImage image = _db.VehicleImages.Where(i => i.vehicleID == listingID).Where(i => i.order == order).SingleOrDefault();
            deletePhotoFromCloud(image.filename);
            image.vehicleID = listingID;
            image.vehicle = _db.Vehicles.Where(v => v.Id == listingID).SingleOrDefault();
            image.order = order;
            image.filename = systemFileName;
            image.imageURL = blobUrl;
            if (order == 1)
            {
                image.vehicle.firstImage = blobUrl;
            }
            _db.SaveChanges();

            return new JsonResult(new { status = "ok", uri = blobUrl });



        }
        [Authorize(Roles = Role.Admin)]
        [HttpPost("addMake")]
        public IActionResult addMake(string name)
        {
            var existent = _db.Makes.Where(m => m.name.ToLower() == name.ToLower()).SingleOrDefault();
            if (existent != null) return BadRequest(new { message = "Manufacturer already exists."});
            Make m = new Make();
            m.name = name;
            _db.Makes.Add(m);
            _db.SaveChanges();
            return new JsonResult(new { status = "Make added" });
        }

        [Authorize(Roles = Role.Admin)]
        [HttpDelete("deleteMake")]
        public IActionResult deleteMake(long makeID)
        {
            var make = _db.Makes.Where(m => m.Id == makeID).SingleOrDefault();
            if (make == null) return BadRequest(new { message = "Manufacturer doesn't exist." });
            var cars = _db.Vehicles.Where(v => v.makeID == makeID).SingleOrDefault();
            if (cars != null) return BadRequest(new { message = "Manufacturer has existing listings." });
            else
            {
                var models = _db.Models.Where(m => m.makeID == makeID).AsNoTracking();
                _db.Models.RemoveRange(models);
                _db.Makes.Remove(make);
                _db.SaveChanges();
            }
            return new JsonResult(new { status = "Manufacturer deleted." });
        }

        [Authorize(Roles = Role.Admin)]
        [HttpDelete("deleteModel")]
        public IActionResult deleteModel(long modelID)
        {
            var model = _db.Models.Where(m => m.Id == modelID).SingleOrDefault();
            if(model == null) return BadRequest(new { message = "Model doesn't exist." });
            var cars = _db.Vehicles.Where(v => v.modelID == modelID).SingleOrDefault();
            if(cars != null) return BadRequest(new { message = "Model has existing listings." });
            else
            {
                _db.Models.Remove(model);
                _db.SaveChanges();
            }
            return new JsonResult(new { status = "Model deleted." });
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPost("addModel")]
        public IActionResult addModel(long makeID,string name)
        {
            var existent = _db.Models.Where(m => m.name.ToLower() == name.ToLower() && m.makeID == makeID).SingleOrDefault();
            if (existent != null) return BadRequest(new { message = "Model already exists for this manufacturer." });
            Model m = new Model();
            m.makeID = makeID;
            m.name = name;
            _db.Models.Add(m);
            _db.SaveChanges();
            return new JsonResult(new { status = "Model added" });
        }

        [HttpGet("getListingPhotos")]
        public IActionResult getListingPhotos(long listingID)
        {
            var query = _db.VehicleImages.Where(i => i.vehicleID == listingID).AsNoTracking();
            var result = query.ToList();
            return new JsonResult(new { array = result });
        }

        [HttpGet("getColors")]
        public IActionResult getColors()
        {

            var query = _db.VehicleColor.AsNoTracking();
            var result = query.ToList();
            return new JsonResult(new { array = result });

        }

        [HttpGet("getFuelTypes")]
        public IActionResult getFuelTypes()
        {

            var query = _db.FuelType.AsNoTracking();
            var results = query.ToList();
            return new JsonResult(new { array = results });

        }

        [HttpGet("getCountries")]
        public IActionResult getCountries()
        {
            var query = _db.Countries.AsNoTracking();
            var result = query.ToList();
            return new JsonResult(new { array = result });

        }

        [HttpGet("getTypes")]
        public IActionResult getTypes()
        {

            var query = _db.VehicleType.AsNoTracking();
            var results = query.ToList();
            return new JsonResult(new { array = results });

        }

        [HttpGet("getMakes")]
        public IActionResult getMakes()
        {

            var query = _db.Makes.AsNoTracking();
            var results = query.ToList();
            results = results.OrderBy(o => o.name).ToList();
            return new JsonResult(new { array = results });

        }
        [HttpGet("getModels")]
        public IActionResult getModels(long makeID)
        {

            var query = _db.Models.AsNoTracking();
            query = query.Where(m => m.makeID == makeID);
            var results = query.ToList();
            results = results.OrderBy(o => o.name).ToList();
            return new JsonResult(new { array = results });

        }

        public void refreshPromoted()
        {
            var queryPromoted = _db.Promoted.AsNoTracking().Where(p => p.promotedUntil < DateTime.Now);
            foreach(Promoted p in queryPromoted)
            {
                p.isPromoted = false;
            }
            _db.SaveChanges();
        }


        [HttpGet("getPromoted")]
        public IActionResult getPromoted()
        {
            this.refreshPromoted();
            List<Vehicle> promotedVehicles = new List<Vehicle>();
            var query = _db.Promoted.AsNoTracking().Where(p => p.isPromoted == true && p.promotedUntil > DateTime.Now);
            foreach(Promoted prom in query)
            {
                Vehicle promoted = _db.Vehicles.Where(v => v.Id == prom.vehicleId).SingleOrDefault();
                if (promoted.active == true)
                {
                    promotedVehicles.Add(promoted);
                }
            }
            return new JsonResult(new { array = promotedVehicles });

        }

    }
}
