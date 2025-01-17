public class Program
{
    public static void Main(string[] args)
    {
        // Sample JSON response (replace with API call result in a real application)
        string jsonResponse = @"
        [
            {
                'MakeId': 1,
                'MakeName': 'Toyota',
                'Models': [
                    { 'ModelId': 1, 'ModelName': 'Corolla' },
                    { 'ModelId': 2, 'ModelName': 'Camry' }
                ]
            },
            {
                'MakeId': 2,
                'MakeName': 'Ford',
                'Models': [
                    { 'ModelId': 3, 'ModelName': 'Focus' },
                    { 'ModelId': 4, 'ModelName': 'Mustang' }
                ]
            }
        ]";

        var clientHelper = new CarClientHelper(jsonResponse);

        Console.WriteLine("All Makes:");
        foreach (var make in clientHelper.GetAllMakes())
        {
            Console.WriteLine(make);
        }

        Console.WriteLine("\nModels for 'Toyota':");
        foreach (var model in clientHelper.GetModelsByMake("Toyota"))
        {
            Console.WriteLine(model);
        }
    }
}
