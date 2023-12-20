using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs.RedisCache
{
    public class ValueObject
    {
        public int Status { get; set; }
        public int IsSuccess { get; set; }
        public string Message { get; set; }
        public int DataCount { get; set; }
        public ExpandoObject Data { get; set; }
    }
}
