using CarInformation.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.VisualBasic.FileIO;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CarInformation.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class Vehicles : ControllerBase
    {
        private readonly string ErrorMessage = "An error occurred, please try again later";
        public Vehicles()
        {

        }

        [HttpGet]
        public IActionResult Welcoming()
        {

            return Ok("Welcome to car information website");
        }



        [HttpGet]
        public async Task<IActionResult> GetModels(string modelYear, string make)
        {

            if (string.IsNullOrEmpty(modelYear) || string.IsNullOrEmpty(make))
                return BadRequest("Please pass the required values (modelYear, make)");

            try
            {

                IsAllDigits(modelYear);
                string URL = $"https://vpic.nhtsa.dot.gov/api/vehicles/GetModelsForMakeIdYear/makeId/{GetMakerIdByMakeName(make)}/modelyear/{modelYear}?format=json";
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, URL);
                HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult();

                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    ResponseModel result = JsonSerializer.Deserialize<ResponseModel>(await response.Content.ReadAsStringAsync());

                    if (result.Results != null)
                        return Ok(new { Models = result.Results.Select(c => c.Model_Name).ToList() });
                }
                else
                    throw new Exception(ErrorMessage);

            }
            catch (Exception ex)
            {
                return BadRequest(string.IsNullOrEmpty(ex.Message) ? ErrorMessage : ex.Message);
            }

            return default;
        }

        private int GetMakerIdByMakeName(string makerName)
        {

            using (TextFieldParser parser = new TextFieldParser("Makers\\CarMake.csv"))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                string filteredData = (from line in System.IO.File.ReadLines("Makers\\CarMake.csv")
                                       let data = line.Split(',')
                                       where data[1].Trim().ToLowerInvariant().Equals(makerName.Trim().ToLowerInvariant())
                                       select data[0]).FirstOrDefault();
                if (filteredData != null)
                    return Convert.ToInt32(filteredData);
                else
                    throw new Exception("We don't have information for this car");

            }
        }

        bool IsAllDigits(string year)
        {
            bool isMatch = Regex.IsMatch(year, @"^\d+$");
            if (!isMatch)
                throw new Exception("Model year cannot contain any letters, only numbers");
            else return isMatch;
        }

    }
}
