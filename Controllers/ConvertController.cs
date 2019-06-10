using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Files = System.IO.File;

namespace ConvertImages.Controllers {
    [Route ("api/[controller]")]
    [ApiController]
    public class ConvertController : ControllerBase {
        [HttpPost]
        public async ValueTask<ActionResult> Post ([FromBody] HashSet<string> list) {
            var failList = new HashSet<string> ();
            foreach (var imgName in list) {
                var imgBasePath = $"{Directory.GetCurrentDirectory ()}/images/{imgName}/{imgName}";
                var horizonSize = $"{imgBasePath}";
                await SaveJsonFile (horizonSize, failList);

                var BigSize = $"{imgBasePath}m";
                await SaveJsonFile (BigSize, failList);

                var smallSize = $"{imgBasePath}m-s";
                await ResizeAndSave (280, 188, BigSize, smallSize);
                await SaveJsonFile (smallSize, failList);
            }
            return Ok (failList);
        }

        private async ValueTask ResizeAndSave (int width, int height, string oriImgPath, string destImgPath) {
            if (Files.Exists ($"{destImgPath}.jpg"))
                return;
            using (Image<Rgba32> image = Image.Load ($"{oriImgPath}.jpg")) {
                image.Mutate (x => x.Resize (width, height));
                image.Save ($"{destImgPath}.jpg");
            }
        }

        private async ValueTask SaveJsonFile (string picName, HashSet<string> failList) {
            if (!Files.Exists ($"{picName}.jpg")) {
                failList.Add ($"{picName}.jpg does not exist.");
                return;
            }
            var dataType = "data:image/jpeg;base64";
            byte[] imgdata = await Files.ReadAllBytesAsync ($"{picName}.jpg");
            string base64Str = Convert.ToBase64String (imgdata);
            using (StreamWriter file = Files.CreateText ($"{picName}.json")) {
                JsonSerializer serializer = new JsonSerializer ();
                serializer.Serialize (file, new { type = dataType, data = base64Str });
            }
        }
    }
}