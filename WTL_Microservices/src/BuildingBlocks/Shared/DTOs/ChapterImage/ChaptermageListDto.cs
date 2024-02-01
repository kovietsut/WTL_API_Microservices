using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs.ChapterImage
{
    public class ChaptermageListDto
    {
        public List<ChapterImageDto>? ImageList { get; set; }
        public int ChapterId { get; set; }
    }
}
