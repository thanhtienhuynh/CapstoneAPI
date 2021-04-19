﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets
{
    public class BaseResponse<T> where T: class
    {
        public bool IsSuccess { get; set; }
        public T Data { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
    }
}
