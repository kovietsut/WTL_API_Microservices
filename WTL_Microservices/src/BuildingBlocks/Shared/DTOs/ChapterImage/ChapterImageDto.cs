using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs.ChapterImage
{
    public class ChapterImageDto
    {
        public int CreatedBy { get; set; }
        public int ChapterId { get; set; }
        public string Name { get; set; }
        public string FileSize { get; set; }
        public string MimeType { get; set; }
        public string FilePath { get; set; }
    }
}
