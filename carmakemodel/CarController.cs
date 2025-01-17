using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace CarAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarController : ControllerBase
    {
        // Mock database tables
        private static readonly List<CarMake> CarMakes = new()
        {
            new CarMake { Id = 1, Name = "Toyota" },
            new CarMake { Id = 2, Name = "Ford" },
            new CarMake { Id = 3, Name = "Tesla" }
        };

        private static readonly List<CarModel> CarModels = new()
        {
            new CarModel { Id = 1, CarMakeId = 1, Name = "Corolla" },
            new CarModel { Id = 2, CarMakeId = 1, Name = "Camry" },
            new CarModel { Id = 3, CarMakeId = 2, Name = "Focus" },
            new CarModel { Id = 4, CarMakeId = 2, Name = "Mustang" },
            new CarModel { Id = 5, CarMakeId = 3, Name = "Model S" },
            new CarModel { Id = 6, CarMakeId = 3, Name = "Model 3" }
        };

        [HttpGet]
        public IActionResult GetCarMakesAndModels()
        {
            var result = CarMakes
                .Select(make => new
                {
                    MakeId = make.Id,
                    MakeName = make.Name,
                    Models = CarModels
                        .Where(model => model.CarMakeId == make.Id)
                        .Select(model => new
                        {
                            ModelId = model.Id,
                            ModelName = model.Name
                        })
                        .ToList()
                })
                .ToList();

            return Ok(result);
        }
    }

    public class CarMake
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CarModel
    {
        public int Id { get; set; }
        public int CarMakeId { get; set; }
        public string Name { get; set; }
    }
}
