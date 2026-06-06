namespace Cinema_Project.Services
{
    public class ActorServices
    {
        public string? SaveImg(IFormFile actorImg)
        {
            try
            {
                var fileName = $"{DateTime.Now.ToString("dd_tt_yyyy")}{Guid.NewGuid()}{Path.GetExtension(actorImg.FileName)}";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images_Layout\\Admin\\ActorImage", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    actorImg.CopyTo(stream);
                }
                return fileName;
            }

            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                Console.WriteLine($"Error saving image: {ex.Message}");
                return null;
            }
        }
        public bool DeleteImg(string imgName)
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images_Layout\\Admin\\ActorImage", imgName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                return true;
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                Console.WriteLine($"Error deleting image: {ex.Message}");
                return false;
            }
        }
    }
}
