﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Celery.DynamicProxy
{
    public interface IProxy
    {
        IInterceptor Intercepter { get; set; }
    }
}
