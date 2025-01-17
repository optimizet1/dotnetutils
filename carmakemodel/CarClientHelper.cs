using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class CarClientHelper
{
    private List<CarMakeDto> _carData;

    public CarClientHelper(string jsonResponse)
    {
        _carData = JsonConvert.DeserializeObject<List<CarMakeDto>>(jsonResponse);
    }

    public List<string> GetAllMakes()
    {
        return _carData.Select(make => make.MakeName).ToList();
    }

    public List<string> GetModelsByMake(string makeName)
    {
        var make = _carData.FirstOrDefault(m => m.MakeName.Equals(makeName, StringComparison.OrdinalIgnoreCase));
        return make?.Models.Select(model => model.ModelName).ToList() ?? new List<string>();
    }
}

public class CarMakeDto
{
    public int MakeId { get; set; }
    public string MakeName { get; set; }
    public List<CarModelDto> Models { get; set; }
}

public class CarModelDto
{
    public int ModelId { get; set; }
    public string ModelName { get; set; }
}
