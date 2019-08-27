using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Files = System.IO.File;

namespace ConvertImages.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConvertController : ControllerBase
    {
        private readonly HashSet<string> _failList;
        public ConvertController()
        {
            _failList = new HashSet<string>();
        }

        [HttpPost]
        public async ValueTask<IActionResult> Post([FromBody] HashSet<string> list)
        {
            var sw = new Stopwatch();
            sw.Start();
            foreach (var imgName in list)
            {
                var imgBasePath = $"{Directory.GetCurrentDirectory()}/images/{imgName}/{imgName}";
                var horizonSize = $"{imgBasePath}";
                await SaveJsonFile(horizonSize);

                var BigSize = $"{imgBasePath}m";
                await SaveJsonFile(BigSize);

                var smallSize = $"{imgBasePath}m-s";
                ResizeAndSave(280, 188, BigSize, smallSize);
                await SaveJsonFile(smallSize);
            }
            sw.Stop();
            return _failList.Count == 0 ? Ok($"{DateTime.Now.ToShortDateString()} Total: {list.Count} Elapsed: {sw.ElapsedMilliseconds} ms") 
                                        : Ok(_failList);
        }

        private void ResizeAndSave(int width, int height, string oriImgPath, string destImgPath)
        {
            if (Files.Exists($"{destImgPath}.jpg"))
                return;
            try
            {
                using (Image<Rgba32> image = Image.Load($"{oriImgPath}.jpg"))
                {
                    image.Mutate(x => x.Resize(width, height));
                    image.Save($"{destImgPath}.jpg");
                }
            }
            catch (Exception e)
            {
                _failList.Add(e.Message);
                return;
            }
        }

        private async ValueTask SaveJsonFile(string picName)
        {
            if (!Files.Exists($"{picName}.jpg"))
            {
                _failList.Add($"{picName}.jpg does not exist.");
                return;
            }
            var dataType = "data:image/jpeg;base64";
            byte[] imgdata = await Files.ReadAllBytesAsync($"{picName}.jpg");
            string base64Str = Convert.ToBase64String(imgdata);
            try
            {
                using (StreamWriter file = Files.CreateText($"{picName}.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, new { type = dataType, data = base64Str });
                }
            }
            catch (Exception e)
            {
                _failList.Add(e.Message);
                return;
            }
        }
    }
}