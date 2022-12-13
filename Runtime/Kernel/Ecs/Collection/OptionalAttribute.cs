using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Morpheus.Ecs
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class OptionalComponentAttribute : System.Attribute {}
}