using CinemaProject.Models;

namespace Cinema_Project.Services
{
    public enum ProductImgType
    {
        MainImg,
        SubImg
    }
    public class MovieServices
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="img"></param>
        /// <param name="productImgType"></param>
        /// <returns></returns>
        public string? SavImg(IFormFile img, ProductImgType productImgType = ProductImgType.MainImg)
        {
            string filePath = string.Empty;
            try
            {
                var fileName = $"{DateTime.Now.ToString("dd_tt_yyyy")}{Guid.NewGuid()}{Path.GetExtension(img.FileName)}";
                switch (productImgType)
                {
                    case ProductImgType.MainImg:
                        {
                            filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images_Layout\\Admin\\MovieImage\\MovieMainImage", fileName);

                        }
                        break;
                    case ProductImgType.SubImg:
                        {
                            filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images_Layout\\Admin\\MovieImage\\MovieSubImages", fileName);
                        }
                        break;
                }
                using (var stream = System.IO.File.Create(filePath))
                {
                    img.CopyTo(stream);
                }
                return fileName;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imgName"></param>
        /// <param name="productImgType"></param>
        /// <returns></returns>
        public bool DeleteImg(string imgName, ProductImgType productImgType = ProductImgType.MainImg)
        {
            string filePath = string.Empty;
            try
            {
                switch (productImgType)
                {
                    case ProductImgType.MainImg:
                        {
                            filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images_Layout\\Admin\\MovieImage\\MovieMainImage", imgName);

                        }
                        break;
                    case ProductImgType.SubImg:
                        {
                            filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images_Layout\\Admin\\MovieImage\\MovieSubImages", imgName);
                        }
                        break;
                }
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
    }
}
