using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Wrappers
{
    public class Response<T>
    {
        public Response()
        {
        }
        public Response(T data)
        {
            Succeeded = true;
            Message = string.Empty;
            Errors = new List<string>();
            Data = data;
        }
        public T Data { get; set; }
        public bool Succeeded { get; set; }
        public List<string> Errors { get; set; }
        public string Message { get; set; }
    }
}
