using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Common
{
    public static class RegexHelper
    {
        public static readonly string PhoneNumberRegexVietNam = @"^(84|0[35789])([0-9]{8})$";
    }
}
