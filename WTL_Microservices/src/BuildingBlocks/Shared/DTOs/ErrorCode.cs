using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public class Status400
    {
        public string ConstraintViolation { get; set; }
        public string BadRequest { get; set; }
        public string Notfound { get; set; }
        public string SystemError { get; set; }
        public string FormatError { get; set; }
        public string APIClientError { get; set; }
    }

    public class Status401
    {
        public string Unauthorized { get; set; }
        public string ServerError { get; set; }
    }

    public class Status403
    {
        public string Forbidden { get; set; }
    }

    public class Status404
    {
        public string NotFound { get; set; }
    }

    public class Status409
    {
        public string Conflict { get; set; }
    }

    public class Status422
    {
        public string UnprocessableEntity { get; set; }
    }

    public class Status500
    {
        public string ServerError { get; set; }
        public string APIServerError { get; set; }
    }

    public class Status503
    {
        public string APINotAvailable { get; set; }
        public string ServiceUnavailable { get; set; }
    }

    public class ErrorCode
    {
        public Status400 Status400 { get; set; }
        public Status401 Status401 { get; set; }
        public Status403 Status403 { get; set; }
        public Status404 Status404 { get; set; }
        public Status409 Status409 { get; set; }
        public Status422 Status422 { get; set; }
        public Status500 Status500 { get; set; }
        public Status503 Status503 { get; set; }
    }
}
