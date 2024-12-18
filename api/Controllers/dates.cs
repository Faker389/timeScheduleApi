using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class dates : Controller
    {
        private readonly FirestoreDb _firestoreDb;

        public dates()
        {
            string credentialPath = "./Controllers/service.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);
            _firestoreDb = FirestoreDb.Create("netapp-fde3c");
        }

        [HttpPost]
        public async Task<IActionResult> AddData([FromBody] YourDataModel data)
        {
            if (data == null || string.IsNullOrEmpty(data.userID))
            {
                return BadRequest("Invalid data or missing user ID");
            }

            try
            {
                // Access the main "Dates" collection
                CollectionReference mainCollection = _firestoreDb.Collection("Dates");

                // Access or create a subcollection with the userID
                CollectionReference userCollection = mainCollection.Document(data.userID).Collection("UserEntries");

                // Generate a new random ID for the document
                var newDocument = new
                {
                    day = data.day,
                    time = data.time,
                    name = data.name,
                    description = data.description,
                    importance = data.importance
                };

                // Add the data to the user's subcollection
                DocumentReference document = await userCollection.AddAsync(newDocument);

                return Ok(new { DocumentId = document.Id, Message = "Data added successfully" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

[HttpGet("get-dates")]
    public async Task<IActionResult> GetDates(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User ID is required.");
        }

        try
        {
            // Access the main "Dates" collection
            CollectionReference mainCollection = _firestoreDb.Collection("Dates");

            // Access the user's subcollection
            CollectionReference userCollection = mainCollection.Document(userId).Collection("UserEntries");

            // Retrieve all documents from the user's subcollection
            QuerySnapshot snapshot = await userCollection.GetSnapshotAsync();

            // Map documents to a list of `YourDataModel`
            var dataList = snapshot.Documents
                .Select(doc => new YourDataModel
                {
                    day = doc.ContainsField("day") ? doc.GetValue<string>("day") : null,
                    time = doc.ContainsField("time") ? doc.GetValue<string>("time") : null,
                    name = doc.ContainsField("name") ? doc.GetValue<string>("name") : null,
                    importance = doc.ContainsField("importance")? doc.GetValue<string>("importance"):null,
                    description = doc.ContainsField("description") ? doc.GetValue<string>("description") : null,
                    userID = userId
                })
                .Where(data => !string.IsNullOrEmpty(data.day)) // Filter out entries without a date
                .OrderBy(data =>
                {
                    // Parse the day string to a DateTime object using the expected format
                    DateTime parsedDate = DateTime.ParseExact(data.day, "MM.dd.yyyy", CultureInfo.InvariantCulture);

                    // Combine with the time if available
                    TimeSpan timeSpan = !string.IsNullOrEmpty(data.time) ? TimeSpan.Parse(data.time) : TimeSpan.Zero;

                    return parsedDate.Add(timeSpan);
                }) // Sort by the combined DateTime
                .ToList();

            return Ok(dataList);
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }




    [FirestoreData]
        public class YourDataModel
        {

            [FirestoreProperty]
            public string userID { get; set; }
            [FirestoreProperty]
            public string day { get; set; }
            [FirestoreProperty]
            public string time { get; set; }
            [FirestoreProperty]
            public string name { get; set; }
            [FirestoreProperty]
            public string description { get; set; }
            [FirestoreProperty]
            public string importance { get; set; }
        }
    }
}
